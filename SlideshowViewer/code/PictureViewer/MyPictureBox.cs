using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;

namespace SlideshowViewer
{
    public class MyPictureBox : Control, ISupportInitialize
    {
        private string _lowerLeftText;
        private string _lowerMiddleText;
        private string _lowerRightText;
        private MyPicture _image;
        private MyPicture _nextImage;
        private Stopwatch _stopwatch=Stopwatch.StartNew();
        private Timer _transitionTimer;
        private float _transitionTime=5000f;

        public MyPictureBox()
        {
            OverlayAlpha = 100;
            OverlayFont = DefaultFont;
            SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.Selectable, false);
            _transitionTimer = new Timer();
            _transitionTimer.Tick += (o, args) => Invalidate();
            _transitionTimer.Interval = 1;
        }

        public void SetImage(Image image)
        {
            _image = MyPicture.create(image, Bounds);
            _nextImage = null;
            _transitionTimer.Enabled = _image.StartAnimate();
            Invalidate();
        }

        public void TransitionImage(Image image, float transitionTime)
        {
            _nextImage=MyPicture.create(image, Bounds);
            _stopwatch = Stopwatch.StartNew();
            _transitionTimer.Enabled = true;
            this._transitionTime = transitionTime;
            Invalidate();
        }

        public string LowerMiddleText
        {
            get { return _lowerMiddleText; }
            set
            {
                _lowerMiddleText = value;
                Invalidate();
            }
        }

        public string LowerLeftText
        {
            get { return _lowerLeftText; }
            set
            {
                _lowerLeftText = value;
                Invalidate();
            }
        }

        public string LowerRightText
        {
            get { return _lowerRightText; }
            set
            {
                _lowerRightText = value;
                Invalidate();
            }
        }

        public int OverlayAlpha { get; set; }

        public Font OverlayFont { get; set; }

        public bool HighQuality { get; set; }


        protected override void OnPaint(PaintEventArgs pe)
        {
            Graphics graphic = pe.Graphics;

            if (_nextImage != null)
            {
                var elapsedMilliseconds = _stopwatch.ElapsedMilliseconds;
                if (elapsedMilliseconds > _transitionTime)
                {
                    _image = _nextImage;
                    _nextImage = null;
                    _transitionTimer.Enabled = _image.StartAnimate();
                    graphic.DrawImageUnscaled(_image.GetRenderedImage(), 0, 0);
                }
                else
                {
                    graphic.DrawImageUnscaled(_image.GetRenderedImage(), 0, 0);

                    var imageAttributes = new ImageAttributes();
                    imageAttributes.SetColorMatrix(new ColorMatrix {Matrix33 = elapsedMilliseconds/_transitionTime});
                    var renderedImage = _nextImage.GetRenderedImage();
                    var bounds = new Rectangle(0, 0, renderedImage.Width, renderedImage.Height);
                    graphic.DrawImage(renderedImage, bounds, 0, 0, bounds.Width, bounds.Height, GraphicsUnit.Pixel,
                        imageAttributes);
                }
            }
            else
            {
                graphic.DrawImageUnscaled(_image.GetRenderedImage(), 0, 0);                
            }


            graphic.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            if (LowerLeftText != null)
            {
                DrawText(graphic, LowerLeftText, StringFormat.GenericDefault,
                         rect => new PointF(rect.X, Bounds.Height - rect.Height));
            }
            if (LowerMiddleText != null)
            {
                var stringFormat = new StringFormat(StringFormat.GenericDefault);
                stringFormat.LineAlignment = StringAlignment.Center;
                DrawText(graphic, LowerMiddleText, stringFormat,
                         rect => new PointF((Bounds.Width - rect.Width)/2, Bounds.Height - rect.Height));
            }
            if (LowerRightText != null)
            {
                var stringFormat = new StringFormat(StringFormat.GenericDefault);
                stringFormat.LineAlignment = StringAlignment.Far;
                DrawText(graphic, LowerRightText, stringFormat,
                         rect => new PointF((Bounds.Width - rect.Width), Bounds.Height - rect.Height));
            }
        }

        private void DrawText(Graphics graphic, string text, StringFormat stringFormat, PlaceText placeText)
        {
            Brush brush = new SolidBrush(Color.FromArgb(OverlayAlpha, 0, 0, 0));
            SizeF sizeF = graphic.MeasureString(text, OverlayFont, Bounds.Width/3, stringFormat);
            var rect = new RectangleF(0, 0, sizeF.Width, sizeF.Height);
            rect.Location = placeText(rect);
            graphic.FillRectangle(brush, rect);
            graphic.DrawString(text, OverlayFont, new SolidBrush(Color.White), rect, stringFormat);
        }

        private delegate PointF PlaceText(RectangleF rect);

        public void BeginInit()
        {
            
        }

        public void EndInit()
        {
            
        }
    }
}