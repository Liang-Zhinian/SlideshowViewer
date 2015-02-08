using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using SlideshowViewer.code.PictureViewer;

namespace SlideshowViewer
{
    public abstract class MyPicture
    {

        public static MyPicture create(Image image, Rectangle bounds)
        {
            if (image.IsAnimated())
                return new AnimatedMyPicture(image, bounds);
            return new StaticMyPicture(image, bounds);
        }

        protected Bitmap RenderImage(Image image, Bitmap bitmap, bool highQuality = true)
        {
            using (var graphic = Graphics.FromImage(bitmap))
            {
                if (highQuality)
                {
                    graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphic.SmoothingMode = SmoothingMode.HighQuality;
                    graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphic.CompositingQuality = CompositingQuality.HighQuality;
                }

                var solidBrush = new SolidBrush(Color.Black);
                var clipBounds = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                graphic.FillRectangle(solidBrush, clipBounds);

                var scaledImage = FitToRectangle(new Rectangle(0, 0, image.Width, image.Height), clipBounds);
                graphic.DrawImage(image, scaledImage);
            }
            return bitmap;
        }

        private static Rectangle FitToRectangle(Rectangle inner, Rectangle outer)
        {
            double scaleY = (double) inner.Height/outer.Height;
            double scaleX = (double) inner.Width/outer.Width;
            double scale = scaleX > scaleY ? scaleX : scaleY;
            var scaledImage = new Rectangle(0, 0, (int) Math.Round(inner.Width/scale),
                (int) Math.Round(inner.Height/scale));
            scaledImage.Y = (int) Math.Round(((double) outer.Height - scaledImage.Height)/2);
            scaledImage.X = (int) Math.Round(((double) outer.Width - scaledImage.Width)/2);
            return scaledImage;
        }

        public abstract Image GetRenderedImage();

        public abstract bool StartAnimate();

    }

    internal class StaticMyPicture : MyPicture
    {
        private Bitmap _renderImage;

        internal StaticMyPicture(Image image, Rectangle bounds)
        {
            _renderImage = RenderImage(image, new Bitmap(bounds.Width, bounds.Height));
        }

        public override Image GetRenderedImage()
        {
            return _renderImage;
        }

        public override bool StartAnimate()
        {
            return false;
        }
    }

    internal class AnimatedMyPicture : MyPicture
    {
        private Rectangle _bounds;
        private List<Bitmap> _image;
        private List<ImageFrame> _imageFrames;
        private Stopwatch _stopwatch;
        private int prevIndex = 0;
        
        protected internal AnimatedMyPicture(Image image, Rectangle bounds)
        {
            _bounds = bounds;
            _imageFrames = image.GetFrames();
            _image=new List<Bitmap>(_imageFrames.Select(frame => (Bitmap)null));
        }

        private int GetIndex()
        {
            if (_stopwatch == null)
                return 0;

            long counter = 0;
            for (int i = 0;;)
            {
                if (counter + _imageFrames[i].Duration >= _stopwatch.ElapsedMilliseconds)
                {
                    return i;
                }
                counter += _imageFrames[i].Duration;
                i = GetIndex(i + 1);
            }
        }

        private int GetIndex(int i)
        {
            return i%_imageFrames.Count;
        }

        public override Image GetRenderedImage()
        {
            var index = GetIndex();
            if (_image[index] == null)
                RenderImage(index);
            else if (index==prevIndex)
            {
                for (int i = GetIndex(index+1); i != index; i=GetIndex(i+1))
                {
                    if (_image[i] == null)
                    {
                        RenderImage(i);
                        break;
                    }
                }
            }
            prevIndex = index;
            return _image[index];
        }

        private void RenderImage(int index, bool highQuality=false)
        {
            _image[index] = RenderImage(_imageFrames[index].ActivateFrame(),
                new Bitmap(_bounds.Width, _bounds.Height), highQuality);
        }

        public override bool StartAnimate()
        {
            _stopwatch = Stopwatch.StartNew();
            return true;
        }
    }
}