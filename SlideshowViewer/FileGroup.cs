using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Action = Amib.Threading.Action;

namespace SlideshowViewer
{
    public class FileGroup
    {
        private readonly List<PictureFile> _files = new List<PictureFile>();
        private readonly List<FileGroup> _groups = new List<FileGroup>();
        private System.Func<PictureFile, bool> _filter = file => true;
        private long? _numberOfFilesFiltered;


        public FileGroup(string name)
        {
            Name = name;
        }

        public virtual string Name { get; private set; }

        public System.Func<PictureFile, bool> Filter
        {
            get { return _filter; }
            set
            {
                _filter = value;
                _numberOfFilesFiltered = null;
                foreach (FileGroup fileGroup in GetGroups())
                {
                    fileGroup.Filter = _filter;
                }
            }
        }

        protected virtual IEnumerable<FileGroup> GetGroups()
        {
            lock (_groups)
            {
                return new List<FileGroup>(_groups);
            }
        }

        public void AddGroups(IEnumerable<FileGroup> fileGroups)
        {
            lock (_groups)
            {
                _groups.AddRange(fileGroups);
            }
        }

        public void RemoveGroups(IEnumerable<string> groups)
        {
            lock (_groups)
            {
                _groups.RemoveAll(@group => groups.Contains(group.Name));
            }
        }


        public void AddFiles(IEnumerable<PictureFile> files)
        {
            lock (_files)
            {
                _files.AddRange(files);
            }
        }

        public void RemoveFiles(IEnumerable<string> existingFiles)
        {
            lock (_files)
            {
                _files.RemoveAll(file => existingFiles.Contains(file.FileName));
            }
        }


        public void AddGroup(FileGroup fileGroup)
        {
            lock (_groups)
            {
                _groups.Add(fileGroup);
            }
        }

        public virtual void AddFile(PictureFile file)
        {
            lock (_files)
            {
                _files.Add(file);
            }
        }

        public virtual IEnumerable<FileGroup> GetNonEmptyGroups()
        {
            return GetGroups().Where(fileGroup => fileGroup.GetNumberOfFiles() > 0);
        }

        public virtual bool HasNonEmptyGroups()
        {
            return GetNonEmptyGroups().Any();
        }


        public static bool CanExpandGetter(object model)
        {
            return ((FileGroup) model).HasNonEmptyGroups();
        }

        public static IEnumerable ChildrenGetter(object model)
        {
            return ((FileGroup) model).GetNonEmptyGroups();
        }

        public IEnumerable<PictureFile> GetFilesRecursive()
        {
            foreach (PictureFile pictureFile in GetFilteredFiles())
                yield return pictureFile;
            foreach (FileGroup fileGroup in GetGroups())
            {
                foreach (PictureFile pictureFile in fileGroup.GetFilesRecursive())
                {
                    yield return pictureFile;
                }
            }
        }

        public List<PictureFile> GetFiles()
        {
            lock (_files)
            {
                return new List<PictureFile>(_files);
            }
        }

        public IEnumerable<PictureFile> GetFilteredFiles()
        {            
            return GetFiles().Where(_filter);
        }

        public long GetNumberOfFiles()
        {
            if (_numberOfFilesFiltered == null)
                _numberOfFilesFiltered = GetFilteredFiles().Count() +
                                         GetGroups().Sum(fileGroup => fileGroup.GetNumberOfFiles());
            return (long) _numberOfFilesFiltered;
        }

        public virtual IEnumerable<FileGroup> ScanDirectories(CancellationToken token)
        {
            return GetGroups();
        }

    }


    public class RootFileGroup : FileGroup
    {
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private Action _onScanningDone;
        private bool _scanningDone;
        private object _onScanningDoneLock=new object();

        public void Stop()
        {
            _tokenSource.Cancel();
        }

        public RootFileGroup() : base("All files")
        {
        }

        public override string Name
        {
            get { return GetGroups().Count() == 1 ? GetGroups().First().Name : base.Name; }
        }

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


        public void StartScan()
        {
            var thread = new Thread(FirstScan) {IsBackground = true};
            thread.Start();
        }

