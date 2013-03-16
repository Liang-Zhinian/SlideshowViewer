using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SlideshowViewer
{
    internal abstract class FileGroup
    {
        protected readonly List<PictureFile> _files = new List<PictureFile>();
        protected readonly List<FileGroup> _groups = new List<FileGroup>();
        private Func<PictureFile, bool> _filter = file => true;
        private long? _numberOfFilesFiltered;

        public FileGroup(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public Func<PictureFile, bool> Filter
        {
            get { return _filter; }
            set
            {
                _filter = value;
                _numberOfFilesFiltered = null;
                foreach (FileGroup fileGroup in _groups)
                {
                    fileGroup.Filter = _filter;
                }
            }
        }

        public abstract bool AddFile(PictureFile file);

        public IEnumerable<FileGroup> GetNonEmptyGroups()
        {
            return _groups.Where(fileGroup => fileGroup.GetNumberOfFiles() > 0);
        }

        public bool HasNonEmptyGroups()
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
            foreach (PictureFile pictureFile in GetFiles()) yield return pictureFile;
            foreach (FileGroup fileGroup in _groups)
            {
                foreach (PictureFile pictureFile in fileGroup.GetFilesRecursive())
                {
                    yield return pictureFile;
                }
            }
        }

        private IEnumerable<PictureFile> GetFiles()
        {
            return _files.Where(_filter);
        }

        public long GetNumberOfFiles()
        {
            if (_numberOfFilesFiltered == null)
                _numberOfFilesFiltered = GetFiles().Count() +
                                         _groups.Sum(fileGroup => fileGroup.GetNumberOfFiles());
            return (long) _numberOfFilesFiltered;
        }
    }




    internal class DirectoryFileGroup : FileGroup
    {
        private readonly List<string> _parts;

        public DirectoryFileGroup(string name) : base(name)
        {
            _parts = SplitPathIntoParts(name);
        }

        public override bool AddFile(PictureFile file)
        {
            string fileName = file.FileName;
            List<string> parts = SplitPathIntoParts(fileName);
            if (parts.StartsWith(_parts))
            {
                parts = parts.GetRange(_parts.Count);
                AddFile(parts, file);
                return true;
            }
            return false;
        }


        protected void AddFile(List<string> parts, PictureFile file)
        {
            if (parts.Count == 1)
            {
                _files.Add(file);
            }
            else
            {
                GetOrCreateDirectory(parts[0]).AddFile(parts.GetRange(1), file);
            }
        }


        private DirectoryFileGroup GetOrCreateDirectory(string name)
        {
            DirectoryFileGroup dir = (DirectoryFileGroup) _groups.Find(directory => directory.Name == name);
            if (dir != null)
                return dir;
            dir = new DirectoryFileGroup(name);
            _groups.Add(dir);
            return dir;
        }

        private List<string> SplitPathIntoParts(string name)
        {
            var ret = new List<string>();
            ret.AddRange(name.Split(new[]
                {
                    Path.AltDirectorySeparatorChar,
                    Path.DirectorySeparatorChar
                }));
            return ret;
        }
    }
}