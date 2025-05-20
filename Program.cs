using LibVLCSharp.Shared;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace rtspclock
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var libVLC = new LibVLC(true, "--demux=mjpeg");
            libVLC.Log += (obj, eventArgs) =>
            {
                Console.WriteLine("libVLC: " + eventArgs.Message);
            };

            var pipe = new Pipe();
            var mediaInput = new PipeMediaInput(pipe.Reader);

            var stream = new MemoryStream();
            var streamToMemory = new StreamToMemory(stream);

            int width = 1920;
            int height = 1024;

            var bitmap = new Bitmap(width, height);
            var graphics = Graphics.FromImage(bitmap);
            var font = new Font(FontFamily.GenericSansSerif, 24);

            // Center horizontally and vertically
            var strFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            var brush = new SolidBrush(Color.White);
            var padding = new Rectangle(0, 0, width, height);

            Task.Run(async () =>
            {
                var interval = TimeSpan.FromMilliseconds(1000);

                while (true)
                {
                    graphics.Clear(Color.DarkGray);

                    var now = DateTime.Now;
                    var dt = now.ToString("hh:mm:ss.fff\nyyyy-MM-dd");
                    graphics.DrawString(dt, font, brush, padding, strFormat);

                    bitmap.Save(stream, ImageFormat.Jpeg);
                    var length = stream.Position;
                    Memory<byte> memory = pipe.Writer.GetMemory((int)length);

                    int bytesRead = streamToMemory.Copy(ref memory, (int)length);

                    pipe.Writer.Advance(bytesRead);
                    await pipe.Writer.FlushAsync();

                    stream.SetLength(0);

                    var duration = DateTime.Now - now;
                    var sleep = interval - duration;

                    if (sleep.TotalMilliseconds > 0)
                        Thread.Sleep(sleep);
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

            var media = new Media(libVLC, mediaInput, options);

            player.Play(media);

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
