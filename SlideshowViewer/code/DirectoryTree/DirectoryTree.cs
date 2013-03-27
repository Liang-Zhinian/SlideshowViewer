using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SlideshowViewer.FileGroup;
using SlideshowViewer.Properties;
using SlideshowViewer.ResumeManager;
using Timer = System.Timers.Timer;

namespace SlideshowViewer
{
    public class DirectoryTree
    {
        private readonly DirectoryTreeForm _form = new DirectoryTreeForm();
        private List<PictureFile.PictureFile> _browseFiles;
        private bool _isScanning = true;
        private decimal _minFileSize;
        private decimal _modifiedAfter;
        private PictureViewerForm _pictureViewerForm;
        private Timer _refreshTimer;
        private RootFileGroup _root;
        private int _slideshowFileIndex;
        private List<PictureFile.PictureFile> _slideshowFiles;

        public DirectoryTree()
        {
            ResumeManager = new MemoryResumeManager();
            Shuffle = true;
            Loop = true;
            DelayInSec = 15;
            OverlayText =
                "{description}{nl}{dateTaken}{nl}{makeAndModel}{nl}{fullName}{nl}{index} / {total}";

            AutoRun = true;

            _form.buildDate.Text = "Build date: " + Utils.RetrieveLinkerTimestamp().ToString();
            _form.directoryTreeView.CanExpandGetter = FileGroup.FileGroup.CanExpandGetter;
            _form.directoryTreeView.ChildrenGetter = FileGroup.FileGroup.ChildrenGetter;
            _form.directoryTreeView.ItemActivate += DirectoryTreeViewOnItemActivate;
            _form.directoryTreeView.KeyPress +=
                delegate(object sender, KeyPressEventArgs args) { args.Handled = args.KeyChar == '\r'; };

            _form.Total.AspectGetter = rowObject => ((FileGroup.FileGroup) rowObject).GetNumberOfFiles();
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
            _form.directoryTreeView.AllowDrop = true;
            _form.directoryTreeView.DragEnter += delegate(object sender, DragEventArgs args)
                {
                    if (args.Data.GetDataPresent(DataFormats.FileDrop))
                        args.Effect = DragDropEffects.Copy;
                    else
                        args.Effect = DragDropEffects.None;
                };
            _form.directoryTreeView.DragDrop += delegate(object sender, DragEventArgs args)
                {
                    try
                    {
                        var a = (Array) args.Data.GetData(DataFormats.FileDrop);

                        _form.BeginInvoke(new MethodInvoker(delegate
                            {
                                foreach (object fileName in a)
                                {
                                    _root.Add(fileName.ToString());
                                }
                                _form.directoryTreeView.Expand(_root);
                            }));
                    }
                    catch (Exception)
                    {
                    }
                };
        }

        public IResumeManager ResumeManager { get; set; }

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
            if (GetNumberOfSelectedFiles() == 0)
                return;
            Cursor cursor = _form.Cursor;
            _form.Cursor = Cursors.WaitCursor;
            ThreadExecutionState.DisplayRequired();
            using (PictureViewerForm pictureViewerForm = CreatePictureViewForm())
            {
                UpdateStatusBar();
                _browseFiles = new List<PictureFile.PictureFile>(GetSelectedFiles());
                SetupForm(pictureViewerForm, null);
                pictureViewerForm.ShowDialog();
                _slideshowFiles = null;
            }
            _pictureViewerForm = null;
            ThreadExecutionState.RestoreDefault();
            UpdateStatusBar();
            if (_form.Cursor == Cursors.WaitCursor)
                _form.Cursor = cursor;
        }


        private void SetupForm(PictureViewerForm pictureViewerForm, PictureFile.PictureFile startFile)
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
                pictureViewerForm.Files = new List<PictureFile.PictureFile>(_browseFiles);
                pictureViewerForm.FileIndex = startFile != null
                                                  ? new List<PictureFile.PictureFile>(_browseFiles).IndexOf(startFile)
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
                    List<PictureFile.PictureFile> pictureFiles;
                    pictureViewerForm.FileIndex = PrepareFileList(_browseFiles, out pictureFiles);
                    pictureViewerForm.Files = pictureFiles;
                }
            }
            pictureViewerForm.Browsing = Browse;
            pictureViewerForm.ShowPicture();
        }

        private int PrepareFileList(IEnumerable<PictureFile.PictureFile> files,
                                    out List<PictureFile.PictureFile> pictureFiles)
        {
            var shownFiles = new List<PictureFile.PictureFile>();
            var notShownFiles = new List<PictureFile.PictureFile>();

            foreach (PictureFile.PictureFile file in files)
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
                shownFiles = new List<PictureFile.PictureFile>();
            }

            pictureFiles = new List<PictureFile.PictureFile>();
            if (Shuffle)
            {
                pictureFiles.AddRange(shownFiles.GetShuffled());
                pictureFiles.AddRange(notShownFiles.GetShuffled());
            }
            else
            {
                pictureFiles.AddRange(shownFiles);
                pictureFiles.AddRange(notShownFiles);
            }
            return shownFiles.Count;
        }

        private void ToggleBrowsing(PictureFile.PictureFile file)
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
                Func<PictureFile.PictureFile, bool> filter = delegate(PictureFile.PictureFile file)
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
            _form.directoryTreeView.SelectObject(_form.directoryTreeView.Objects.Cast<FileGroup.FileGroup>().First());

            _root.OnScanningDone = delegate { _form.BeginInvoke(new MethodInvoker(OnScanningDone)); };
            _refreshTimer = new Timer(500);
            _refreshTimer.SynchronizingObject = _form;
            _refreshTimer.AutoReset = true;
            _refreshTimer.Elapsed += delegate
                {
                    if (FileGroup.FileGroup.Changed)
                    {
                        UpdateFilter();
                        RefreshTree();
                        UpdateStatusBar();
                    }
                };
            _refreshTimer.Start();
        }


        private void UpdateStatusBar()
        {
            string text = "";
            if (!_isScanning && _root.GetNumberOfFiles() == 0)
            {
                text = "No files found. Drag and drop folders and files into file area to view.";
                _form.throbber.Visible = false;
            }
            else
            {
                var items = new List<string>();
                if (_isScanning)
                    items.Add("Scanning directories");
                if (AutoRun)
                    items.Add("Will start slideshow when scanning is done");
                if (_pictureViewerForm != null)
                    items.Add("Starting viewer");
                foreach (string item in items)
                {
                    if (!text.IsEmpty())
                        text += " - ";
                    text += item;
                }
                _form.throbber.Visible = !text.IsEmpty();
            }
            _form.statusBar.Text = text;
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
                UpdateStatusBar();
            }
        }


        private decimal GetNumberOfSelectedFiles()
        {
            return
                _form.directoryTreeView.SelectedObjects.Cast<FileGroup.FileGroup>()
                     .Sum(directory => directory.GetNumberOfFiles());
        }

        private IEnumerable<PictureFile.PictureFile> GetSelectedFiles()
        {
            return
                _form.directoryTreeView.SelectedObjects.Cast<FileGroup.FileGroup>()
                     .SelectMany(directory => directory.GetFilesRecursive());
        }

        private void DirectoryTreeViewOnItemActivate(object sender, EventArgs eventArgs)
        {
            if (!AutoRun)
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

        #region Nested type: LabelledInt

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

        #endregion
    }
}