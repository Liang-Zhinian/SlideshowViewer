using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SlideshowViewer.FileGroup
{
    public class FileGroup : IComparable<FileGroup>
    {
        private SortedSet<PictureFile.PictureFile> _files = new SortedSet<PictureFile.PictureFile>();
        private object _filesLock=new object();
        private readonly SortedSet<FileGroup> _groups = new SortedSet<FileGroup>();
        private Func<PictureFile.PictureFile, bool> _filter = file => true;
        private long? _numberOfFilesFiltered;


        protected internal FileGroup(string name)
        {
            Name = name;
        }

        public virtual string Name { get; private set; }

        public Func<PictureFile.PictureFile, bool> Filter
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

        #region Changed

        private static bool _flag;
        private static readonly object _flagLock = new object();

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
            protected set
            {
                lock (_flagLock)
                {
                    _flag = value;
                }
            }
        }

        #endregion

        #region groups

        protected IEnumerable<FileGroup> GetGroups()
        {
            lock (_groups)
            {
                return new List<FileGroup>(_groups);
            }
        }

        protected void AddGroup(FileGroup fileGroup)
        {
            lock (_groups)
            {
                _groups.Add(fileGroup);
            }
        }

        protected void AddGroups(IEnumerable<FileGroup> fileGroups)
        {
            lock (_groups)
            {
                _groups.AddAll(fileGroups);
            }
        }

        protected void RemoveGroups(IEnumerable<string> groups)
        {
            lock (_groups)
            {
                if (_groups.RemoveWhere(@group => groups.Contains(group.Name)) > 0)
                    Changed = true;
            }
        }

        #endregion

        #region files

        public SortedSet<PictureFile.PictureFile> GetFiles()
        {
            return _files;
        }

        protected internal void AddFiles(IEnumerable<PictureFile.PictureFile> files)
        {
            lock (_filesLock)
            {
                SortedSet<PictureFile.PictureFile> newfiles = new SortedSet<PictureFile.PictureFile>(_files);
                newfiles.AddAll(files);
                _files = newfiles;
                Changed = true;
            }
        }

        protected internal void RemoveFiles(IEnumerable<string> existingFiles)
        {
            lock (_filesLock)
            {
                SortedSet<PictureFile.PictureFile> newfiles = new SortedSet<PictureFile.PictureFile>(_files);
                newfiles.RemoveWhere(file => existingFiles.Contains(file.FileName));
                _files = newfiles;
                Changed = true;
            }
        }

        internal void AddFile(params PictureFile.PictureFile[] file)
        {
            AddFiles(file);
        }

        #endregion

        internal virtual IEnumerable<FileGroup> GetNonEmptyGroups()
        {
            return GetGroups().Where(fileGroup => fileGroup.GetNumberOfFiles() > 0);
        }

        internal virtual bool HasNonEmptyGroups()
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

        public virtual IEnumerable<PictureFile.PictureFile> GetFilesRecursive()
        {
            foreach (var pictureFile in _files)
            {
                yield return pictureFile;
            }

            foreach (var pictureFile in _groups.SelectMany(fileGroup => fileGroup.GetFilesRecursive()))
            {
                yield return pictureFile;
            }
        }


        internal IEnumerable<PictureFile.PictureFile> GetFilteredFiles()
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

        #region comparison

        public int CompareTo(FileGroup other)
        {
            return String.Compare(Name.ToUpper(), other.Name.ToUpper(), StringComparison.Ordinal);
        }


        #endregion
    }
}