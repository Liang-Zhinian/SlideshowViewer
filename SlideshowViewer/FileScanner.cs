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
        private static readonly IEnumerable<string> _filenamepatterns =
            ImageCodecInfo.GetImageEncoders().SelectMany(info => info.FilenameExtension.Split(';'));

        private List<string> _fileNames=new List<string>();
        private List<string> _folders = new List<string>();

        public bool AddFile(string filename)
        {
            if (_filenamepatterns.Any(s => Path.GetFileName(filename).MatchGlob(s)))
                _fileNames.Add(filename);
            return false;
        }


        public void AddFolder(string folderName)
        {
            _folders.Add(folderName);
        }

        private static IEnumerable<FileInfo> ScanRecursive(string dirName)
        {
            var files =
                _filenamepatterns.SelectMany(
                    pattern =>
                    new DirectoryInfo(dirName).EnumerateFiles(pattern)).Distinct(new CompareFileNames());
            foreach (FileInfo file in files)
            {
                if (!file.Name.StartsWith("._"))
                    yield return file;
            }
            foreach (string directory in Directory.GetDirectories(dirName))
            {
                if (Path.GetFileName(directory) != ".picasaoriginals")
                    foreach (FileInfo file in ScanRecursive(directory))
                        yield return file;
            }
        }

        private class CompareFileNames : IEqualityComparer<FileInfo>
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

        public FileGroup GetRoot()
        {
            return GetRoot(CancellationToken.None,delegate(long l) {  });
        }

        public FileGroup GetRoot(CancellationToken ct, Action<long> Progress)
        {
            long numberOfFiles = 0;
            RootFileGroup root = new RootFileGroup();

            foreach (var folder in _folders)
            {
                root.AddBaseDir(new DirectoryInfo(folder).FullName);
                foreach (FileInfo file in ScanRecursive(folder))
                {
                    ct.ThrowIfCancellationRequested();
                    root.AddFile(new PictureFile(file));
                    numberOfFiles++;
                    if (numberOfFiles%100 == 0)
                        Progress(numberOfFiles);
                }
                
            }
            foreach (var fileName in _fileNames)
            {
                ct.ThrowIfCancellationRequested();
                root.AddFile(new PictureFile(new FileInfo(fileName)));
                numberOfFiles++;
                if (numberOfFiles % 100 == 0)
                    Progress(numberOfFiles);
            }
            return root;
        }
    }
}