using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace SlideshowViewer.FileGroup
{
    internal class DirectoryTreeFileGroup : FileGroup
    {
        private readonly DirectoryInfo _directoryInfo;
        private readonly FileGroup _files = new FileGroup("");


        public DirectoryTreeFileGroup(string name, DirectoryInfo directoryInfo)
            : base(name)
        {
            _directoryInfo = directoryInfo;
            AddGroup(_files);
        }


        internal override IEnumerable<FileGroup> GetNonEmptyGroups()
        {
            var groupCount = GetGroups().Count();
            return base.GetNonEmptyGroups().Where(@group => groupCount != 1 || group != _files);
        }

        public override IEnumerable<PictureFile.PictureFile> GetFilesRecursive()
        {
            Func<object, string> getName = delegate(object o)
                {
                    var @group = o as FileGroup;
                    return @group != null ? @group.Name : ((PictureFile.PictureFile) o).FileInfo.Name;
                };

            foreach (
                var next in
                    Utils.MergeSorted<object>(
                        (o, o1) => String.Compare(getName(o), getName(o1), StringComparison.Ordinal), 
                        _files.GetFilteredFiles(),
                        GetGroups().Where(@group => group!=_files)))
            {
                var @group = next as FileGroup;
                if (@group != null)
                {
                    foreach (var pictureFile in @group.GetFilesRecursive())
                    {
                        yield return pictureFile;
                    }
                }
                else
                {
                    yield return (PictureFile.PictureFile) next;
                }
            }
        }


        public override IEnumerable<FileGroup> ScanDirectories(CancellationToken token)
        {
            var existingGroups =
                new HashSet<string>(GetGroups().Where(@group => group != _files).Select(@group => group.Name));
            IEnumerable<DirectoryTreeFileGroup> fileGroups =
                _directoryInfo.EnumerateDirectories()
                              .Where(info => info.Name != ".picasaoriginals")
                              .Where(info => !existingGroups.Remove(info.Name))
                              .Select(info => new DirectoryTreeFileGroup(info.Name, info));


            var groups = new List<FileGroup>();
            foreach (DirectoryTreeFileGroup fileGroup in fileGroups)
            {
                token.ThrowIfCancellationRequested();
                groups.Add(fileGroup);
            }
            AddGroups(groups);

            if (!existingGroups.IsEmpty())
            {
                RemoveGroups(existingGroups);
            }

            var existingFiles = new HashSet<string>(_files.GetFiles().Select(file => file.FileName));
            IEnumerable<PictureFile.PictureFile> pictureFiles =
                PictureFile.PictureFile.FileNamePatterns.SelectMany(pattern => _directoryInfo.EnumerateFiles(pattern))
                           .Distinct(new CompareFileNames())
                           .Where(info => !info.Name.StartsWith("._"))
                           .Where(info => !existingFiles.Remove(info.FullName))
                           .Select(info => new PictureFile.PictureFile(info));
            var files = new List<PictureFile.PictureFile>();
            foreach (PictureFile.PictureFile pictureFile in pictureFiles)
            {
                token.ThrowIfCancellationRequested();
                files.Add(pictureFile);
                if (files.Count > 999)
                {
                    _files.AddFiles(files);
                    files = new List<PictureFile.PictureFile>();
                }
            }
            if (!files.IsEmpty())
            {
                _files.AddFiles(files);
            }
            if (!existingFiles.IsEmpty())
            {
                _files.RemoveFiles(existingFiles);
            }

            return GetGroups();
        }

        #region Nested type: CompareFileNames

        private class CompareFileNames : IEqualityComparer<FileInfo>
        {
            #region IEqualityComparer<FileInfo> Members

            public bool Equals(FileInfo x, FileInfo y)
            {
                return x.FullName == y.FullName;
            }

            public int GetHashCode(FileInfo obj)
            {
                return obj.FullName.GetHashCode();
            }

            #endregion
        }

        #endregion
    }
}