        private void FirstScan()
        {
            try
            {
                Queue<FileGroup> groups = new Queue<FileGroup>();
                groups.Enqueue(this);
                while (groups.Count > 0)
                {
                    _tokenSource.Token.ThrowIfCancellationRequested();
                    foreach (var fileGroup in groups.Dequeue().ScanDirectories(_tokenSource.Token))
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
                var thread = new Thread(ContinousScan) {Priority = ThreadPriority.BelowNormal,IsBackground = true};
                thread.Start();
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
                    Queue<FileGroup> groups = new Queue<FileGroup>();
                    groups.Enqueue(this);
                    while (groups.Count > 0)
                    {
                        _tokenSource.Token.ThrowIfCancellationRequested();
                        foreach (var fileGroup in groups.Dequeue().ScanDirectories(_tokenSource.Token))
                        {
                            groups.Enqueue(fileGroup);
                        }
                    }
                }
            }
            catch (OperationCanceledException )
            {
            }
        }


        public override IEnumerable<FileGroup> GetNonEmptyGroups()
        {
            return GetGroups().Count() == 1 ? GetGroups().First().GetNonEmptyGroups() : base.GetNonEmptyGroups();
        }

        public override bool HasNonEmptyGroups()
        {
            return GetGroups().Count() == 1 ? GetGroups().First().HasNonEmptyGroups() : base.HasNonEmptyGroups();
        }
    }


    internal class ScanningDirectoryFileGroup : FileGroup
    {
        private static bool _flag;
        private static readonly object _flagLock = new object();

        private readonly DirectoryInfo _directoryInfo;


        public ScanningDirectoryFileGroup(string name, DirectoryInfo directoryInfo)
            : base(name)
        {
            _directoryInfo = directoryInfo;
        }

        public static bool Changed
        {
            get
            {
                lock (_flagLock)
                {
                    bool ret = _flag;
                    _flag = false;
                    return ret;
                }
            }
            private set
            {
                lock (_flagLock)
                {
                    _flag = value;
                }
            }
        }


        public override IEnumerable<FileGroup> ScanDirectories(CancellationToken token)
        {
            HashSet<string> existingGroups = new HashSet<string>(GetGroups().Select(@group => group.Name));
            IEnumerable<ScanningDirectoryFileGroup> fileGroups =
                _directoryInfo.EnumerateDirectories()
                              .Where(info => info.Name != ".picasaoriginals")
                              .Where(info => !existingGroups.Remove(info.Name))
                              .Select(info => new ScanningDirectoryFileGroup(info.Name, info));


            var groups = new List<FileGroup>();
            foreach (ScanningDirectoryFileGroup fileGroup in fileGroups)
            {
                token.ThrowIfCancellationRequested();
                groups.Add(fileGroup);
            }
            AddGroups(groups);

            if (!existingGroups.IsEmpty())
            {
                RemoveGroups(existingGroups);
                Changed = true;
            }

            HashSet<string> existingFiles=new HashSet<string>(GetFiles().Select(file => file.FileName));
            IEnumerable<PictureFile> pictureFiles =
                FileScanner.FileNamePatterns.SelectMany(pattern => _directoryInfo.EnumerateFiles(pattern))
                           .Distinct(new FileScanner.CompareFileNames())
                           .Where(info => !info.Name.StartsWith("._"))
                           .Where(info => !existingFiles.Remove(info.FullName))
                           .Select(info => new PictureFile(info));
            var files = new List<PictureFile>();
            foreach (PictureFile pictureFile in pictureFiles)
            {
                token.ThrowIfCancellationRequested();
                files.Add(pictureFile);
                if (files.Count > 1000)
                {
                    AddFiles(files);
                    Changed = true;
                    files=new List<PictureFile>();
                }
            }
            if (!files.IsEmpty())
            {
                AddFiles(files);
                Changed = true;                
            }
            if (!existingFiles.IsEmpty())
            {
                RemoveFiles(existingFiles);
                Changed = true;
            }

            return GetGroups();
        }

    }
}