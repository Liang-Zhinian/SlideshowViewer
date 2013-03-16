using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows.Forms;
using System.Linq;
using Timer = System.Timers.Timer;

namespace SlideshowViewer
{
    public partial class PictureViewerForm : Form
    {
        private readonly Timer _preLoadTimer;
        private Timer _slideShowTimer;

        public delegate void PictureShownDelegate(PictureFile file);
        public delegate void AllPicturesShownDelegate(IEnumerable<PictureFile> file);

        public PictureShownDelegate PictureShown { get; set; }
        public AllPicturesShownDelegate AllPicturesShown { get; set; }

        public PictureViewerForm()
        {
            DelayInSec = 15;
            Loop = false;
            OverlayTextTemplate = "{fullName}";
            InitializeComponent();
            _preLoadTimer = new Timer(1000);
            _preLoadTimer.SynchronizingObject = this;
            _preLoadTimer.Elapsed += PreLoadTimerOnElapsed;
            _preLoadTimer.AutoReset = false;            
        }

        public List<PictureFile> Files { private get; set; }

        public int FileIndex { get; set; }

        public int DelayInSec { get; set; }

        public bool Loop { get; set; }

        public string OverlayTextTemplate { get; set; }

        [DllImport("kernel32.dll")]
        private static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

        private void PreLoadTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            for (int i = 0; i < Files.Count; i++)
            {
                PictureFile pictureFile = Files[i];
                if (i < FileIndex - 3 || i > FileIndex + 3)
                    pictureFile.UnloadImage();
                if (i >= FileIndex - 1 && i <= FileIndex + 1)
                    pictureFile.GetImage();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            WindowState = FormWindowState.Normal;
            FormBorderStyle = FormBorderStyle.None;
            Bounds = Screen.PrimaryScreen.Bounds;
            pictureBox1.MouseEnter += (o, args) => Cursor.Hide();
            pictureBox1.MouseLeave += (o, args) => Cursor.Show();
            ShowPicture();
        }


        private void ShowPicture()
        {
            if (FileIndex >= Files.Count)
            {

                if (Loop)
                {
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

            PictureFile file = Files[FileIndex];
            Image bitmap = file.Image;
            int imageDuration = file.GetImageDuration();

            pictureBox1.HighQuality = imageDuration == 0;
            pictureBox1.Image = bitmap;
            pictureBox1.LowerLeftText = GetOverlayText(file, OverlayTextTemplate);
            StopSlideShowTimer();
            int interval = Math.Max(DelayInSec*1000, imageDuration + imageDuration/2);
            pictureBox1.LowerMiddleText = null;
            StartSlideShowTimer(interval);
            if (PictureShown != null)
            {
                PictureShown(file);
            }
            if (FileIndex == Files.Count - 1 && AllPicturesShown != null)
            {
                AllPicturesShown(Files);
            }
            _preLoadTimer.Stop();
            if (!file.IsAnimatedGif())
                _preLoadTimer.Start();
        }

        private string GetOverlayText(PictureFile file, string template)
        {
            template = template.Replace("{eol}", "\n");
            template = template.Replace("{fullName}", file.FileName);
            template = template.Replace("{imageDescription}", file.GetImageDescription());
            template = template.Replace("{description}", file.GetDescription());
            template = template.Replace("{dateTime}", file.GetDateTime());
            template = template.Replace("{model}", file.GetModel());
            template = template.Replace("{index}", (FileIndex + 1).ToString("n0"));
            template = template.Replace("{total}", Files.Count.ToString("n0"));
            return string.Join("\n", template.SplitIntoLines().Where(s => s.Length > 0));
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

        private void StartSlideShowTimer(int interval)
        {
            _slideShowTimer = new Timer(interval);
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

        private void MarkFile(int i, PictureFile pictureFile)
        {
            using (File.Create(pictureFile.FileName + ".ssv." + i))
            {
            }
            pictureBox1.LowerMiddleText = "MARKED " + i;
        }

        private void Pause()
        {
            if (_slideShowTimer.Enabled)
            {
                _slideShowTimer.Stop();
                pictureBox1.LowerMiddleText = "PAUSED";
            }
            else
            {
                _slideShowTimer.Start();
                pictureBox1.LowerMiddleText = null;
            }
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
            foreach (PictureFile pictureFile in Files)
            {
                pictureFile.UnloadImage();
            }            

        }
    }
}