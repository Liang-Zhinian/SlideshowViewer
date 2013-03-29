using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SlideshowViewer.PictureFile
{
    public class PictureFile : IComparable<PictureFile>
    {
        private readonly FileInfo _fileInfo;
        internal Task<PictureFileData> _dataTask;
        private readonly object _dataTaskLock=new object();


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
                LoadData();
                return _dataTask.Result;
            }
        }

        private void LoadData()
        {
            lock (_dataTaskLock)
            {
                if (_dataTask == null)
                {
                    _dataTask = new Task<PictureFileData>(() => new PictureFileData(FileInfo));
                    _dataTask.Start();
                }
            }
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
            if (HasInternalDataTask())
            {
                Data.UnloadImage();
            }
        }

        public bool HasData()
        {
            return HasInternalDataTask() && _dataTask.IsCompleted;
        }

        public void LoadImage()
        {
            if (HasData())
                Data.LoadImage();
            else
                LoadData();
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

        public bool HasInternalDataTask()
        {
            return _dataTask != null;
        }

        public void SetInternalDataTaskIfNotSet(Task<PictureFileData> task)
        {
            lock (_dataTaskLock)
            {
                if (!HasInternalDataTask())
                    _dataTask = task;
            }
        }
    }
}