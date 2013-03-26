using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows.Media.Imaging;
using ExifLib;

namespace SlideshowViewer.PictureFile
{
    public class PictureFileData
    {
        private static readonly List<string> _propertyNames =
            new List<string>(PropertySystemHelper.GetPropertyDefinitions());

        private readonly string _fileName;
        private readonly SortedDictionary<string, object> _properties = new SortedDictionary<string, object>();
        private bool _error;
        private Image _image;
        private int _rotation;

        private class DynamicProperty
        {
            private Func<string> _toString;

            public DynamicProperty(Func<string> toString)
            {
                _toString = toString;
            }

            public override string ToString()
            {
                return _toString();
            }
        }

        public PictureFileData(FileInfo fileInfo)
        {
            _fileName = fileInfo.FullName;

            foreach (var propertyInfo in typeof(FileInfo).GetProperties())
            {
                _properties["File." + propertyInfo.Name] = propertyInfo.GetMethod.Invoke(fileInfo, new object[0]);
            }

            string rotationFilename = _fileName + ".ssv.rotation";
            _rotation = 0;
            if (File.Exists(rotationFilename))
                _rotation = Int32.Parse(File.ReadAllText(rotationFilename));

            _properties["Program.Rotation"] = new DynamicProperty(() => _rotation == 0 ? "" : _rotation.ToString());

            var externalDescription = ReadExternalDescription(fileInfo);
            if (externalDescription != null)
                _properties["DotDescription.Description"] = externalDescription;

            _properties["Description"] = new DynamicProperty(delegate
                {
                    var maker = Get("CameraManufacturer", "").ToString().FirstWord().Trim().ToLower();
                    string ret = "";
                    foreach (var propertyName in Utils.Array("Title", "Subject", "DotDescription.Description"))
                    {
                        var s = Get(propertyName, "").ToString().Trim();
                        if (!ret.Contains(s) && (maker.IsEmpty() || !s.ToLower().StartsWith(maker)))
                            ret += "\n" + s;
                    }
                    return String.Join("\n",ret.SplitIntoLines().Where(s => !s.IsEmpty()));
                });

            _properties["MakeAndModel"] = new DynamicProperty(delegate
            {
                var maker = Get("CameraManufacturer", "").ToString().Trim();
                var makerFirstWord = maker.FirstWord().Trim().ToLower();
                var model = Get("CameraModel", "").ToString().Trim();
                if (!model.ToLower().Contains(makerFirstWord))
                    return maker + " " + model;
                return model;
            });


            try
            {
                using (var bitmapStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BitmapDecoder bitmapDecoder =
                        BitmapDecoder.Create(bitmapStream,
                                             BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

                    var bitmapMetadata = (BitmapMetadata)bitmapDecoder.Frames[0].Metadata;
                    foreach (string propertyName in _propertyNames)
                    {
                        if (bitmapMetadata.ContainsQuery(propertyName))
                        {
                            object value = bitmapMetadata.GetQuery(propertyName);
                            if (value is FILETIME)
                            {
                                value = ((FILETIME)value).ToDateTime();
                            }
                            if (value is String[])
                            {
                                value = String.Join(", ", (String[])value);
                            }
                            _properties[propertyName] = value;
                        }
                    }                    
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

            var animatedGif = Image.FrameDimensionsList.Any(guid => guid == FrameDimension.Time.Guid);
            ImageDuration = 0;
            if (animatedGif)
            {
                int frameCount = Image.GetFrameCount(FrameDimension.Time);
                if (frameCount > 1)
                {
                    byte[] times = Image.GetPropertyItem(0x5100).Value;
                    for (int i = 0; i < frameCount; ++i)
                    {
                        int frameDuration = BitConverter.ToInt32(times, 4 * i) * 10;
                        if (frameDuration < 50)
                            frameDuration = 50;
                        ImageDuration += frameDuration;
                    }
                }
            }

            _properties["Program.ImageDuration"] = new DynamicProperty(() => ImageDuration.ToString());

            foreach (var property in _properties)
            {
                Debug.WriteLine(property.Key + ": '" + property.Value + "'");
            }
        }


        public IEnumerable<KeyValuePair<string, object>> Properties
        {
            get { return _properties; }
        }


        public int ImageDuration { get; private set; }

        internal int Rotation // User initiated rotation
        {
            get { return _rotation; }
            set
            {
                int rotation = (value + 360)%360;
                string rotationFilename = _fileName + ".ssv.rotation";
                if (rotation == 0)
                    File.Delete(rotationFilename);
                else
                    File.WriteAllText(rotationFilename, rotation.ToString());
                _rotation = rotation;
            }
        }


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


        private KeyValuePair<string, object>? GetProperty(string propertyName)
        {
            foreach (var property in _properties)
            {
                if (property.Key.ToLower().Equals(propertyName.ToLower()))
                    return property;
            }
            foreach (var property in _properties)
            {
                if (property.Key.ToLower().EndsWith("."+propertyName.ToLower()))
                    return property;
            }
            return null;
        }

        public object Get(string propertyName, object @default)
        {
            KeyValuePair<string, object>? keyValuePair = GetProperty(propertyName);
            if (keyValuePair == null)
                return @default;
            return ((KeyValuePair<string, object>)keyValuePair).Value;
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
                        string[] split = line.Split(new[] {'='}, 2);
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
            return null;
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
                    var o = Get("orientation", "1");
                    switch (int.Parse(o.ToString()))
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