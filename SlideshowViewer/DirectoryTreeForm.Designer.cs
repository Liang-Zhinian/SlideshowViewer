namespace SlideshowViewer
{
    partial class DirectoryTreeForm
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
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.loop = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.modifiedAfterSuffix = new System.Windows.Forms.ComboBox();
            this.modifiedAfter = new System.Windows.Forms.NumericUpDown();
            this.minSize = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.minSizeSuffix = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.shuffle = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.delay = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.browse = new System.Windows.Forms.CheckBox();
            this.directoryTreeView = new BrightIdeasSoftware.TreeListView();
            this.DirectoryName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.Total = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.panel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.modifiedAfter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.minSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.delay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.directoryTreeView)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tableLayoutPanel1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(816, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(291, 796);
            this.panel1.TabIndex = 2;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.browse, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.label6, 2, 8);
            this.tableLayoutPanel1.Controls.Add(this.delay, 1, 8);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.loop, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.modifiedAfterSuffix, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.modifiedAfter, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.minSize, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.minSizeSuffix, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.shuffle, 0, 7);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 10;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(291, 796);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // loop
            // 
            this.loop.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.loop, 3);
            this.loop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.loop.Location = new System.Drawing.Point(3, 91);
            this.loop.Name = "loop";
            this.loop.Size = new System.Drawing.Size(285, 17);
            this.loop.TabIndex = 9;
            this.loop.Text = "Loop";
            this.loop.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.label4, 3);
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(3, 71);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(285, 17);
            this.label4.TabIndex = 7;
            this.label4.Text = "Options";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // modifiedAfterSuffix
            // 
            this.modifiedAfterSuffix.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modifiedAfterSuffix.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.modifiedAfterSuffix.FormattingEnabled = true;
            this.modifiedAfterSuffix.Items.AddRange(new object[] {
            "bytes",
            "KB",
            "MB"});
            this.modifiedAfterSuffix.Location = new System.Drawing.Point(183, 47);
            this.modifiedAfterSuffix.MinimumSize = new System.Drawing.Size(40, 0);
            this.modifiedAfterSuffix.Name = "modifiedAfterSuffix";
            this.modifiedAfterSuffix.Size = new System.Drawing.Size(105, 21);
            this.modifiedAfterSuffix.TabIndex = 5;
            // 
            // modifiedAfter
            // 
            this.modifiedAfter.AutoSize = true;
            this.modifiedAfter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modifiedAfter.Location = new System.Drawing.Point(130, 47);
            this.modifiedAfter.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.modifiedAfter.Name = "modifiedAfter";
            this.modifiedAfter.Size = new System.Drawing.Size(47, 20);
            this.modifiedAfter.TabIndex = 4;
            // 
            // minSize
            // 
            this.minSize.AutoSize = true;
            this.minSize.Dock = System.Windows.Forms.DockStyle.Fill;
            this.minSize.Location = new System.Drawing.Point(130, 20);
            this.minSize.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.minSize.Name = "minSize";
            this.minSize.Size = new System.Drawing.Size(47, 20);
            this.minSize.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(121, 27);
            this.label1.TabIndex = 0;
            this.label1.Text = "Minimum file size";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // minSizeSuffix
            // 
            this.minSizeSuffix.Dock = System.Windows.Forms.DockStyle.Fill;
            this.minSizeSuffix.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.minSizeSuffix.FormattingEnabled = true;
            this.minSizeSuffix.Items.AddRange(new object[] {
            "bytes",
            "KB",
            "MB"});
            this.minSizeSuffix.Location = new System.Drawing.Point(183, 20);
            this.minSizeSuffix.MinimumSize = new System.Drawing.Size(40, 0);
            this.minSizeSuffix.Name = "minSizeSuffix";
            this.minSizeSuffix.Size = new System.Drawing.Size(105, 21);
            this.minSizeSuffix.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(121, 27);
            this.label2.TabIndex = 3;
            this.label2.Text = "Maximum file age";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.label3, 3);
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(285, 17);
            this.label3.TabIndex = 6;
            this.label3.Text = "Filter";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // shuffle
            // 
            this.shuffle.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.shuffle, 3);
            this.shuffle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.shuffle.Location = new System.Drawing.Point(3, 154);
            this.shuffle.Name = "shuffle";
            this.shuffle.Size = new System.Drawing.Size(285, 17);
            this.shuffle.TabIndex = 8;
            this.shuffle.Text = "Shuffle";
            this.shuffle.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(3, 174);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(121, 26);
            this.label5.TabIndex = 10;
            this.label5.Text = "Each picture i shown for";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // delay
            // 
            this.delay.AutoSize = true;
            this.delay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.delay.Location = new System.Drawing.Point(130, 177);
            this.delay.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.delay.Name = "delay";
            this.delay.Size = new System.Drawing.Size(47, 20);
            this.delay.TabIndex = 11;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.Location = new System.Drawing.Point(183, 174);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(105, 26);
            this.label6.TabIndex = 12;
            this.label6.Text = "seconds";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.label7, 3);
            this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(3, 134);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(285, 17);
            this.label7.TabIndex = 13;
            this.label7.Text = "Slideshow Options";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // browse
            // 
            this.browse.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.browse, 3);
            this.browse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.browse.Location = new System.Drawing.Point(3, 114);
            this.browse.Name = "browse";
            this.browse.Size = new System.Drawing.Size(285, 17);
            this.browse.TabIndex = 14;
            this.browse.Text = "Browse";
            this.browse.UseVisualStyleBackColor = true;
            // 
            // directoryTreeView
            // 
            this.directoryTreeView.AllColumns.Add(this.DirectoryName);
            this.directoryTreeView.AllColumns.Add(this.Total);
            this.directoryTreeView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.DirectoryName,
            this.Total});
            this.directoryTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.directoryTreeView.Font = new System.Drawing.Font("Verdana", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.directoryTreeView.FullRowSelect = true;
            this.directoryTreeView.Location = new System.Drawing.Point(0, 0);
            this.directoryTreeView.Name = "directoryTreeView";
            this.directoryTreeView.OwnerDraw = true;
            this.directoryTreeView.ShowGroups = false;
            this.directoryTreeView.Size = new System.Drawing.Size(816, 796);
            this.directoryTreeView.TabIndex = 1;
            this.directoryTreeView.UseCompatibleStateImageBehavior = false;
            this.directoryTreeView.UseTranslucentSelection = true;
            this.directoryTreeView.View = System.Windows.Forms.View.Details;
            this.directoryTreeView.VirtualMode = true;
            // 
            // DirectoryName
            // 
            this.DirectoryName.AspectName = "Name";
            this.DirectoryName.CellPadding = null;
            this.DirectoryName.FillsFreeSpace = true;
            this.DirectoryName.Text = "Name";
            // 
            // Total
            // 
            this.Total.AspectToStringFormat = "{0:n0}";
            this.Total.CellPadding = null;
            this.Total.HeaderTextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.Total.Text = "Total";
            this.Total.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // DirectoryTreeForm
            // 
            this.AccessibleDescription = "";
            this.AccessibleName = "DirectoryTreeForm";
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1107, 796);
            this.Controls.Add(this.directoryTreeView);
            this.Controls.Add(this.panel1);
            this.Name = "DirectoryTreeForm";
            this.Text = "Slideshow Viewer";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.modifiedAfter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.minSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.delay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.directoryTreeView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        internal BrightIdeasSoftware.TreeListView directoryTreeView;
        private BrightIdeasSoftware.OLVColumn DirectoryName;
        public BrightIdeasSoftware.OLVColumn Total;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.NumericUpDown minSize;
        public System.Windows.Forms.ComboBox minSizeSuffix;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.ComboBox modifiedAfterSuffix;
        public System.Windows.Forms.NumericUpDown modifiedAfter;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.CheckBox loop;
        public System.Windows.Forms.CheckBox shuffle;
        public System.Windows.Forms.CheckBox browse;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        public System.Windows.Forms.NumericUpDown delay;
        private System.Windows.Forms.Label label5;


    }
}