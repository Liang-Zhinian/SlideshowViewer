using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SlideshowViewer.Properties;

namespace SlideshowViewer
{
    public partial class DirectoryTree : Form
    {
        private decimal _minFileSize;
        private decimal _modifiedAfter;

        public DirectoryTree()
        {
            Icon = Resources.image_x_generic;
            InitializeComponent();

            ResumeManager = new DummyResumeManager();
            Shuffle = false;
            Loop = true;
            DelayInSec = 15;
            Text =
                "{imageDescription}{eol}{description}{eol}{dateTime}{eol}{model}{eol}{fullName}  ( {index} / {total} )";

            AutoRun = true;


            directoryTreeView.CanExpandGetter = FileGroup.CanExpandGetter;
            directoryTreeView.ChildrenGetter = FileGroup.ChildrenGetter;
            directoryTreeView.ItemActivate += DirectoryTreeViewOnItemActivate;
            directoryTreeView.KeyPress +=
                delegate(object sender, KeyPressEventArgs args) { args.Handled = args.KeyChar == '\r'; };
            Total.AspectGetter = rowObject => ((FileGroup) rowObject).GetNumberOfFiles();
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

        public ResumeManager ResumeManager { get; set; }

        public bool Shuffle
        {
            get { return shuffle.Checked; }
            set { shuffle.Checked = value; }
        }

        public bool Loop
        {
            get { return loop.Checked; }
            set { loop.Checked = value; }
        }

        public decimal DelayInSec
        {
            get { return delay.Value; }
            set { delay.Value = value; }
        }

        public string Text { get; set; }

        protected decimal ModifiedAfter
        {
            get { return _modifiedAfter; }
            set
            {
                _modifiedAfter = value;
                UpdateFilter();
            }
        }

        public decimal MinFileSize
        {
            get { return _minFileSize; }
            set
            {
                _minFileSize = value;
                UpdateFilter();
            }
        }

        public bool AutoRun { get; set; }


        public void ShowPictures(IEnumerable<PictureFile> files)
        {
            List<PictureFile> pictureFiles;
            int index = PrepareFileList(files, out pictureFiles);

            using (PictureViewerForm pictureViewerForm = CreatePictureViewForm(pictureFiles, index))
            {
                pictureViewerForm.ShowDialog();
            }
        }

        private int PrepareFileList(IEnumerable<PictureFile> files, out List<PictureFile> pictureFiles)
        {
            var shownFiles = new List<PictureFile>();
            var notShownFiles = new List<PictureFile>();

            foreach (PictureFile file in files)
            {
                if (ResumeManager.IsShown(file))
                    shownFiles.Add(file);
                else
                    notShownFiles.Add(file);
            }

            if (notShownFiles.Count == 0)
            {
                ResumeManager.SetToNotShown(shownFiles);
                notShownFiles = shownFiles;
                shownFiles = new List<PictureFile>();
            }

            pictureFiles = new List<PictureFile>();
            if (Shuffle)
            {
                pictureFiles.AddRange(shownFiles.Shuffle());
                pictureFiles.AddRange(notShownFiles.Shuffle());
            }
            else
            {
                pictureFiles.AddRange(shownFiles);
                pictureFiles.AddRange(notShownFiles);
            }
            return shownFiles.Count;
        }


        private PictureViewerForm CreatePictureViewForm(List<PictureFile> pictureFiles, int fileIndex)
        {
            var pictureViewerForm = new PictureViewerForm();
            pictureViewerForm.Files = pictureFiles;
            pictureViewerForm.FileIndex = fileIndex;
            pictureViewerForm.Loop = Loop;
            pictureViewerForm.DelayInSec = DelayInSec;
            pictureViewerForm.OverlayTextTemplate = Text;
            pictureViewerForm.Icon = Resources.image_x_generic;
            pictureViewerForm.PictureShown = ResumeManager.SetToShown;
            pictureViewerForm.AllPicturesShown = ResumeManager.SetToNotShown;
            return pictureViewerForm;
        }


        private void UpdateFilter()
        {
            decimal minFileSize = MinFileSize;
            decimal modifiedAfter = ModifiedAfter;
            Func<PictureFile, bool> filter = delegate(PictureFile file)
                {
                    if (file.FileSize < minFileSize)
                        return false;

                    if (modifiedAfter > 0 &&
                        DateTime.Now.AddSeconds((double) (0 - modifiedAfter)).CompareTo(file.ModifiedDate) > 0)
                        return false;

                    return true;
                };
            foreach (object o in directoryTreeView.Objects)
            {
                ((FileGroup) o).Filter = filter;
            }
        }

        private void ModifiedAfterSuffixOnSelectedValueChanged(object sender, EventArgs eventArgs)
        {
            ModifiedAfter = modifiedAfter.Value*((LabelledInt) modifiedAfterSuffix.SelectedItem).Value;
            RefreshTree();
        }

        private void RefreshTree()
        {
            directoryTreeView.RebuildAll(true);
            directoryTreeView.Sort();
            Total.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            Total.Width = Math.Max(20+Total.Width,100);
        }

        private void MinSizeOnValueChanged(object sender, EventArgs eventArgs)
        {
            MinFileSize = minSize.Value*((LabelledInt) minSizeSuffix.SelectedItem).Value;
            RefreshTree();
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
            UpdateFilter();
            if (AutoRun)
            {
                ShowPictures(
                    directoryTreeView.Objects.Cast<FileGroup>().SelectMany(directory => directory.GetFilesRecursive()));
            }
            RefreshTree();
        }


        private IEnumerable<PictureFile> GetSelectedFiles()
        {
            return
                directoryTreeView.SelectedObjects.Cast<FileGroup>()
                                 .SelectMany(directory => directory.GetFilesRecursive());
        }

        private void DirectoryTreeViewOnItemActivate(object sender, EventArgs eventArgs)
        {
            ShowPictures(GetSelectedFiles());
        }


        public void AddBaseDir(string baseDir)
        {
            directoryTreeView.AddObject(new DirectoryFileGroup(baseDir));
        }

        public void AddFile(PictureFile file)
        {
            foreach (object o in directoryTreeView.Objects)
            {
                if (((FileGroup) o).AddFile(file))
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