using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;

namespace rtspclock
{
    /// <summary>
    /// Creates a VLC MediaPlayer that's used to play individual
    /// JPEG images. Use the PipeWriter to output the invidual
    /// frames.
    /// </summary>
    public class JpegMediaPlayer
    {
        private LibVLC _libVLC;
        private Pipe _pipe;
        private PipeMediaInput _mediaInput;
        private Media _media;
        private MediaPlayer _mp;

        public PipeWriter Writer
        { get { return _pipe.Writer; } }

        public MediaPlayer Player
        { get { return _mp; } }

        public JpegMediaPlayer(int fps, int port, bool playToRTSP = false)
        {
            List<string> options = new List<string>();

            if (playToRTSP)
            {
                options.AddRange(new string[]
                    {
                        $":sout=#transcode{{vcodec=h264,fps={fps}}}:rtp{{sdp=rtsp://127.0.0.1:{port}}}",
                        ":no-sout-all",
                        ":sout-keep"
                    });
            }

            options.ForEach(o => Console.WriteLine(o));

            _libVLC = new LibVLC(false, "--demux=mjpeg");
            _pipe = new Pipe();
            _mediaInput = new PipeMediaInput(_pipe.Reader);
            _media = new Media(_libVLC, _mediaInput, options.ToArray());
            _mp = new MediaPlayer(_media);
        }
    }
}
