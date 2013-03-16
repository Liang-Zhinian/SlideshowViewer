using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SlideshowViewer.Properties;

namespace SlideshowViewer
{
    public partial class DirectoryTree : Form
    {
        private readonly PictureViewer _pictureViewer;
        private decimal _minFileSize;

        public DirectoryTree(PictureViewer pictureViewer)
        {
            _pictureViewer = pictureViewer;
            Icon = Resources.image_x_generic;
            InitializeComponent();

            AutoRun = true;


            directoryTreeView.CanExpandGetter = Directory.CanExpandGetter;
            directoryTreeView.ChildrenGetter = Directory.ChildrenGetter;
            directoryTreeView.ItemActivate += DirectoryTreeViewOnItemActivate;
            directoryTreeView.KeyPress +=
                delegate(object sender, KeyPressEventArgs args) { args.Handled = args.KeyChar == '\r'; };
            Directory.AcceptFile = AcceptFile;
            Total.AspectGetter = rowObject => ((Directory) rowObject).GetNumberOfFiles();
            minSize.ValueChanged += MinSizeOnValueChanged;
            minSizeSuffix.SelectedValueChanged += MinSizeOnValueChanged;
            minSizeSuffix.Items.Clear();
            var bytes = new LabelledInt("bytes", 1);
            minSizeSuffix.Items.Add(bytes);
            minSizeSuffix.Items.Add(new LabelledInt("KB", 1024));
            minSizeSuffix.Items.Add(new LabelledInt("MB", 1024*1024));
            minSizeSuffix.SelectedItem = bytes;
            modifiedAfterSuffix.Items.Clear();
            modifiedAfterSuffix.Items.Add(new LabelledInt("seconds", 1));
            modifiedAfterSuffix.Items.Add(new LabelledInt("minutes", 60));
            modifiedAfterSuffix.Items.Add(new LabelledInt("hours", 60*60));
            var days = new LabelledInt("days", 60*60*24);
            modifiedAfterSuffix.Items.Add(days);
            modifiedAfterSuffix.Items.Add(new LabelledInt("weeks", 60*60*24*7));
            modifiedAfterSuffix.Items.Add(new LabelledInt("months", 60*60*24*30));
            modifiedAfterSuffix.Items.Add(new LabelledInt("years", 60*60*24*365));
            modifiedAfterSuffix.SelectedItem = days;
            modifiedAfterSuffix.SelectedValueChanged += ModifiedAfterSuffixOnSelectedValueChanged;
            modifiedAfter.ValueChanged += ModifiedAfterSuffixOnSelectedValueChanged;
        }

        protected decimal ModifiedAfter { get; set; }

        public decimal MinFileSize
        {
            get { return _minFileSize; }
            set { _minFileSize = value; }
        }

        public bool AutoRun { get; set; }

        private void ModifiedAfterSuffixOnSelectedValueChanged(object sender, EventArgs eventArgs)
        {
            ModifiedAfter = modifiedAfter.Value*((LabelledInt) modifiedAfterSuffix.SelectedItem).Value;
            RefreshTree();
        }

        private void RefreshTree()
        {
            directoryTreeView.RebuildAll(true);
            Total.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            Total.Width += 20;
        }

        private void MinSizeOnValueChanged(object sender, EventArgs eventArgs)
        {
            MinFileSize = minSize.Value*((LabelledInt) minSizeSuffix.SelectedItem).Value;
            RefreshTree();
        }

        private bool AcceptFile(PictureFile file)
        {
            if (file.FileSize < MinFileSize)
                return false;

            if (ModifiedAfter > 0 &&
                DateTime.Now.AddSeconds((double) (0 - ModifiedAfter)).CompareTo(file.ModifiedDate) > 0)
                return false;

            return true;
        }


        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            foreach (object item in minSizeSuffix.Items)
            {
                decimal value = _minFileSize/((LabelledInt) item).Value;
                if (value < 1024)
                {
                    minSize.Value = value;
                    minSizeSuffix.SelectedItem = item;
                    break;
                }
            }
            if (AutoRun)
            {
                ShowPictures(
                    directoryTreeView.Objects.Cast<Directory>().SelectMany(directory => directory.GetFilesRecursive()));
            }
            RefreshTree();
        }

        private void ShowPictures(IEnumerable<PictureFile> pictureFiles)
        {
            _pictureViewer.ShowPictures(pictureFiles);
        }


        private IEnumerable<PictureFile> GetSelectedFiles()
        {
            return
                directoryTreeView.SelectedObjects.Cast<Directory>()
                                 .SelectMany(directory => directory.GetFilesRecursive());
        }

        private void DirectoryTreeViewOnItemActivate(object sender, EventArgs eventArgs)
        {
            ShowPictures(GetSelectedFiles());
        }


        public void AddBaseDir(string baseDir)
        {
            directoryTreeView.AddObject(new Directory(baseDir));
        }

        public void AddFile(PictureFile file)
        {
            foreach (object o in directoryTreeView.Objects)
            {
                if (((Directory) o).AddFile(file))
                    return;
            }
        }

        private class LabelledInt
        {
            public LabelledInt(string text, int value)
            {
                Text = text;
                Value = value;
            }

            public string Text { get; set; }
            public int Value { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }
    }
}