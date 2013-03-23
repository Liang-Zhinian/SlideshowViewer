using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace SlideshowViewer.FileGroup
{
    internal class DirectoryTreeFileGroup : FileGroup
    {
        private readonly DirectoryInfo _directoryInfo;


        public DirectoryTreeFileGroup(string name, DirectoryInfo directoryInfo)
            : base(name)
        {
            _directoryInfo = directoryInfo;
        }


        public override IEnumerable<FileGroup> ScanDirectories(CancellationToken token)
        {
            var existingGroups = new HashSet<string>(GetGroups().Select(@group => group.Name));
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

            var existingFiles = new HashSet<string>(GetFiles().Select(file => file.FileName));
            IEnumerable<PictureFile> pictureFiles =
                PictureFile.FileNamePatterns.SelectMany(pattern => _directoryInfo.EnumerateFiles(pattern))
                           .Distinct(new CompareFileNames())
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
                    files = new List<PictureFile>();
                }
            }
            if (!files.IsEmpty())
            {
                AddFiles(files);
            }
            if (!existingFiles.IsEmpty())
            {
                RemoveFiles(existingFiles);
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