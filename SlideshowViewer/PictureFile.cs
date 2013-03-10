using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace SlideshowViewer
{
    public class PictureFile
    {
        private readonly string _fileName;
        private Image _image;

        public PictureFile(string fileName)
        {
            _fileName = fileName;
        }

        public string FileName
        {
            get { return _fileName; }
        }

        public Image Image
        {
            get { return GetImage(); }
        }


        public string GetDateTime()
        {
            PropertyItem propertyItem = null;
            try
            {
                propertyItem = Image.GetPropertyItem(36867);
            }
            catch (ArgumentException)
            {
            }
            return propertyItem != null ? Encoding.ASCII.GetString(propertyItem.Value) : "";
        }

        public string GetModel()
        {
            PropertyItem propertyItem = null;
            try
            {
                propertyItem = Image.GetPropertyItem(272);
            }
            catch (ArgumentException)
            {
            }
            if (propertyItem != null)
                return Encoding.ASCII.GetString(propertyItem.Value);
            return "";
        }

        public string GetDescription()
        {
            string directoryName = Path.GetDirectoryName(_fileName);
            string name = Path.GetFileName(_fileName);
            Debug.Assert(directoryName != null, "directoryName != null");
            string descriptionFileName = Path.Combine(directoryName, ".description");
            if (File.Exists(descriptionFileName))
            {
                foreach (string line in File.ReadLines(descriptionFileName,Encoding.UTF8))
                {
                    string[] split = line.Split(new[] {'='}, 2);
                    if (split.Length == 2 && split[0] == name)
                    {
                        return split[1];
                    }
                }
            }
            return "";
        }

        public Image GetImage()
        {
            if (_image == null)
                try
                {
                    _image = new Bitmap(_fileName);
                }
                catch (Exception e)
                {
                    _image = new Bitmap(2000, 2000);
                    using (Graphics graphics = Graphics.FromImage(_image))
                    {
                        var pageUnit = GraphicsUnit.Pixel;
                        var format = StringFormat.GenericDefault;
                        format.LineAlignment = StringAlignment.Center;
                        format.Alignment = StringAlignment.Center;
                        graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphics.DrawString("Error loading image: " + _fileName + "\n" + e.Message, new Font("Thaoma", 20), Brushes.OrangeRed, _image.GetBounds(ref pageUnit), format);
                        
                    }
                }
            return _image;
        }

        public bool HasImage()
        {
            return _image != null;
        }

        public void UnloadImage()
        {
            if (_image != null)
                _image.Dispose();
            _image = null;
        }


        public int GetImageDuration()
        {
            int imageDuration = 0;
            if (IsAnimatedGif(Image))
            {
                int frameCount = Image.GetFrameCount(FrameDimension.Time);
                if (frameCount > 1)
                {
                    byte[] times = Image.GetPropertyItem(0x5100).Value;
                    for (int i = 0; i < frameCount; ++i)
                    {
                        int frameDuration = BitConverter.ToInt32(times, 4*i)*10;
                        if (frameDuration < 50)
                            frameDuration = 50;
                        imageDuration += frameDuration;
                    }
                }
            }
            return imageDuration;
        }

        private bool IsAnimatedGif(Image bitmap)
        {
            return bitmap.FrameDimensionsList.Any(guid => guid == FrameDimension.Time.Guid);
        }
    }
}