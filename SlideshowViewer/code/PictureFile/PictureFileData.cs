using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using ExifLib;

namespace SlideshowViewer.PictureFile
{
    public class PictureFileData
    {
        private readonly bool _animatedGif;
        private readonly string _fileName;
        private bool _error;
        private Image _image;
        private int _rotation;

        public PictureFileData(FileInfo fileInfo)
        {
            _fileName = fileInfo.FullName;

            #region rotation

            _rotation = 0;
            string rotationFilename = _fileName + ".ssv.rotation";
            if (File.Exists(rotationFilename))
                _rotation = Int32.Parse(File.ReadAllText(rotationFilename));

            #endregion

            #region image duration

            _animatedGif = Image.FrameDimensionsList.Any(guid => guid == FrameDimension.Time.Guid);
            ImageDuration = 0;
            if (_animatedGif)
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
                        ImageDuration += frameDuration;
                    }
                }
            }

            #endregion

            ExifReader exifReader = null;
            try
            {
                exifReader = new ExifReader(_fileName);
            }
            catch (Exception)
            {
            }

            #region orientation

            Orientation = ExifGet(exifReader, ExifTags.Orientation, 1);

            #endregion

            #region date time

            DateTime = ExifGet(exifReader, ExifTags.DateTimeOriginal, "");

            #endregion

            #region model

            string model = ExifGet(exifReader, ExifTags.Model, "");
            string make = ExifGet(exifReader, ExifTags.Make, "");

            if (!model.StartsWith(make))
                model = make + " " + model;
            Model = model;

            #endregion

            #region image description

            string imageDescription = ExifGet(exifReader, ExifTags.ImageDescription, "").Trim();
            if (imageDescription.EndsWith("DIGITAL CAMERA"))
                imageDescription = "";
            ImageDescription = imageDescription;

            #endregion

            if (exifReader != null)
                exifReader.Dispose();

            #region description

            Description = ReadExternalDescription(fileInfo);

            #endregion
        }

        private string ReadExternalDescription(FileInfo fileInfo)
        {
            try
            {
                string descriptionFileName = Path.Combine(fileInfo.DirectoryName, ".description");
                if (File.Exists(descriptionFileName))
                {
                    foreach (string line in File.ReadLines(descriptionFileName, Encoding.GetEncoding(0)))
                    {
                        string[] split = line.Split(new[] { '=' }, 2);
                        if (split.Length == 2 && split[0] == fileInfo.Name)
                        {
                            return split[1];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return "Error reading .description: " + e.Message;
            }
            return "";
        }

        public string Description { get; private set; }

        public string ImageDescription { get; private set; }

        public string Model { get; private set; }

        public string DateTime { get; private set; }

        public int ImageDuration { get; private set; }

        internal int Rotation // User initiated rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = (value + 360)%360;
                string rotationFilename = _fileName + ".ssv.rotation";
                if (_rotation == 0)
                    File.Delete(rotationFilename);
                else
                    File.WriteAllText(rotationFilename, _rotation.ToString());
            }
        }

        private int Orientation { get; set; }

        private bool Error
        {
            get { return _error; }
            set
            {
                _error = value;
                string errorFilename = _fileName + ".ssv.error";
                if (!_error)
                    File.Delete(errorFilename);
                else
                    File.WriteAllText(errorFilename, "");
            }
        }

        public Image Image
        {
            get
            {
                LoadImage();
                return _image;
            }
        }


        private static T ExifGet<T>(ExifReader exifReader, ExifTags tag, T defaultValue)
        {
            if (exifReader == null)
                return defaultValue;
            try
            {
                T result;
                if (exifReader.GetTagValue(tag, out result))
                    return result;
            }
            catch (Exception)
            {
            }
            return defaultValue;
        }

        public void UnloadImage()
        {
            if (_image != null)
                _image.Dispose();
            _image = null;
        }

        public void LoadImage()
        {
            if (_image == null)
                try
                {
                    _image = new Bitmap(_fileName);

                    const double maxSideSize = 2000;

                    if (_image.Height > maxSideSize || _image.Width > maxSideSize)
                    {
                        Image oldImage = _image;
                        double scalefactor = Math.Min(maxSideSize/oldImage.Height, maxSideSize/oldImage.Width);
                        _image = new Bitmap(oldImage, (int) Math.Round(oldImage.Width*scalefactor),
                                            (int) Math.Round(oldImage.Height*scalefactor));
                        oldImage.Dispose();
                    }
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
                    switch (Orientation)
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
                    switch (Rotation)
                    {
                        case 90:
                            _image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                            break;
                        case 180:
                            _image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                            break;
                        case 270:
                            _image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Error = true;
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
                                            new Font("Thaoma", 30), Brushes.OrangeRed,
                                            _image.GetBounds(ref pageUnit),
                                            format);
                    }
                }
        }
    }
}