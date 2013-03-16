using System.Collections.Generic;
using System.IO;

namespace SlideshowViewer
{
    public abstract class ResumeManager
    {
        public abstract bool IsShown(PictureFile file);

        public abstract void SetToNotShown(IEnumerable<PictureFile> files);

        public abstract void SetToShown(PictureFile pictureFile);
    }

    internal class FileResumeManager : ResumeManager
    {
        private readonly string _fileName;
        private readonly HashSet<string> _shownFiles = new HashSet<string>();

        public FileResumeManager(string fileName)
        {
            _fileName = fileName;
            if (File.Exists(fileName))
                _shownFiles = new HashSet<string>(File.ReadAllLines(fileName));
        }

        public override bool IsShown(PictureFile file)
        {
            return _shownFiles.Contains(file.FileName);
        }

        public override void SetToNotShown(IEnumerable<PictureFile> files)
        {
            foreach (PictureFile pictureFile in files)
            {
                _shownFiles.Remove(pictureFile.FileName);
            }
            File.WriteAllLines(_fileName, _shownFiles);
        }

        public override void SetToShown(PictureFile pictureFile)
        {
            _shownFiles.Add(pictureFile.FileName);
            File.AppendAllLines(_fileName, new[] {pictureFile.FileName});
        }
    }

    internal class DummyResumeManager : ResumeManager
    {
        public override bool IsShown(PictureFile file)
        {
            return false;
        }

        public override void SetToNotShown(IEnumerable<PictureFile> files)
        {
        }

        public override void SetToShown(PictureFile pictureFile)
        {
        }
    }
}