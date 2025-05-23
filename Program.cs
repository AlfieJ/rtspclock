using System;
using System.Threading;
using System.Threading.Tasks;

namespace rtspclock
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            int framesPerSecond = 4;
            bool toRTSP = true;
            int port = 7000;

            JpegMediaPlayer player = new JpegMediaPlayer(framesPerSecond, port, toRTSP);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;

            Task task = Task.Run(async () =>
            {
                int ms = 1000 / framesPerSecond;
                TimeSpan increment = TimeSpan.FromMilliseconds(ms);

                JpegText2StreamWriter jpegWriter = new JpegText2StreamWriter(800, 600);
                StreamToPipeWriter streamWriter = new StreamToPipeWriter(jpegWriter.JpegStream, player.Writer);

                while (!token.IsCancellationRequested)
                {
                    DateTime date = DateTime.Now;
                    jpegWriter.DrawText(() => date.ToString("hh:mm:ss.fff\nyyyy-MM-dd"));

                    (bool isCompleted, bool isCanceled) result = await streamWriter.Write(token);

                    if (result.isCompleted || result.isCanceled)
                        break;

                    TimeSpan duration = DateTime.Now - date;
                    TimeSpan sleep = increment - duration;

                    if (sleep.TotalMilliseconds > 0)
                        Thread.Sleep(sleep);
                }

                await player.Writer.CompleteAsync();
            });

            player.Player.Play();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

            cancellationTokenSource.Cancel();
            task.Wait();
        }
    }
}
