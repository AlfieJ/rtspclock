using LibVLCSharp.Shared;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace rtspclock
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var libVLC = new LibVLC(enableDebugLogs: true);
            libVLC.Log += (obj, eventArgs) =>
            {
                Console.WriteLine("libVLC: " + eventArgs.Message);
            };

            var stream = new MemoryStream();
            var streamMediaInput = new StreamMediaInput(stream);

            int width = 1920;
            int height = 1024;

            var bitmap = new Bitmap(width, height);
            var graphics = Graphics.FromImage(bitmap);
            var font = new Font(FontFamily.GenericSansSerif, 24);

            // Center horizontally and vertically
            var strFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            var brush = new SolidBrush(Color.White);
            var padding = new Rectangle(0, 0, width, height);

            Task.Run(() =>
            {
                while (true)
                {
                    graphics.Clear(Color.DarkGray);

                    var dt = DateTime.Now.ToString("yyyy-MM-dd\nhh:mm:ss.fff");
                    graphics.DrawString(dt, font, brush, padding, strFormat);

                    bitmap.Save(stream, ImageFormat.Jpeg);

                    Thread.Sleep(500);
                }
            });

            var player = new MediaPlayer(libVLC)
            {
                EnableHardwareDecoding = true,
            };
            player.EncounteredError += (_, err) =>
            {
                Console.Error.WriteLine("MediaPlayer: " + err.ToString());
            };

            string[] options =
            {
                ":sout=#rtp{{sdp=rtsp://127.0.0.1:7000}}",
                ":no-sout-all",
                ":sout-keep",
            };

            var media = new Media(libVLC, streamMediaInput, options);

            player.Play(media);

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
