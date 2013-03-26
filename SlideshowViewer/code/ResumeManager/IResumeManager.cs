using System.Collections.Generic;

namespace SlideshowViewer.ResumeManager
{
    public interface IResumeManager
    {

        bool IsShown(PictureFile.PictureFile file);

        void SetToNotShown(IEnumerable<PictureFile.PictureFile> files);

        void SetToShown(PictureFile.PictureFile pictureFile);
    }
}