using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace SlideshowViewer
{
    public partial class PictureViewerForm : Form
    {
        private Timer _timer;

        public PictureViewerForm()
        {
            DelayInSec = 15;
            Loop = false;
            OverlayTextTemplate = "{fullName}";
            InitializeComponent();
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
            StopTimer();
            int interval = Math.Max(DelayInSec*1000, imageDuration + imageDuration/2);
            pictureBox1.LowerMiddleText = null;
            StartTimer(interval);
            RegisterFileForResume(file.FileName);
            for (int i = 0; i < Files.Count; i++)
            {
                var pictureFile = Files[i];
                if (i < FileIndex - 1 || i > FileIndex + 1)
                    pictureFile.UnloadImage();
                else
                {
                    pictureFile.GetImage();
                }
            }
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

        private void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
            } 
        }

        private void StartTimer(int interval)
        {
            _timer = new Timer(interval);
            _timer.Elapsed += TimerOnElapsed;
            _timer.SynchronizingObject = this;
            _timer.Start();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyValue == (decimal) Keys.Right || e.KeyValue == (decimal) Keys.PageUp)
            {
                NextPicture();
            }
            if (e.KeyValue == (decimal) Keys.Left || e.KeyValue == (decimal) Keys.PageDown)
            {
                PrevPicture();
            }
            if (e.KeyValue == (decimal) Keys.Escape)
            {
                Close();
            }
            if (e.KeyValue == (decimal) Keys.Space || e.KeyValue == (decimal) Keys.Enter)
            {
                Pause();
            }
            base.OnKeyDown(e);
        }

        private void Pause()
        {
            if (_timer.Enabled)
            {
                _timer.Stop();
                pictureBox1.LowerMiddleText = "PAUSED";
            }
            else
            {
                _timer.Start();
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

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (sender == _timer)
                NextPicture();
        }

        private void RegisterFileForResume(string fileName)
        {
            if (ResumeFile != null)
                File.AppendAllLines(ResumeFile, new[] {fileName});
        }
    }
}