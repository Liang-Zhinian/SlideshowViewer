using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            this.StartPosition = FormStartPosition.CenterScreen;
            Cursor=Cursors.AppStarting;
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
