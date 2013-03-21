using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SlideshowViewer.Properties;
using Timer = System.Timers.Timer;

namespace SlideshowViewer
{
    public class DirectoryTree
    {
        private readonly DirectoryTreeForm _form = new DirectoryTreeForm();
        private bool _isScanning = true;
        private decimal _minFileSize;
        private decimal _modifiedAfter;
        private PictureViewerForm _pictureViewerForm;
        private Timer _refreshTimer;
        private RootFileGroup _root;
        private int _slideshowFileIndex;
        private List<PictureFile> _slideshowFiles;

        public DirectoryTree()
        {
            ResumeManager = new DummyResumeManager();
            Shuffle = false;
            Loop = true;
            DelayInSec = 15;
            OverlayText =
                "{imageDescription}{eol}{description}{eol}{dateTime}{eol}{model}{eol}{fullName}{eol}{index} / {total}";

            AutoRun = true;


            _form.directoryTreeView.CanExpandGetter = FileGroup.CanExpandGetter;
            _form.directoryTreeView.ChildrenGetter = FileGroup.ChildrenGetter;
            _form.directoryTreeView.ItemActivate += DirectoryTreeViewOnItemActivate;
            _form.directoryTreeView.KeyPress +=
                delegate(object sender, KeyPressEventArgs args) { args.Handled = args.KeyChar == '\r'; };

            _form.Total.AspectGetter = rowObject => ((FileGroup) rowObject).GetNumberOfFiles();
            _form.minSize.ValueChanged += MinSizeOnValueChanged;
            _form.minSizeSuffix.SelectedValueChanged += MinSizeOnValueChanged;
            _form.minSizeSuffix.Items.Clear();
            var bytes = new LabelledInt("bytes", 1);
            _form.minSizeSuffix.Items.Add(bytes);
            _form.minSizeSuffix.Items.Add(new LabelledInt("KB", 1024));
            _form.minSizeSuffix.Items.Add(new LabelledInt("MB", 1024*1024));
            _form.minSizeSuffix.SelectedItem = bytes;
            _form.modifiedAfterSuffix.Items.Clear();
            _form.modifiedAfterSuffix.Items.Add(new LabelledInt("seconds", 1));
            _form.modifiedAfterSuffix.Items.Add(new LabelledInt("minutes", 60));
            _form.modifiedAfterSuffix.Items.Add(new LabelledInt("hours", 60*60));
            var days = new LabelledInt("days", 60*60*24);
            _form.modifiedAfterSuffix.Items.Add(days);
            _form.modifiedAfterSuffix.Items.Add(new LabelledInt("weeks", 60*60*24*7));
            _form.modifiedAfterSuffix.Items.Add(new LabelledInt("months", 60*60*24*30));
            _form.modifiedAfterSuffix.Items.Add(new LabelledInt("years", 60*60*24*365));
            _form.modifiedAfterSuffix.SelectedItem = days;
            _form.modifiedAfterSuffix.SelectedValueChanged += ModifiedAfterSuffixOnSelectedValueChanged;
            _form.modifiedAfter.ValueChanged += ModifiedAfterSuffixOnSelectedValueChanged;
            _form.Shown += OnFormShown;
            _form.FormClosing += OnFormClosing;
        }

        public ResumeManager ResumeManager { get; set; }

        public bool Shuffle
        {
            get { return _form.shuffle.Checked; }
            set { _form.shuffle.Checked = value; }
        }

        public bool Loop
        {
            get { return _form.loop.Checked; }
            set { _form.loop.Checked = value; }
        }

        public bool Browse
        {
            get { return _form.browse.Checked; }
            set { _form.browse.Checked = value; }
        }


        public decimal DelayInSec
        {
            get { return _form.delay.Value; }
            set { _form.delay.Value = value; }
        }

        public string OverlayText { get; set; }

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

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            _refreshTimer.Stop();
            _root.Stop();
        }


        public void ShowPictures()
        {
            var cursor = _form.Cursor;
            _form.Cursor=Cursors.WaitCursor;
            using (PictureViewerForm pictureViewerForm = CreatePictureViewForm())
            {
                UpdateStatusBar();
                SetupForm(pictureViewerForm, null);
                pictureViewerForm.ShowDialog();
                _slideshowFiles = null;
            }
            _pictureViewerForm = null;
            UpdateStatusBar();
            if (_form.Cursor == Cursors.WaitCursor)
                _form.Cursor = cursor;
        }

