using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace SlideshowViewer.code.PictureViewer
{
    public static class ImageUtils
    {
        public static bool IsAnimated(this Image image)
        {
            return image.GetFrames().Any();
        }

        public static List<ImageFrame> GetFrames(this Image image)
        {
            List<ImageFrame> ret = new List<ImageFrame>();
            if (image.FrameDimensionsList.Any(guid => guid == FrameDimension.Time.Guid))
            {
                int frameCount = image.GetFrameCount(FrameDimension.Time);
                if (frameCount > 1)
                {
                    byte[] times = image.GetPropertyItem(0x5100).Value;
                    for (int i = 0; i < frameCount; ++i)
                    {
                        int frameDuration = BitConverter.ToInt32(times, 4*i)*10;
                        if (frameDuration < 20)
                            frameDuration = 100;
                        ret.Add(new ImageFrame(image, i, frameDuration));
                    }
                }
            }
            return ret;
        }
    }


    public class ImageFrame
    {
        private readonly Image _image;
        private readonly int _index;
        private readonly int _duration;

        public ImageFrame(Image image, int index, int duration)
        {
            _image = image;
            _index = index;
            _duration = duration;
        }

        public Image ActivateFrame()
        {
            _image.SelectActiveFrame(FrameDimension.Time, _index);
            return _image;
        }

        public int Duration
        {
            get { return _duration; }
        }
    }


}