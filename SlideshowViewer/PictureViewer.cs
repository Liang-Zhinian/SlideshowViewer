using System.Collections.Generic;
using SlideshowViewer.Properties;

namespace SlideshowViewer
{
    public class PictureViewer
    {
        public ResumeManager ResumeManager { get; set; }

        public bool Shuffle { get; set; }

        public bool Loop { get; set; }

        public int DelayInSec { get; set; }

        public string Text { get; set; }

        public PictureViewer()
        {
            ResumeManager = new DummyResumeManager();
            Shuffle = false;
            Loop = true;
            DelayInSec = 15;
            Text =
                "{imageDescription}{eol}{description}{eol}{dateTime}{eol}{model}{eol}{fullName}  ( {index} / {total} )";
        }

        public void ShowPictures(IEnumerable<PictureFile> files)
        {
            List<PictureFile> pictureFiles;
            int index = PrepareFileList(files, out pictureFiles);

            using (PictureViewerForm pictureViewerForm = CreatePictureViewForm(pictureFiles, index))
            {
                pictureViewerForm.ShowDialog();
            }
        }

        private int PrepareFileList(IEnumerable<PictureFile> files, out List<PictureFile> pictureFiles)
        {
            var shownFiles = new List<PictureFile>();
            var notShownFiles = new List<PictureFile>();

            foreach (PictureFile file in files)
            {
                if (ResumeManager.IsShown(file))
                    shownFiles.Add(file);
                else
                    notShownFiles.Add(file);
            }

            if (notShownFiles.Count == 0)
            {
                ResumeManager.SetToNotShown(shownFiles);
                notShownFiles = shownFiles;
                shownFiles = new List<PictureFile>();
            }

            pictureFiles = new List<PictureFile>();
            if (Shuffle)
            {
                pictureFiles.AddRange(shownFiles.Shuffle());
                pictureFiles.AddRange(notShownFiles.Shuffle());
            }
            else
            {
                pictureFiles.AddRange(shownFiles);
                pictureFiles.AddRange(notShownFiles);
            }
            return shownFiles.Count;
        }


        private PictureViewerForm CreatePictureViewForm(List<PictureFile> pictureFiles, int fileIndex)
        {
            var pictureViewerForm = new PictureViewerForm();
            pictureViewerForm.Files = pictureFiles;
            pictureViewerForm.FileIndex = fileIndex;
            pictureViewerForm.Loop = Loop;
            pictureViewerForm.DelayInSec = DelayInSec;
            pictureViewerForm.OverlayTextTemplate = Text;
            pictureViewerForm.Icon = Resources.image_x_generic;
            pictureViewerForm.PictureShown = ResumeManager.SetToShown;
            pictureViewerForm.AllPicturesShown = ResumeManager.SetToNotShown;
            return pictureViewerForm;
        }

    }
}