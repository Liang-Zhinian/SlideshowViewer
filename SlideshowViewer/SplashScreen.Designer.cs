namespace SlideshowViewer
{
    partial class SplashScreen
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.numberOfFilesScanned = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // numberOfFilesScanned
            // 
            this.numberOfFilesScanned.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.numberOfFilesScanned.Location = new System.Drawing.Point(210, 232);
            this.numberOfFilesScanned.MinimumSize = new System.Drawing.Size(60, 0);
            this.numberOfFilesScanned.Name = "numberOfFilesScanned";
            this.numberOfFilesScanned.Size = new System.Drawing.Size(79, 15);
            this.numberOfFilesScanned.TabIndex = 1;
            this.numberOfFilesScanned.Text = "0";
            this.numberOfFilesScanned.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(210, 250);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(79, 33);
            this.button1.TabIndex = 2;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // SplashScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackgroundImage = global::SlideshowViewer.Properties.Resources.Splash;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(301, 295);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.numberOfFilesScanned);
            this.Cursor = System.Windows.Forms.Cursors.Help;
            this.Name = "SplashScreen";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Slideshow Viewer";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.SplashScreen_Load);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.Label numberOfFilesScanned;
        private System.Windows.Forms.Button button1;

    }
}