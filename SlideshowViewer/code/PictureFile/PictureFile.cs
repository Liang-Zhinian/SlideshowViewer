using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace SlideshowViewer.PictureFile
{
    public class PictureFile : IComparable<PictureFile>
    {
        private readonly FileInfo _fileInfo;
        private PictureFileData _data;


        static PictureFile()
        {
            FileNamePatterns = ImageCodecInfo.GetImageEncoders().SelectMany(info => info.FilenameExtension.Split(';'));
        }

        public PictureFile(FileInfo fileInfo)
        {
            _fileInfo = fileInfo;
        }


        public PictureFileData Data
        {
            get
            {
                if (_data == null)
                    _data = new PictureFileData(FileInfo);
                return _data;
            }
            set { _data = value; }
        }

        public string FileName
        {
            get { return _fileInfo.FullName; }
        }

        public long FileSize
        {
            get { return _fileInfo.Length; }
        }

        public object ModifiedDate
        {
            get { return _fileInfo.LastWriteTime; }
        }

        public FileInfo FileInfo
        {
            get { return _fileInfo; }
        }

        public static IEnumerable<string> FileNamePatterns { get; private set; }


        public void RotateLeft()
        {
            Data.Image.RotateFlip(RotateFlipType.Rotate270FlipNone);
            Data.Rotation -= 90;
        }

        public void RotateRight()
        {
            Data.Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
            Data.Rotation += 90;
        }

        public void UnloadImage()
        {
            if (HasData())
            {
                Data.UnloadImage();
            }
        }

        public bool HasData()
        {
            return _data != null;
        }

        public void LoadImage()
        {
            Data.LoadImage();
        }

        #region Comparison

        public int CompareTo(PictureFile other)
        {
            return String.Compare(FileName.ToUpper(), other.FileName.ToUpper(), StringComparison.Ordinal);
        }

        protected bool Equals(PictureFile other)
        {
            return Equals(FileName.ToUpper(), other.FileName.ToUpper());
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PictureFile) obj);
        }

        public override int GetHashCode()
        {
            return FileName.ToUpper().GetHashCode();
        }

        public static bool operator ==(PictureFile left, PictureFile right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PictureFile left, PictureFile right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}