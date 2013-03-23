using System.Collections.Generic;
using System.IO;

namespace SlideshowViewer
{
    internal class FileResumeManager : IResumeManager
    {
        private readonly string _fileName;
        private readonly HashSet<string> _shownFiles = new HashSet<string>();

        public FileResumeManager(string fileName)
        {
            _fileName = fileName;
            if (File.Exists(fileName))
                _shownFiles = new HashSet<string>(File.ReadAllLines(fileName));
        }

        public bool IsShown(PictureFile file)
        {
            return _shownFiles.Contains(file.FileName);
        }

        public void SetToNotShown(IEnumerable<PictureFile> files)
        {
            foreach (PictureFile pictureFile in files)
            {
                _shownFiles.Remove(pictureFile.FileName);
            }
            File.WriteAllLines(_fileName, _shownFiles);
        }

        public void SetToShown(PictureFile pictureFile)
        {
            _shownFiles.Add(pictureFile.FileName);
            File.AppendAllLines(_fileName, new[] {pictureFile.FileName});
        }
    }
}