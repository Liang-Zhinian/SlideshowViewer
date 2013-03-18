using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SlideshowViewer
{
    public class FileGroup
    {
        protected readonly List<PictureFile> _files = new List<PictureFile>();
        private readonly List<FileGroup> _groups = new List<FileGroup>();
        private Func<PictureFile, bool> _filter = file => true;
        private long? _numberOfFilesFiltered;


        public FileGroup(string name)
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

        protected virtual IEnumerable<FileGroup> GetGroups()
        {
            return _groups;
        }

        protected void AddGroup(FileGroup fileGroup)
        {
            _groups.Add(fileGroup);
        }

        public virtual bool AddFile(PictureFile file)
        {
            _files.Add(file);
            return true;
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
            foreach (PictureFile pictureFile in GetFiles()) 
                yield return pictureFile;
            foreach (FileGroup fileGroup in GetGroups())
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
                                         GetGroups().Sum(fileGroup => fileGroup.GetNumberOfFiles());
            return (long) _numberOfFilesFiltered;
        }
    }


    internal class RootFileGroup : FileGroup
    {
        private FileGroup _restGroup;

        public RootFileGroup() : base("All files")
        {
        }

        public override string Name
        {
            get { return GetGroups().Count() == 1 ? GetGroups().First().Name : base.Name; }
        }

        private FileGroup GetRestGroup()
        {
            if (_restGroup == null)
                _restGroup = new FileGroup("Manually added files");
            return _restGroup;
        }

        protected override IEnumerable<FileGroup> GetGroups()
        {
            foreach (FileGroup fileGroup in base.GetGroups())
            {
                yield return fileGroup;
            }
            if (_restGroup != null)
                yield return _restGroup;
        }

        public override bool AddFile(PictureFile file)
        {
            foreach (FileGroup fileGroup in GetGroups())
            {
                if (fileGroup.AddFile(file))
                    return true;
            }
            return GetRestGroup().AddFile(file);
        }

        public void AddBaseDir(string directory)
        {
            AddGroup(new DirectoryFileGroup(directory));
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
                base.AddFile(file);
            }
            else
            {
                GetOrCreateDirectory(parts[0]).AddFile(parts.GetRange(1), file);
            }
        }


        private DirectoryFileGroup GetOrCreateDirectory(string name)
        {
            var dir = (DirectoryFileGroup) GetGroups().FirstOrDefault(directory => directory.Name == name);
            if (dir != null)
                return dir;
            dir = new DirectoryFileGroup(name);
            AddGroup(dir);
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