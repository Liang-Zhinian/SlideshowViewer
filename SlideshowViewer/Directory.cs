using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SlideshowViewer
{
    internal class Directory
    {
        public delegate bool AcceptFileDelegate(PictureFile file);
        public static AcceptFileDelegate AcceptFile { get; set; }

        private readonly List<PictureFile> _files = new List<PictureFile>();

        private readonly List<string> _parts;
        private readonly List<Directory> _subDirectories = new List<SlideshowViewer.Directory>();

        public Directory(string name)
        {
            Name = name;
            _parts = SplitPathIntoParts(name);
        }

        public string Name { get; private set; }

        public bool AddFile(PictureFile file)
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

        public IEnumerable<SlideshowViewer.Directory> GetNonEmptySubDirectories()
        {
            return _subDirectories.Where(directory => directory.GetNumberOfFiles() > 0);
        }

        public bool HasNonEmptySubDirectories()
        {
            return GetNonEmptySubDirectories().Any();
        }

        private void AddFile(List<string> parts, PictureFile file)
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


        private Directory GetOrCreateDirectory(string name)
        {
            Directory dir = _subDirectories.Find(directory => directory.Name == name);
            if (dir != null)
                return dir;
            dir = new Directory(name);
            _subDirectories.Add(dir);
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

        public static bool CanExpandGetter(object model)
        {
            return ((Directory) model).HasNonEmptySubDirectories();
        }

        public static IEnumerable ChildrenGetter(object model)
        {
            return ((Directory) model).GetNonEmptySubDirectories();
        }

        public IEnumerable<PictureFile> GetFilesRecursive()
        {
            foreach (PictureFile pictureFile in GetFiles()) yield return pictureFile;
            foreach (var subDirectory in _subDirectories)
            {
                foreach (PictureFile pictureFile in subDirectory.GetFilesRecursive())
                {
                    yield return pictureFile;
                }
            }
        }

        private IEnumerable<PictureFile> GetFiles()
        {
            return _files.Where(pictureFile => AcceptFile == null || AcceptFile(pictureFile));
        }

        public long GetNumberOfFiles()
        {
            return GetFiles().Count() + _subDirectories.Sum(subDirectory => subDirectory.GetNumberOfFiles());
        }
    }
}