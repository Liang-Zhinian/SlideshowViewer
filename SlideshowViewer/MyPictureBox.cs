using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace SlideshowViewer
{
    public class MyPictureBox : PictureBox
    {
        private string _lowerLeftText;
        private string _lowerMiddleText;
        private string _lowerRightText;

        public MyPictureBox()
        {
            OverlayAlpha = 100;
            OverlayFont = DefaultFont;
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
            if (HighQuality)
            {
                graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphic.SmoothingMode = SmoothingMode.HighQuality;
                graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphic.CompositingQuality = CompositingQuality.HighQuality;
            }
            base.OnPaint(pe);
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
    }
}