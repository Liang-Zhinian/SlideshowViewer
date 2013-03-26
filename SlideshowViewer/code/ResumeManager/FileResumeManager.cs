using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SlideshowViewer.ResumeManager
{
    internal class FileResumeManager : MemoryResumeManager
    {
        private readonly string _fileName;

        public FileResumeManager(string fileName)
        {
            _fileName = fileName;
            if (File.Exists(fileName))
                _shownFiles = new HashSet<string>(File.ReadAllLines(fileName).Select(s => s.ToUpper()));
        }

        public override void SetToNotShown(IEnumerable<PictureFile.PictureFile> files)
        {
            base.SetToNotShown(files);
            File.WriteAllLines(_fileName, _shownFiles);
        }

        public override void SetToShown(PictureFile.PictureFile pictureFile)
        {
            base.SetToShown(pictureFile);
            File.AppendAllLines(_fileName, new[] { pictureFile.FileName.ToUpper() });
        }
    }
}