        private void SetupForm(PictureViewerForm pictureViewerForm, PictureFile startFile)
        {
            _pictureViewerForm.Loop = Loop;
            _pictureViewerForm.DelayInSec = DelayInSec;
            _pictureViewerForm.OverlayTextTemplate = OverlayText;
            if (Browse)
            {
                if (!pictureViewerForm.Browsing)
                {
                    _slideshowFiles = pictureViewerForm.Files;
                    _slideshowFileIndex = pictureViewerForm.FileIndex;
                }
                pictureViewerForm.Files = new List<PictureFile>(GetSelectedFiles());
                pictureViewerForm.FileIndex = startFile != null
                                                  ? new List<PictureFile>(GetSelectedFiles()).IndexOf(startFile)
                                                  : 0;
            }
            else
            {
                if (_slideshowFiles != null)
                {
                    pictureViewerForm.FileIndex = _slideshowFileIndex;
                    pictureViewerForm.Files = _slideshowFiles;
                }
                else
                {
                    List<PictureFile> pictureFiles;
                    pictureViewerForm.FileIndex = PrepareFileList(GetSelectedFiles(), out pictureFiles);
                    pictureViewerForm.Files = pictureFiles;
                }
            }
            pictureViewerForm.Browsing = Browse;
            pictureViewerForm.ShowPicture();
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

        private void ToggleBrowsing(PictureFile file)
        {
            Browse = !Browse;
            SetupForm(_pictureViewerForm, file);
        }


        private PictureViewerForm CreatePictureViewForm()
        {
            _pictureViewerForm = new PictureViewerForm();
            _pictureViewerForm.Icon = Resources.image_x_generic;
            _pictureViewerForm.PictureShown = ResumeManager.SetToShown;
            _pictureViewerForm.AllPicturesShown = ResumeManager.SetToNotShown;
            _pictureViewerForm.ToggleBrowsing = ToggleBrowsing;
            return _pictureViewerForm;
        }


        private void UpdateFilter()
        {
            if (_root != null)
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
                _root.Filter = filter;
            }
        }

        private void ModifiedAfterSuffixOnSelectedValueChanged(object sender, EventArgs eventArgs)
        {
            ModifiedAfter = _form.modifiedAfter.Value*((LabelledInt) _form.modifiedAfterSuffix.SelectedItem).Value;
            RefreshTree();
        }

        private void RefreshTree()
        {
            ListViewItem item = _form.directoryTreeView.FocusedItem;
            _form.directoryTreeView.RebuildAll(true);
            try
            {
                _form.directoryTreeView.FocusedItem = item;
            }
            catch (Exception)
            {
            }
            _form.directoryTreeView.Sort();
        }

        private void MinSizeOnValueChanged(object sender, EventArgs eventArgs)
        {
            MinFileSize = _form.minSize.Value*((LabelledInt) _form.minSizeSuffix.SelectedItem).Value;
            RefreshTree();
        }

        private void OnFormShown(object sender, EventArgs eventArgs)
        {
            _form.Cursor = Cursors.AppStarting;
            UpdateStatusBar();
            foreach (object item in _form.minSizeSuffix.Items)
            {
                decimal value = _minFileSize/((LabelledInt) item).Value;
                if (value < 1024)
                {
                    _form.minSize.Value = value;
                    _form.minSizeSuffix.SelectedItem = item;
                    break;
                }
            }
            if (AutoRun)
            {
                _form.directoryTreeView.ForeColor = Color.FromName(KnownColor.InactiveCaptionText.ToString());
            }
            UpdateFilter();
            RefreshTree();
            _form.directoryTreeView.SelectObject(_form.directoryTreeView.Objects.Cast<FileGroup>().First());

            _root.OnScanningDone = delegate { _form.BeginInvoke(new MethodInvoker(OnScanningDone)); };
            _refreshTimer = new Timer(1000);
            _refreshTimer.SynchronizingObject = _form;
            _refreshTimer.AutoReset = true;
            _refreshTimer.Elapsed += delegate
                {
                    if (ScanningDirectoryFileGroup.Changed)
                    {
                        UpdateFilter();
                        RefreshTree();
                    }
                };
            _refreshTimer.Start();
        }


        private void UpdateStatusBar()
        {
            string text = "";
            var items = new List<string>();
            if (_isScanning)
                items.Add("Scanning directories");
            if (AutoRun)
                items.Add("Will start slideshow when scanning is done");
            if (_pictureViewerForm != null)
                items.Add("Starting viewer");
            foreach (var item in items)
            {
                if (!text.IsEmpty())
                    text += " - ";
                text += item;
            }
            _form.statusBar.Text = text;
            _form.throbber.Visible = !text.IsEmpty();
            _form.statusBar.Invalidate();
            _form.statusBar.Update();
        }

        private void OnScanningDone()
        {
            _form.statusBar.Text = "";
            _form.directoryTreeView.BackColor = Color.White;
            _form.directoryTreeView.ForeColor = Color.FromName(KnownColor.WindowText.ToString());
            _form.Cursor = Cursors.Default;
            _isScanning = false;
            UpdateFilter();
            RefreshTree();
            UpdateStatusBar();
            if (AutoRun)
            {
                AutoRun = false;
                _form.directoryTreeView.SelectedObject = _root;
                _form.Update();
                ShowPictures();
            }
        }


        private IEnumerable<PictureFile> GetSelectedFiles()
        {
            return
                _form.directoryTreeView.SelectedObjects.Cast<FileGroup>()
                     .SelectMany(directory => directory.GetFilesRecursive());
        }

        private void DirectoryTreeViewOnItemActivate(object sender, EventArgs eventArgs)
        {
            if (_root.GetNumberOfFiles() > 0 && !AutoRun)
                ShowPictures();
        }


        public void Run()
        {
            Application.Run(_form);
        }

        public void SetRoot(RootFileGroup root)
        {
            _root = root;
            _form.directoryTreeView.Objects = new[] {root};
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