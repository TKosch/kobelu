using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using HciLab.Kinect;
using HciLab.Kinect.DepthSmoothing;
using HciLab.Utilities.Mathematics.Core;
using KoBeLUAdmin.ContentProviders;
using KoBeLUAdmin.Frontend;
using KoBeLUAdmin.GUI;
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

        private Image<Gray, byte> mTouch;
        private Image<Gray, Int32> mForeground;
        private VectorOfVectorOfPoint mContours = new VectorOfVectorOfPoint();
        private VectorOfPointF mTouchPoints;

        public TouchManager()
        {
        }

        public void DetectTouch(Image<Gray, Int32> pImage, Image<Gray, Int32> pReferenceImage, double pTouchDepthMin, double pTouchDepthMax, double pTouchMinArea = 10, double pTouchMaxArea = 45)
        {

            mForeground = pReferenceImage - pImage;
            mTouch = mForeground.Cmp(pTouchDepthMin, Emgu.CV.CvEnum.CmpType.GreaterThan) & mForeground.Cmp(pTouchDepthMax, Emgu.CV.CvEnum.CmpType.LessThan);
            CvInvoke.FindContours(mTouch, mContours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            System.Drawing.PointF[] touchpoint_array = new System.Drawing.PointF[mContours.Size];

            for (int i = 0; i < mContours.Size; i++)
            {
                if (CvInvoke.ContourArea(mContours[i]) > pTouchMinArea)
                {
                    MCvScalar center = CvInvoke.Mean(mContours[i]);
                    touchpoint_array[i] = new System.Drawing.PointF((float)center.V0, (float)center.V1);

                    // adjust coordinates to the projector resolution
                    Rectangle projectorResolution = ScreenManager.getProjectorResolution();
                    // TODO: replace hard coded values with calibration resolution
                    Vector2 scaleFactor = new Vector2(projectorResolution.Width / 512, projectorResolution.Height / 361);
                    
                    double x = (SettingsManager.Instance.Settings.SettingsTable.KinectDrawing_AssemblyArea.X + touchpoint_array[i].X) * scaleFactor.X;
                    double y = (SettingsManager.Instance.Settings.SettingsTable.KinectDrawing_AssemblyArea.Y + touchpoint_array[i].Y) * scaleFactor.Y;
                }
            }

            // push unprocessed "raw" touch points into the exposed array
            TouchPoints = new VectorOfPointF();
            TouchPoints.Push(touchpoint_array);

        }

        public VectorOfPointF TouchPoints { get => mTouchPoints; set => mTouchPoints = value; }
    }

}
