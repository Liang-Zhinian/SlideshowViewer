using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brushes = System.Drawing.Brushes;

namespace SlideshowViewer.PictureFile
{
   
    public class PictureFileData
    {
        private static readonly List<string> _propertyNames =
            new List<string>(PhotoMetadataHelper.GetPropertyDefinitions());

        private static readonly Dictionary<string, string> _additionalPropertyNames = new Dictionary<string, string>()
            {
                {"/app1/{ushort=0}/{ushort=34665}/{ushort=42036}","Photo.Lens"}
            }; 

        private string _fileName;
        private readonly SortedDictionary<string, object> _properties = new SortedDictionary<string, object>();
        private bool _error;
        private Image _image;
        private int _rotation;

        public PictureFileData(FileInfo fileInfo)
        {
            _fileName = fileInfo.FullName;

            foreach (PropertyInfo propertyInfo in typeof (FileInfo).GetProperties())
            {
                _properties["File." + propertyInfo.Name] = propertyInfo.GetMethod.Invoke(fileInfo, new object[0]);
            }

            string rotationFilename = _fileName + ".ssv.rotation";
            _rotation = 0;
            if (File.Exists(rotationFilename))
                _rotation = Int32.Parse(File.ReadAllText(rotationFilename));

            _properties["Program.Rotation"] = new DynamicProperty(() => _rotation == 0 ? "" : _rotation.ToString());

            string externalDescription = ReadExternalDescription(fileInfo);
            if (externalDescription != null)
                _properties["DotDescription.Description"] = externalDescription;

            _properties["Formatted.Description"] = new DynamicProperty(delegate
                {
                    string maker = Get("CameraManufacturer", "").ToString().FirstWord().Trim().ToLower();
                    string ret = "";
                    foreach (string propertyName in Utils.Array("Title", "Subject", "DotDescription.Description"))
                    {
                        string s = Get(propertyName, "").ToString().Trim();
                        if (!ret.Contains(s) && (maker.IsEmpty() || !s.ToLower().StartsWith(maker)))
                            ret += "\n" + s;
                    }
                    return String.Join("\n", ret.SplitIntoLines().Where(s => !s.IsEmpty()));
                });

            _properties["Formatted.MakeAndModel"] = new DynamicProperty(delegate
                {
                    string maker = Get("CameraManufacturer", "").ToString().Trim();
                    string model = Get("CameraModel", "").ToString().Trim();
                    if (model.IsEmpty() || !maker.ToLower().Contains(model.FirstWord().ToLower()))
                        return (maker + " " + model).Trim();
                    return model;
                });
            _properties["Formatted.CameraDescription"] = new DynamicProperty(delegate
                {
                    string camera = Get("Formatted.MakeAndModel", "").ToString();
                    string lens = Get("Photo.Lens", "").ToString();
                    if (!lens.IsEmpty())
                    {
                        return camera + " using lens " + lens;
                    }
                    return camera + lens;
                });
            _properties["Formatted.ExposureTime"] = new DynamicProperty(delegate
                {
                    string ret = "";
                    KeyValuePair<string, object>? exposureTimeProp = GetProperty("System.Photo.ExposureTime");
                    if (exposureTimeProp != null)
                    {
                        var exposureTime = (double) ((KeyValuePair<String, object>) exposureTimeProp).Value;
                        if ((1/exposureTime)%1 == 0 && exposureTime != 1)
                            ret += "1/" + (1/exposureTime);
                        else
                            ret += exposureTime;
                        ret += " sec";
                    }
                    return ret;
                });
            _properties["Formatted.Aperture"] = new DynamicProperty(delegate
                {
                    string ret = "";
                    KeyValuePair<string, object>? fNumberProp = GetProperty("System.Photo.FNumber");
                    if (fNumberProp != null)
                    {
                        var fNumber = (double) ((KeyValuePair<String, object>) fNumberProp).Value;
                        ret += "f / " + fNumber;
                    }
                    return ret;
                });
            _properties["Formatted.ISO"] = new DynamicProperty(delegate
                {
                    string ret = "";
                    KeyValuePair<string, object>? isoProp = GetProperty("System.Photo.ISOSpeed");
                    if (isoProp != null)
                    {
                        var iso = (ushort) ((KeyValuePair<String, object>) isoProp).Value;
                        ret += "ISO " + iso;
                    }
                    return ret;
                });

            _properties["Formatted.FocalLength"] = new DynamicProperty(delegate
                {
                    string ret = "";
                    KeyValuePair<string, object>? focalLengthProp = GetProperty("System.Photo.FocalLength");
                    if (focalLengthProp != null)
                    {
                        var focalLength = (double) ((KeyValuePair<String, object>) focalLengthProp).Value;
                        ret += "" + focalLength + " mm";
                    }
                    return ret;
                });

            _properties["Formatted.Exposure"] = new DynamicProperty(delegate
                {
                    string ret = "";
                    ret += Get("Formatted.ExposureTime", "");
                    string aperture = Get("Formatted.Aperture", "").ToString();
                    if (!aperture.IsEmpty())
                    {
                        if (!ret.IsEmpty())
                            ret += " at ";
                        ret += aperture;
                    }
                    string iso = Get("Formatted.ISO", "").ToString();
                    if (!iso.IsEmpty())
                    {
                        if (!ret.IsEmpty())
                            ret += ", ";
                        ret += iso;
                    }
                    string focalLength = Get("Formatted.FocalLength", "").ToString();
                    if (!focalLength.IsEmpty())
                    {
                        if (!ret.IsEmpty())
                            ret += ", ";
                        ret += focalLength;
                    }
                    return ret;
                });


            try
            {
                using (
                    var bitmapStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read)
                    )
                {
                    BitmapDecoder bitmapDecoder =
                        BitmapDecoder.Create(bitmapStream,
                                             BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

                    var bitmapMetadata = (BitmapMetadata) bitmapDecoder.Frames[0].Metadata;
                    foreach (string propertyName in _propertyNames)
                    {
                        CopyMetadataToProperties(bitmapMetadata, propertyName, propertyName);
                    }
                    foreach (var entry in _additionalPropertyNames)
                    {
                        CopyMetadataToProperties(bitmapMetadata, entry.Key, entry.Value);
                    }
                    foreach (var entry in CaptureMetadata(bitmapMetadata, String.Empty))
                    {
                        Debug.WriteLine("-- "+entry.Key + ": '" + entry.Value + "'");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

            bool animatedGif = Image.FrameDimensionsList.Any(guid => guid == FrameDimension.Time.Guid);
            ImageDuration = 0;
            if (animatedGif)
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

            _properties["Program.ImageDuration"] = new DynamicProperty(() => ImageDuration.ToString());

            foreach (var property in _properties)
            {
                Debug.WriteLine(property.Key + ": '" + property.Value + "'");
            }
        }

        [HandleProcessCorruptedStateExceptions]
        private void CopyMetadataToProperties(BitmapMetadata bitmapMetadata, string query, string propertyName)
        {
            try
            {
                if (bitmapMetadata.ContainsQuery(query))
                {
                    object value = bitmapMetadata.GetQuery(query);
                    _properties[propertyName] = TranslateBitmapMetadataValue(value);
                }
            }
            catch (AccessViolationException e)
            {
                Debug.WriteLine(e);
            }
        }

        private static object TranslateBitmapMetadataValue(object value)
        {
            if (value is FILETIME)
            {
                return ((FILETIME) value).ToDateTime();
            }
            if (value is String[])
            {
                return String.Join(", ", (String[]) value);
            }
            return value;
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

        private Dictionary<string, object> CaptureMetadata(ImageMetadata imageMetadata, string query)
        {
            var ret = new Dictionary<string, object>();
            var bitmapMetadata = imageMetadata as BitmapMetadata;

            if (bitmapMetadata != null)
            {
                foreach (string relativeQuery in bitmapMetadata)
                {
                    string fullQuery = query + relativeQuery;
                    object metadataQueryReader = bitmapMetadata.GetQuery(relativeQuery);
                    var innerBitmapMetadata = metadataQueryReader as BitmapMetadata;
                    if (innerBitmapMetadata != null)
                    {
                        ret.AddAll(CaptureMetadata(innerBitmapMetadata, fullQuery));
                    }
                    else if (!(metadataQueryReader is BitmapMetadataBlob))
                    {
                        ret[fullQuery] = metadataQueryReader;
                    }
                }
            }
            return ret;
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
                if (property.Key.ToLower().EndsWith("." + propertyName.ToLower()))
                    return property;
            }
            return null;
        }

        public object Get(string propertyName, object @default)
        {
            KeyValuePair<string, object>? keyValuePair = GetProperty(propertyName);
            if (keyValuePair == null)
                return @default;
            return ((KeyValuePair<string, object>) keyValuePair).Value??"";
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

                    _properties["Image.Megapixels"] = _image.Width*_image.Height/1000000.0;
                    _properties["Image.Width"] = _image.Width;
                    _properties["Image.Height"] = _image.Height;

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
                    object o = Get("orientation", "1");
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

        private class DynamicProperty
        {
            private readonly Func<string> _toString;

            public DynamicProperty(Func<string> toString)
            {
                _toString = toString;
            }

            public override string ToString()
            {
                return _toString();
            }
        }
    }
}