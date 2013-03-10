using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace SlideshowViewer
{


    public partial class PictureViewerForm : Form
    {
        [DllImport("kernel32.dll")]
        static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

        private Timer _slideShowTimer;

        private Timer _preLoadTimer;

        public PictureViewerForm()
        {
            DelayInSec = 15;
            Loop = false;
            OverlayTextTemplate = "{fullName}";
            InitializeComponent();
            _preLoadTimer=new Timer(1000);
            _preLoadTimer.SynchronizingObject = this;
            _preLoadTimer.Elapsed+=PreLoadTimerOnElapsed;
            _preLoadTimer.AutoReset = false;
        }

        private void PreLoadTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            for (int i = 0; i < Files.Count; i++)
            {
                var pictureFile = Files[i];
                if (i < FileIndex - 3 || i > FileIndex + 3)
                    pictureFile.UnloadImage();
                if (i >= FileIndex - 1 && i <= FileIndex + 1)
                    pictureFile.GetImage();
            }
        }

        public List<PictureFile> Files { private get; set; }

        public int FileIndex { get; set; }

        public int DelayInSec { get; set; }

        public bool Loop { get; set; }

        public string OverlayTextTemplate { get; set; }

        public string ResumeFile { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ShowPictures();
        }


        public void ShowPictures()
        {
            ShowPicture(FileIndex);
            WindowState = FormWindowState.Normal;
            FormBorderStyle = FormBorderStyle.None;
            Bounds = Screen.PrimaryScreen.Bounds;
            pictureBox1.MouseEnter += (o, args) => Cursor.Hide();
            pictureBox1.MouseLeave += (o, args) => Cursor.Show();
        }

        private void ShowPicture(int index)
        {
            var file = Files[index];
            pictureBox1.LowerMiddleText = "...";
            Image bitmap = file.Image;
            int imageDuration = file.GetImageDuration();

            pictureBox1.HighQuality = imageDuration == 0;
            pictureBox1.Image = bitmap;
            pictureBox1.LowerLeftText = GetOverlayText(file, OverlayTextTemplate);
            StopSlideShowTimer();
            int interval = Math.Max(DelayInSec*1000, imageDuration + imageDuration/2);
            pictureBox1.LowerMiddleText = null;
            StartSlideShowTimer(interval);
            RegisterFileForResume(file.FileName);
            _preLoadTimer.Stop();
            _preLoadTimer.Start();
        }

        private string GetOverlayText(PictureFile file,string template)
        {
            template = template.Replace("{eol}", "\n");
            template = template.Replace("{fullName}", file.FileName);
            template = template.Replace("{description}", file.GetDescription());
            template = template.Replace("{dateTime}", file.GetDateTime());
            template = template.Replace("{model}", file.GetModel());
            template = template.Replace("{index}", (FileIndex + 1).ToString("n0"));
            template = template.Replace("{total}", Files.Count.ToString("n0"));
            return template;
        }

        private void StopSlideShowTimer()
        {
            if (_slideShowTimer != null)
            {
                _slideShowTimer.Stop();
                _slideShowTimer.Dispose();
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

        private void MarkFile(int i, PictureFile pictureFile)
        {
            using (File.Create(pictureFile.FileName + ".ssv." + i)) { }
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
            if (FileIndex >= Files.Count)
            {
                if (Loop)
                {
                    if (ResumeFile != null)
                        File.Delete(ResumeFile);
                    FileIndex = 0;
                }
                else
                {
                    Close();
                    return;
                }
            }
            ShowPicture(FileIndex);
        }

        private void PrevPicture()
        {
            FileIndex--;
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
            ShowPicture(FileIndex);
        }

        private void SlideShowTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (sender == _slideShowTimer)
                NextPicture();
        }

        private void RegisterFileForResume(string fileName)
        {
            if (ResumeFile != null)
                File.AppendAllLines(ResumeFile, new[] {fileName});
        }
    }
}