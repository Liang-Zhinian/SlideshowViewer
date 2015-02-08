using System;
using System.Drawing;
using System.Windows.Forms;

namespace SlideshowViewer
{
    public partial class SplashScreen : Form
    {
        public SplashScreen()
        {
            InitializeComponent();
            WindowState = FormWindowState.Normal;
            FormBorderStyle = FormBorderStyle.None;
            var graphicsUnit = GraphicsUnit.Display;
            Bounds = Rectangle.Truncate(BackgroundImage.GetBounds(ref graphicsUnit));
            StartPosition = FormStartPosition.CenterScreen;
            Cursor = Cursors.AppStarting;
        }

        private void SplashScreen_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}