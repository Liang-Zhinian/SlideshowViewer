using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                _shownFiles = new HashSet<string>(File.ReadAllLines(fileName).Select(s => s.ToUpper()));
        }

        public bool IsShown(PictureFile.PictureFile file)
        {
            return _shownFiles.Contains(file.FileName.ToUpper());
        }

        public void SetToNotShown(IEnumerable<PictureFile.PictureFile> files)
        {
            foreach (PictureFile.PictureFile pictureFile in files)
            {
                _shownFiles.Remove(pictureFile.FileName.ToUpper());
            }
            File.WriteAllLines(_fileName, _shownFiles);
        }

        public void SetToShown(PictureFile.PictureFile pictureFile)
        {
            _shownFiles.Add(pictureFile.FileName.ToUpper());
            File.AppendAllLines(_fileName, new[] {pictureFile.FileName.ToUpper()});
        }
    }
}