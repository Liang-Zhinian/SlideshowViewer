using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using ExifLib;

namespace SlideshowViewer
{
    public class PictureFile
    {
        private readonly string _fileName;
        private Image _image;
        private ExifReader _exifReader;

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


        public int GetOrientation()
        {
            UInt16 ret = 1;
            try
            {
                GetExifReader().GetTagValue(ExifTags.Orientation, out ret);
            }
            catch (Exception)
            {
            }
            return ret;
        }

        private ExifReader GetExifReader()
        {
            if (_exifReader == null)
            {
                _exifReader = new ExifReader(_fileName);
            }
            return _exifReader;
        }

        public string GetTagValue(ExifTags tag, string @default)
        {
            try
            {
                string ret;
                if (!GetExifReader().GetTagValue(tag, out ret))
                    return @default;
                return ret;
            }
            catch (ExifLibException)
            {
                return @default;
            }            
        }

        public string GetDateTime()
        {
            return GetTagValue(ExifTags.DateTimeOriginal, "");
        }

        public string GetModel()
        {
            string model = GetTagValue(ExifTags.Model, "");
            string make = GetTagValue(ExifTags.Make, "");

            if (!model.StartsWith(make))
                model = make + " " + model;
            return model;
        }

        public string GetImageDescription()
        {
            var imageDescription = GetTagValue(ExifTags.ImageDescription, "").Trim();
            if (imageDescription.EndsWith("DIGITAL CAMERA"))
                return "";
            return imageDescription;
        }

        public string GetDescription()
        {
            string directoryName = Path.GetDirectoryName(_fileName);
            string name = Path.GetFileName(_fileName);
            Debug.Assert(directoryName != null, "directoryName != null");
            string descriptionFileName = Path.Combine(directoryName, ".description");
            if (File.Exists(descriptionFileName))
            {
                foreach (string line in File.ReadLines(descriptionFileName, Encoding.GetEncoding(0)))
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
/*
1 = Horizontal (normal) 
2 = Mirror horizontal 
3 = Rotate 180 
4 = Mirror vertical 
5 = Mirror horizontal and rotate 270 CW 
6 = Rotate 90 CW 
7 = Mirror horizontal and rotate 90 CW 
8 = Rotate 270 CW
*/
                    switch (GetOrientation())
                    {
                        case 2:
                            _image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                            break;
                        case 3:
                            _image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            break;
                        case 4:
                            _image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                            break;
                        case 5:
                            _image.RotateFlip(RotateFlipType.Rotate270FlipY);
                            break;
                        case 6:
                            _image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            break;
                        case 7:
                            _image.RotateFlip(RotateFlipType.Rotate90FlipY);
                            break;
                        case 8:
                            _image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            break;
                    }
                }
                catch (Exception e)
                {
                    _image = new Bitmap(2000, 2000);
                    using (Graphics graphics = Graphics.FromImage(_image))
                    {
                        var pageUnit = GraphicsUnit.Pixel;
                        var format = new StringFormat(StringFormat.GenericDefault)
                            {
                                LineAlignment = StringAlignment.Center,
                                Alignment = StringAlignment.Center
                            };
                        graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphics.DrawString("Error loading image: " + _fileName + "\n" + e.Message,
                                            new Font("Thaoma", 30), Brushes.OrangeRed, _image.GetBounds(ref pageUnit),
                                            format);
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
            if (_exifReader!=null)
                _exifReader.Dispose();
            _exifReader = null;
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