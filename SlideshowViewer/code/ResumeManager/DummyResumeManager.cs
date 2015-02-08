using System.Collections.Generic;

namespace SlideshowViewer.ResumeManager
{
    internal class DummyResumeManager : IResumeManager
    {
        public bool IsShown(PictureFile.PictureFile file)
        {
            return false;
        }

        public void SetToNotShown(IEnumerable<PictureFile.PictureFile> files)
        {
        }

        public void SetToShown(PictureFile.PictureFile pictureFile)
        {
        }
    }
}