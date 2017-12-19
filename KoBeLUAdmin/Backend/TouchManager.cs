using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using HciLab.Kinect;
using KoBeLUAdmin.ContentProviders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoBeLUAdmin.Backend
{
    public class TouchManager
    {

        private Image<Gray, byte> touch;
        private VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
        private VectorOfPointF touchPoints;
        Image<Gray, byte> touchRoi;
        Rectangle roi;


        public TouchManager()
        {
            //roi = new Rectangle(SettingsManager.Instance.Settings.SettingsTable.KinectDrawing.X, SettingsManager.Instance.Settings.SettingsTable.KinectDrawing.Y,
            //    SettingsManager.Instance.Settings.SettingsTable.KinectDrawing.X + SettingsManager.Instance.Settings.SettingsTable.KinectDrawing.Width,
            //    SettingsManager.Instance.Settings.SettingsTable.KinectDrawing.Y + SettingsManager.Instance.Settings.SettingsTable.KinectDrawing.Height);
            roi = new Rectangle(200, 100, 200, 200);
        }


        public void DetectTouch(double pTouchDepthMin, double pTouchDepthMax, double pTouchMinArea = 50, double pTouchMaxArea = 55)
        {
            Image<Gray, byte> image = KinectManager.Instance.KinectConnector.DepthImgByte;
            touch = image.Cmp(pTouchDepthMin, Emgu.CV.CvEnum.CmpType.GreaterThan) & image.Cmp(pTouchDepthMax, Emgu.CV.CvEnum.CmpType.LessThan);
            touchRoi = touch;
            touchRoi.ROI = roi;
            CvInvoke.FindContours(touchRoi, contours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            System.Drawing.PointF[] touchpoint_array = new System.Drawing.PointF[contours.Size];
            touchPoints = new VectorOfPointF();

            CvInvoke.Imshow("Test", touchRoi);

            for (int i = 0; i < contours.Size; i++)
            {
                if (CvInvoke.ContourArea(contours[i]) > pTouchMinArea)
                {
                    MCvScalar center = CvInvoke.Mean(contours[i]);
                    touchpoint_array[i] = new System.Drawing.PointF((float)center.V0, (float)center.V1);
                    touchPoints.Push(touchpoint_array);
                }
            }

            for (int i = 0; i < touchPoints.Size; i++)
            {
                Console.WriteLine("Touch detected at position: " + touchPoints[i].X + " " + touchPoints[i].Y);
            }
        }

    }
}
