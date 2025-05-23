using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace rtspclock
{
    /// <summary>
    /// Creates a bitmap of the specified size. Then you draw the
    /// specified text in the center of the bitmap, outputting
    /// the image as a Jpeg to a MemoryStream.
    /// </summary>
    public class JpegText2StreamWriter
    {
        private Bitmap _bitmap;
        private Graphics _graphics;
        private Font _font;
        private StringFormat _strFormat;
        private SolidBrush _brush;
        private Rectangle _padding;

        private MemoryStream _jpegOutputMemoryStream;

        public MemoryStream JpegStream
        { get { return _jpegOutputMemoryStream; } }

        public JpegText2StreamWriter(int width, int height)
        {
            _bitmap = new Bitmap(width, height);
            _graphics = Graphics.FromImage(_bitmap);
            _font = new Font(FontFamily.GenericSansSerif, 48);

            // Center horizontally and vertically
            _strFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            _brush = new SolidBrush(Color.White);
            _padding = new Rectangle(0, 0, width, height);

            _jpegOutputMemoryStream = new MemoryStream();
        }

        public void DrawText(Func<string> func)
        {
            if (func == null)
                return;

            string text = func();

            Console.WriteLine(text);

            _graphics.Clear(Color.DarkGray);
            _graphics.DrawString(text, _font, _brush, _padding, _strFormat);
            _bitmap.Save(_jpegOutputMemoryStream, ImageFormat.Jpeg);
        }
    }
}
