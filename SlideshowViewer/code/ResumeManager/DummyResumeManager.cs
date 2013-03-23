using System.Collections.Generic;

namespace SlideshowViewer
{
    internal class DummyResumeManager : IResumeManager
    {
        public bool IsShown(PictureFile file)
        {
            return false;
        }

        public void SetToNotShown(IEnumerable<PictureFile> files)
        {
        }

        public void SetToShown(PictureFile pictureFile)
        {
        }
    }
}