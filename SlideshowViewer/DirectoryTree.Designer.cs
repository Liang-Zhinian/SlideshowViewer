namespace SlideshowViewer
{
    partial class DirectoryTree
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
            this.directoryTreeView = new BrightIdeasSoftware.TreeListView();
            this.DirectoryName = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.Total = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.modifiedAfterSuffix = new System.Windows.Forms.ComboBox();
            this.modifiedAfter = new System.Windows.Forms.NumericUpDown();
            this.minSize = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.minSizeSuffix = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.directoryTreeView)).BeginInit();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.modifiedAfter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.minSize)).BeginInit();
            this.SuspendLayout();
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
            this.directoryTreeView.Location = new System.Drawing.Point(0, 0);
            this.directoryTreeView.Name = "directoryTreeView";
            this.directoryTreeView.OwnerDraw = true;
            this.directoryTreeView.ShowGroups = false;
            this.directoryTreeView.Size = new System.Drawing.Size(858, 796);
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
            // panel1
            // 
            this.panel1.Controls.Add(this.tableLayoutPanel1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(858, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(249, 796);
            this.panel1.TabIndex = 2;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 105F));
            this.tableLayoutPanel1.Controls.Add(this.modifiedAfterSuffix, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.modifiedAfter, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.minSize, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.minSizeSuffix, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(249, 796);
            this.tableLayoutPanel1.TabIndex = 0;
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
            this.modifiedAfterSuffix.Location = new System.Drawing.Point(147, 30);
            this.modifiedAfterSuffix.MinimumSize = new System.Drawing.Size(40, 0);
            this.modifiedAfterSuffix.Name = "modifiedAfterSuffix";
            this.modifiedAfterSuffix.Size = new System.Drawing.Size(99, 21);
            this.modifiedAfterSuffix.TabIndex = 5;
            // 
            // modifiedAfter
            // 
            this.modifiedAfter.AutoSize = true;
            this.modifiedAfter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modifiedAfter.Location = new System.Drawing.Point(94, 30);
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
            this.minSize.Location = new System.Drawing.Point(94, 3);
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
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 27);
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
            this.minSizeSuffix.Location = new System.Drawing.Point(147, 3);
            this.minSizeSuffix.MinimumSize = new System.Drawing.Size(40, 0);
            this.minSizeSuffix.Name = "minSizeSuffix";
            this.minSizeSuffix.Size = new System.Drawing.Size(99, 21);
            this.minSizeSuffix.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "Modified after";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // DirectoryTree
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1107, 796);
            this.Controls.Add(this.directoryTreeView);
            this.Controls.Add(this.panel1);
            this.Name = "DirectoryTree";
            this.Text = "Slideshow Viewer";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)(this.directoryTreeView)).EndInit();
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.modifiedAfter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.minSize)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private BrightIdeasSoftware.TreeListView directoryTreeView;
        private BrightIdeasSoftware.OLVColumn DirectoryName;
        private BrightIdeasSoftware.OLVColumn Total;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown minSize;
        private System.Windows.Forms.ComboBox minSizeSuffix;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox modifiedAfterSuffix;
        private System.Windows.Forms.NumericUpDown modifiedAfter;


    }
}