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
            roi = new Rectangle(SettingsManager.Instance.Settings.SettingsTable.KinectDrawing.X, SettingsManager.Instance.Settings.SettingsTable.KinectDrawing.Y,
                SettingsManager.Instance.Settings.SettingsTable.KinectDrawing.X + SettingsManager.Instance.Settings.SettingsTable.KinectDrawing.Width,
                SettingsManager.Instance.Settings.SettingsTable.KinectDrawing.Y + SettingsManager.Instance.Settings.SettingsTable.KinectDrawing.Height);
        }


        public void DetectTouch(double pTouchDepthMin, double pTouchDepthMax)
        {
            Image<Gray, byte> image = KinectManager.Instance.KinectConnector.DepthImgByte;
            touch = image.Cmp(pTouchDepthMin, Emgu.CV.CvEnum.CmpType.GreaterThan) & image.Cmp(pTouchDepthMax, Emgu.CV.CvEnum.CmpType.LessThan);
            touchRoi = touch;
            touchRoi.ROI = roi;

            CvInvoke.Imshow("Test", touchRoi);
        }

    }
}
