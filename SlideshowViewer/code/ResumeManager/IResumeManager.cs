using System.Collections.Generic;

namespace SlideshowViewer
{
    public interface IResumeManager
    {

        bool IsShown(PictureFile file);

        void SetToNotShown(IEnumerable<PictureFile> files);

        void SetToShown(PictureFile pictureFile);
    }
}