using Emgu.CV;
using Emgu.CV.Structure;
using Intel.RealSense;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace HciLab.Kinect
{
    public class RealSenseManager
    {

        private static RealSenseManager mInstance;

        private Pipeline pipeline;
        private Colorizer colorizer;
        private CancellationTokenSource tokenSource = new CancellationTokenSource();

        public static RealSenseManager Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new RealSenseManager();
                }
                return mInstance;
            }
        }

        public RealSenseManager()
        {

        }

        public void StartRealSenseD415Capturing()
        {
            try
            {
                pipeline = new Pipeline();
                colorizer = new Colorizer();

                var cfg = new Config();
                cfg.EnableStream(Stream.Depth, 1280, 720);
                cfg.EnableStream(Stream.Color, Format.Rgb8);

                pipeline.Start(cfg);

                var token = tokenSource.Token;

                var t = Task.Factory.StartNew(() =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        var frames = pipeline.WaitForFrames();
                        //var colorized_depth = colorizer.Colorize(frames.DepthFrame);
                        // TODO: Add event to cameramanager

                        byte[] byteDepthArray = new byte[frames.DepthFrame.Stride * frames.DepthFrame.Height];
                        frames.DepthFrame.CopyTo(byteDepthArray);
                        Image<Gray, Int16> depthImage = new Image<Gray, Int16>(frames.DepthFrame.Width, frames.DepthFrame.Height);
                        depthImage.Bytes = byteDepthArray;
                        //CameraManager.Instance.SetImages(colorFrame, colorFrame, xFrame, xFrame)
                    }
                }, token);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during RealSense D415 startup");
            }
        }

        //private void UploadImage(Image img, VideoFrame frame)
        //{
        //    Dispatcher.Invoke(new Action(() =>
        //    {
        //        if (frame.Width == 0) return;

        //        var bytes = new byte[frame.Stride * frame.Height];
        //        frame.CopyTo(bytes);

        //        var bs = BitmapSource.Create(frame.Width, frame.Height,
        //                          300, 300,
        //                          PixelFormats.Rgb24,
        //                          null,
        //                          bytes,
        //                          frame.Stride);

        //        var imgSrc = bs as ImageSource;

        //        img.Source = imgSrc;
        //    }));
        //}

        public void StopRealSenseD415Capturing()
        {
            tokenSource.Cancel();
        }
    }
}
