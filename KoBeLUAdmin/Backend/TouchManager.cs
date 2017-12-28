using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using HciLab.Kinect;
using HciLab.Kinect.DepthSmoothing;
using KoBeLUAdmin.ContentProviders;
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

        public VectorOfPointF DetectTouch(Image<Gray, Int32> pImage, Image<Gray, Int32> pReferenceImage, double pTouchDepthMin, double pTouchDepthMax, double pTouchMinArea = 10, double pTouchMaxArea = 45)
        {

            mForeground = pReferenceImage - pImage;
            mTouch = mForeground.Cmp(pTouchDepthMin, Emgu.CV.CvEnum.CmpType.GreaterThan) & mForeground.Cmp(pTouchDepthMax, Emgu.CV.CvEnum.CmpType.LessThan);
            CvInvoke.FindContours(mTouch, mContours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            //CvInvoke.Imshow("test", mTouch);

            System.Drawing.PointF[] touchpoint_array = new System.Drawing.PointF[mContours.Size];
            mTouchPoints = new VectorOfPointF();

            for (int i = 0; i < mContours.Size; i++)
            {
                if (CvInvoke.ContourArea(mContours[i]) > pTouchMinArea)
                {
                    MCvScalar center = CvInvoke.Mean(mContours[i]);
                    touchpoint_array[i] = new System.Drawing.PointF((float)center.V0, (float)center.V1);
                }
            }

            mTouchPoints.Push(touchpoint_array);

            //for (int i = 0; i < mTouchPoints.Size; i++)
            //{
            //    if (mTouchPoints[i].X != 0 && mTouchPoints[i].Y != 0)
            //        Console.WriteLine("Touch detected at position: " + mTouchPoints[i].X + " " + mTouchPoints[i].Y);
            //}

            return mTouchPoints;
        }

    }
}
