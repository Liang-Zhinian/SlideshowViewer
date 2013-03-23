using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SlideshowViewer.FileGroup
{
    public class FileGroup : IComparable<FileGroup>
    {
        private readonly SortedSet<PictureFile> _files = new SortedSet<PictureFile>();
        private readonly SortedSet<FileGroup> _groups = new SortedSet<FileGroup>();
        private Func<PictureFile, bool> _filter = file => true;
        private long? _numberOfFilesFiltered;


        protected internal FileGroup(string name)
        {
            Name = name;
        }

        public virtual string Name { get; private set; }

        public Func<PictureFile, bool> Filter
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

        protected List<PictureFile> GetFiles()
        {
            lock (_files)
            {
                return new List<PictureFile>(_files);
            }
        }

        protected void AddFiles(IEnumerable<PictureFile> files)
        {
            lock (_files)
            {
                _files.AddAll(files);
                Changed = true;
            }
        }

        protected void RemoveFiles(IEnumerable<string> existingFiles)
        {
            lock (_files)
            {
                _files.RemoveWhere(file => existingFiles.Contains(file.FileName));
                Changed = true;
            }
        }

        internal void AddFile(PictureFile file)
        {
            lock (_files)
            {
                _files.Add(file);
                Changed = true;
            }
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

        public IEnumerable<PictureFile> GetFilesRecursive()
        {

            Func<object, string> GetName = delegate(object o)
                {
                    if (o is FileGroup)
                        return ((FileGroup) o).Name;
                    return ((PictureFile) o).FileInfo.Name;
                };

            foreach (var next in Extensions.MergeSorted<object>((o, o1) => GetName(o).CompareTo(GetName(o1)),GetFilteredFiles(),GetGroups()))
            {
                if (next is FileGroup)
                {
                    foreach (var pictureFile in ((FileGroup) next).GetFilesRecursive())
                    {
                        yield return pictureFile;
                    }
                }
                else
                {
                    yield return (PictureFile) next;
                }
            }
        }


        private IEnumerable<PictureFile> GetFilteredFiles()
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