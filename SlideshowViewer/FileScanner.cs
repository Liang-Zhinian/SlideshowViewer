using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;

namespace SlideshowViewer
{
    public class FileScanner
    {
        public static IEnumerable<string> FileNamePatterns { get; private set; }

        private List<string> _fileNames=new List<string>();
        private List<string> _folders = new List<string>();

        static FileScanner()
        {
            FileNamePatterns = ImageCodecInfo.GetImageEncoders().SelectMany(info => info.FilenameExtension.Split(';'));
        }

        public bool AddFile(string filename)
        {
            if (!FileNamePatterns.Any(s => Path.GetFileName(filename).MatchGlob(s)))
                return false;
            _fileNames.Add(filename);
            return true;
        }


        public void AddFolder(string folderName)
        {
            _folders.Add(folderName);
        }


        public class CompareFileNames : IEqualityComparer<FileInfo>
        {
            public bool Equals(FileInfo x, FileInfo y)
            {
                return x.FullName == y.FullName;
            }

            public int GetHashCode(FileInfo obj)
            {
                return obj.FullName.GetHashCode();
            }
        }

        public RootFileGroup GetRoot()
        {
            var root = new RootFileGroup();

            root.AddGroups(_folders.Select(s => new DirectoryInfo(s)).Select(info => new ScanningDirectoryFileGroup(info.FullName,info)));
            
            if (!_fileNames.IsEmpty())
            {
                var manuallyAdded = new FileGroup("Manually added");
                manuallyAdded.AddFiles(_fileNames.Select(s => new FileInfo(s)).Select(info => new PictureFile(info)));
                root.AddGroup(manuallyAdded);
            }

            return root;
        }

    }
}