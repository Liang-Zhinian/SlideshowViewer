using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SlideshowViewer.PictureFile;

namespace SlideshowViewer.FileGroup
{
    public class RootFileGroup : FileGroup
    {
        private FileGroup _addedFiles;


        public RootFileGroup() : base("All files")
        {
        }

        #region disappear if only one group

        public override string Name
        {
            get { return GetGroups().Count() == 1 ? GetGroups().First().Name : base.Name; }
        }

        internal override IEnumerable<FileGroup> GetNonEmptyGroups()
        {
            return GetGroups().Count() == 1 ? GetGroups().First().GetNonEmptyGroups() : GetGroups();
        }

        internal override bool HasNonEmptyGroups()
        {
            return GetGroups().Count() == 1 ? GetGroups().First().HasNonEmptyGroups() : GetGroups().Any();
        }

        #endregion

        public bool Add(string fileName)
        {
            if (Directory.Exists(fileName))
            {
                var directoryInfo = new DirectoryInfo(fileName);
                AddGroup(new DirectoryTreeFileGroup(directoryInfo.FullName, directoryInfo));
                Changed = true;
                return true;
            }
            if (!PictureFile.PictureFile.FileNamePatterns.Any(s => Path.GetFileName(fileName).MatchGlob(s)))
                return false;
            if (File.Exists(fileName))
            {
                if (_addedFiles == null)
                {
                    _addedFiles = new FileGroup("<Added files>");
                    AddGroup(_addedFiles);
                    Changed = true;
                }
                _addedFiles.AddFile(new PictureFile.PictureFile(new FileInfo(fileName)));
                return true;
            }
            return false;
        }

        #region scanning

        private readonly object _onScanningDoneLock = new object();
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private Action _onScanningDone;
        private bool _scanningDone;

        public Action OnScanningDone
        {
            set
            {
                lock (_onScanningDoneLock)
                {
                    _onScanningDone = value;
                    if (_scanningDone)
                        _onScanningDone();
                }
            }
        }

        public void Stop()
        {
            _tokenSource.Cancel();
        }

        public void StartScan()
        {
            var thread = new Thread(FirstScan) {IsBackground = true};
            thread.Start();
        }

        private void FirstScan()
        {
            try
            {
                var groups = new Queue<FileGroup>();
                groups.Enqueue(this);
                while (groups.Count > 0)
                {
                    _tokenSource.Token.ThrowIfCancellationRequested();
                    foreach (FileGroup fileGroup in groups.Dequeue().ScanDirectories(_tokenSource.Token))
                    {
                        groups.Enqueue(fileGroup);
                    }
                }
                lock (_onScanningDoneLock)
                {
                    _scanningDone = true;
                    if (_onScanningDone != null)
                        _onScanningDone();
                }
                {
                    var thread = new Thread(ContinousScan) { Priority = ThreadPriority.BelowNormal, IsBackground = true };
                    thread.Start();
                }
                {
                    var thread = new Thread(ScanData) {Priority = ThreadPriority.BelowNormal, IsBackground = true};
                    thread.Start();
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void ContinousScan()
        {
            try
            {
                while (true)
                {
                    _tokenSource.Token.WaitHandle.WaitOne(5000);
                    _tokenSource.Token.ThrowIfCancellationRequested();
                    var groups = new Queue<FileGroup>();
                    groups.Enqueue(this);
                    while (groups.Count > 0)
                    {
                        _tokenSource.Token.ThrowIfCancellationRequested();
                        groups.AddAll(groups.Dequeue().ScanDirectories(_tokenSource.Token));
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void ScanData()
        {
            try
            {
                while (true)
                {
                    _tokenSource.Token.WaitHandle.WaitOne(5000);
                    _tokenSource.Token.ThrowIfCancellationRequested();
                    var groups = new Queue<FileGroup>();
                    groups.Enqueue(this);
                    while (groups.Count > 0)
                    {
                        _tokenSource.Token.ThrowIfCancellationRequested();
                        var fileGroup = groups.Dequeue();
                        foreach (var pictureFile in fileGroup.GetFiles())
                        {
                            _tokenSource.Token.ThrowIfCancellationRequested();
                            if (!pictureFile.HasInternalDataTask())
                            {
                                var task = new Task<PictureFileData>(delegate
                                    {
                                        var pictureFileData = new PictureFileData(pictureFile.FileInfo);
                                        pictureFileData.UnloadImage();
                                        return pictureFileData;
                                    });
                                task.RunSynchronously();
                                task.Wait(_tokenSource.Token);
                                pictureFile.SetInternalDataTaskIfNotSet(task);                                
                            }

                        }
                        groups.AddAll(fileGroup.GetGroups());
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        #endregion
    }
}