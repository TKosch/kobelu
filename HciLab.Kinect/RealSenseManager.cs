using Emgu.CV;
using Emgu.CV.Structure;
using HciLab.KoBeLU.InterfacesAndDataModel.Data;
using Intel.RealSense;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
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
        private const int MapDepthToByteImage = 8000 / 256;
        private byte[] mColorPixels;
        private byte[] mDepthPixels;
        private const int SCALE_FACTOR = Int16.MaxValue / 1500;
        private Image<Bgra, byte> mColorImage;
        private Image<Gray, int> mDepthImage;


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
                        using (var frames = pipeline.WaitForFrames())
                        {


                            VideoFrame depthFrame = frames.DepthFrame;
                            VideoFrame colorFrame = frames.ColorFrame;

                            mDepthPixels = new byte[depthFrame.Stride * depthFrame.Height * 2];
                            mColorPixels = new byte[colorFrame.Width * colorFrame.Height * 3];

                            depthFrame.CopyTo(mDepthPixels);
                            colorFrame.CopyTo(mColorPixels);

                            // color frame
                            Image<Rgb, byte> bufferedColorImage = new Image<Rgb, byte>(colorFrame.Width, colorFrame.Height);
                            bufferedColorImage.Bytes = mColorPixels;
                            mColorImage = bufferedColorImage.Convert<Bgra, byte>();

                            // depth frame
                            ushort maxDepth = ushort.MaxValue;
                            ProcessDepthFrameData(depthFrame.Data, (uint) (depthFrame.Width * depthFrame.Height), depthFrame.BitsPerPixel, 0, maxDepth);

                            int size = depthFrame.Width * depthFrame.Height * 2;
                            byte[] managedArray = new byte[size];
                            Marshal.Copy(depthFrame.Data, managedArray, 0, size);

                            Image<Gray, Int16> bufferedDepthImage = new Image<Gray, Int16>(depthFrame.Width, depthFrame.Height);
                            bufferedDepthImage.Bytes = managedArray;
                            bufferedDepthImage = bufferedDepthImage.ConvertScale<Int16>(SCALE_FACTOR, 0);

                            mDepthImage = bufferedDepthImage.Convert<Gray, int>();

                            // streamline frames into the cameramanager
                            CameraManager.Instance.SetImages(mColorImage, mColorImage, mDepthImage, mDepthImage);
                        }
                    }
                }, token);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during RealSense D415 startup");
            }
        }

        /// <summary>
        /// Directly accesses the underlying image buffer of the DepthFrame to 
        /// create a displayable bitmap.
        /// This function requires the /unsafe compiler option as we make use of direct
        /// access to the native memory pointed to by the depthFrameData pointer.
        /// </summary>
        /// <param name="depthFrameData">Pointer to the DepthFrame image data</param>
        /// <param name="depthFrameDataSize">Size of the DepthFrame image data</param>
        /// <param name="minDepth">The minimum reliable depth value for the frame</param>
        /// <param name="maxDepth">The maximum reliable depth value for the frame</param>
        private unsafe void ProcessDepthFrameData(IntPtr depthFrameData, uint depthFrameDataSize, int bitsPerPixel, ushort minDepth, ushort maxDepth)
        {
            // depth frame data is a 16 bit value
            ushort* frameData = (ushort*)depthFrameData;

            // convert depth to a visual representation
            for (int i = 0; i < (int)(depthFrameDataSize / bitsPerPixel); ++i)
            {
                // Get the depth for this pixel
                ushort depth = frameData[i];

                // To convert to a byte, we're mapping the depth value to the byte range.
                // Values outside the reliable depth range are mapped to 0 (black).
                this.mDepthPixels[i] = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByteImage) : 0);
            }
        }

        public void StopRealSenseD415Capturing()
        {
            tokenSource.Cancel();
        }
    }
}
