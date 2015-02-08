using System.Collections.Generic;

namespace SlideshowViewer.ResumeManager
{
    internal class MemoryResumeManager : IResumeManager
    {
        protected HashSet<string> _shownFiles = new HashSet<string>();

        public bool IsShown(PictureFile.PictureFile file)
        {
            return _shownFiles.Contains(file.FileName.ToUpper());
        }

        public virtual void SetToNotShown(IEnumerable<PictureFile.PictureFile> files)
        {
            foreach (PictureFile.PictureFile pictureFile in files)
            {
                _shownFiles.Remove(pictureFile.FileName.ToUpper());
            }
        }

        public virtual void SetToShown(PictureFile.PictureFile pictureFile)
        {
            _shownFiles.Add(pictureFile.FileName.ToUpper());
        }
    }
}