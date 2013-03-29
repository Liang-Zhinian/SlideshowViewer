using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace SlideshowViewer
{
    internal enum InfoState
    {
        Template,
        Hidden,
        Full
    }

    public partial class PictureViewerForm : Form
    {
        #region Delegates

        public delegate void AllPicturesShownDelegate(IEnumerable<PictureFile.PictureFile> file);

        public delegate void PictureShownDelegate(PictureFile.PictureFile file);

        #endregion

        private readonly Timer _preLoadTimer;
        private Timer _slideShowTimer;

        public PictureViewerForm()
        {
            DelayInSec = 15;
            Loop = false;
            Paused = false;
            InfoState = InfoState.Template;
            OverlayTextTemplate = "{fullName}";
            InitializeComponent();
            _preLoadTimer = new Timer(1);
            _preLoadTimer.SynchronizingObject = this;
            _preLoadTimer.Elapsed += PreLoadTimerOnElapsed;
            _preLoadTimer.AutoReset = false;

            WindowState = FormWindowState.Normal;
            FormBorderStyle = FormBorderStyle.None;
            Bounds = Screen.PrimaryScreen.Bounds;
            pictureBox1.MouseEnter += (o, args) => Cursor.Hide();
            pictureBox1.MouseLeave += (o, args) => Cursor.Show();
        }

        public PictureShownDelegate PictureShown { get; set; }
        public AllPicturesShownDelegate AllPicturesShown { get; set; }

        public List<PictureFile.PictureFile> Files { get; set; }

        public int FileIndex { get; set; }

        public decimal DelayInSec { get; set; }

        public bool Loop { get; set; }

        public string OverlayTextTemplate { get; set; }

        public Action<PictureFile.PictureFile> ToggleBrowsing { get; set; }

        public bool Browsing { get; set; }

        private bool Paused { get; set; }

        private InfoState InfoState { get; set; }

        [DllImport("kernel32.dll")]
        private static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

        private void PreLoadTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            for (int i = 0; i < Files.Count; i++)
            {
                PictureFile.PictureFile pictureFile = Files[i];
                if ((i < FileIndex - 3 || i > FileIndex + 3))
                    pictureFile.UnloadImage();
                if (i >= FileIndex - 1 && i <= FileIndex + 1)
                    pictureFile.LoadImage();
            }
        }


        internal void ShowPicture()
        {
            if (FileIndex >= Files.Count)
            {
                if (Loop)
                {
                    if (AllPicturesShown != null)
                    {
                        AllPicturesShown(Files);
                    }
                    FileIndex = 0;
                }
                else
                {
                    Close();
                    return;
                }
            }
            if (FileIndex < 0)
            {
                if (Loop)
                    FileIndex = Files.Count - 1;
                else
                {
                    Close();
                    return;
                }
            }

            PictureFile.PictureFile file = Files[FileIndex];
            Image bitmap = file.Data.Image;
            int imageDuration = file.Data.ImageDuration;

            pictureBox1.HighQuality = imageDuration == 0;
            pictureBox1.Image = bitmap;
            string lowerRightText = null;

            StopSlideShowTimer();
            decimal interval = Math.Max(DelayInSec*1000, imageDuration + imageDuration/2);
            if (interval == 0)
                interval = 1;
            if (!Browsing)
            {
                if (!Paused)
                {
                    StartSlideShowTimer(interval);
                }
                else
                {
                    lowerRightText = "PAUSED";
                }
                if (PictureShown != null)
                {
                    PictureShown(file);
                }
            }
            else
            {
                lowerRightText = "BROWSING";
            }
            switch (InfoState)
            {
                case InfoState.Template:
                    pictureBox1.LowerRightText = lowerRightText;
                    pictureBox1.LowerLeftText = GetOverlayText(file, OverlayTextTemplate);
                    break;
                case InfoState.Hidden:
                    pictureBox1.LowerRightText = null;
                    pictureBox1.LowerLeftText = null;
                    break;
                case InfoState.Full:
                    pictureBox1.LowerRightText = lowerRightText;
                    pictureBox1.LowerLeftText = String.Join("\n",
                                                            file.Data.Properties.Select(
                                                                pair => pair.Key + ": '" + pair.Value + "'"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _preLoadTimer.Stop();
            if (file.Data.ImageDuration == 0)
                _preLoadTimer.Start();
        }

        private string GetOverlayText(PictureFile.PictureFile file, string template)
        {
            string overlayText = Regex.Replace(template, @"\{(.*?)\}",
                                               match => GetReplacement(match.Groups[1].ToString().ToLower()));
            return string.Join("\n", overlayText.SplitIntoLines().Where(s => s.Length > 0));
        }

        private string GetReplacement(string match)
        {
            switch (match)
            {
                case "nl":
                case "eol":
                    return "\n";
                case "index":
                    return (FileIndex + 1).ToString("n0");
                case "total":
                    return Files.Count.ToString("n0");
                default:
                    return Files[FileIndex].Data.Get(match, "").ToString().Trim();
            }
        }

        private void StopSlideShowTimer()
        {
            if (_slideShowTimer != null)
            {
                _slideShowTimer.Stop();
                _slideShowTimer.Dispose();
                _slideShowTimer = null;
            }
        }

        private void StartSlideShowTimer(decimal interval)
        {
            _slideShowTimer = new Timer((double) interval);
            _slideShowTimer.Elapsed += SlideShowTimerOnElapsed;
            _slideShowTimer.SynchronizingObject = this;
            _slideShowTimer.Start();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Right:
                case Keys.PageUp:
                    NextPicture();
                    break;
                case Keys.Left:
                case Keys.PageDown:
                    PrevPicture();
                    break;
                case Keys.Escape:
                    Close();
                    break;
                case Keys.Space:
                case Keys.Enter:
                    Pause();
                    break;
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (e.KeyChar >= '0' && e.KeyChar <= '9')
                MarkFile(e.KeyChar - '0', Files[FileIndex]);
            if (e.KeyChar == 'b' && ToggleBrowsing != null)
                ToggleBrowsing(Files[FileIndex]);
            if (e.KeyChar == 'i')
            {
                InfoState = (InfoState) (((int) InfoState + 1)%Enum.GetValues(typeof (InfoState)).Length);
                ShowPicture();
            }
            if (e.KeyChar == 'q')
            {
                Files[FileIndex].RotateLeft();
                ShowPicture();
            }
            if (e.KeyChar == 'e')
            {
                Files[FileIndex].RotateRight();
                ShowPicture();
            }

            base.OnKeyPress(e);
        }


        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                PrevPicture();
            }
            else
            {
                NextPicture();
            }
            base.OnMouseWheel(e);
        }

        private void MarkFile(int i, PictureFile.PictureFile pictureFile)
        {
            using (File.Create(pictureFile.FileName + ".ssv." + i))
            {
            }
            pictureBox1.LowerRightText = "MARKED " + i;
        }

        private void Pause()
        {
            if (Browsing)
                return;
            Paused = !Paused;
            ShowPicture();
        }

        private void NextPicture()
        {
            FileIndex++;
            ShowPicture();
        }

        private void PrevPicture()
        {
            FileIndex--;
            ShowPicture();
        }

        private void SlideShowTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (sender == _slideShowTimer)
                NextPicture();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            StopSlideShowTimer();
            foreach (PictureFile.PictureFile pictureFile in Files)
            {
                pictureFile.UnloadImage();
            }
        }
    }
}