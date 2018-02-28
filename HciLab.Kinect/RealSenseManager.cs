using Emgu.CV;
using Emgu.CV.Structure;
using Intel.RealSense;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
                        VideoFrame depthFrame = colorizer.Colorize(frames.DepthFrame);
                        VideoFrame colorFrame = frames.ColorFrame;

                        if (depthFrame.Width == 0) return;

                        byte[] byteDepthArray = new byte[depthFrame.Stride * depthFrame.Height];
                        byte[] byteColorArray = new byte[colorFrame.Stride * colorFrame.Height];
                        depthFrame.CopyTo(byteDepthArray);
                        colorFrame.CopyTo(byteColorArray);

                        BitmapSource bs_color = BitmapSource.Create(depthFrame.Width, depthFrame.Height,
                            300, 300,
                            System.Windows.Media.PixelFormats.Rgb24,
                            null,
                            byteColorArray,
                            colorFrame.Stride);

                        BitmapSource bs_depth = BitmapSource.Create(depthFrame.Width, depthFrame.Height,
                            300, 300,
                            System.Windows.Media.PixelFormats.Rgb24,
                            null,
                            byteDepthArray,
                            depthFrame.Stride);

                        //// encode color stream
                        //System.IO.MemoryStream outStreamColor = new System.IO.MemoryStream();
                        //BitmapEncoder encoderColor = new BmpBitmapEncoder();
                        //encoderColor.Frames.Add(BitmapFrame.Create(bs_color));
                        //encoderColor.Save(outStreamColor);
                        //Bitmap bmColor = new System.Drawing.Bitmap(outStreamColor);
                        //Image<Bgra, byte> colorImage = new Image<Bgra, byte>(bmColor);

                        ////encode depth stream
                        //System.IO.MemoryStream outStreamDepth = new System.IO.MemoryStream();
                        //BitmapEncoder encoderDepth = new BmpBitmapEncoder();
                        //encoderDepth.Frames.Add(BitmapFrame.Create(bs_depth));
                        //encoderDepth.Save(outStreamDepth);
                        //Bitmap bmDepth = new System.Drawing.Bitmap(outStreamDepth);
                        //Image<Bgra, byte> depthImage = new Image<Bgra, byte>(bmDepth);
                        //Image<Gray, byte> depthImageTemp = depthImage.Convert<Gray, byte>();
                        //Image<Gray, int> depthImageTemp2 = depthImageTemp.Convert<Gray, int>();

                        //CameraManager.Instance.SetImages(colorImage, colorImage, depthImageTemp2, depthImageTemp2);
                    }
                }, token);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during RealSense D415 startup");
            }
        }

        public void StopRealSenseD415Capturing()
        {
            tokenSource.Cancel();
        }
    }
}
