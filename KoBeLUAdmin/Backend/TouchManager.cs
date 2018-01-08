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
using TUIO;

namespace KoBeLUAdmin.Backend
{
    public class TouchManager
    {

        private Image<Gray, byte> mTouch;
        private Image<Gray, Int32> mForeground;
        private VectorOfVectorOfPoint mContours = new VectorOfVectorOfPoint();
        private VectorOfPointF mTouchPoints;
        private static TuioServer mTuioServer = new TuioServer("127.0.0.1", 20002, 65536);
        private static TouchManager mInstance;

        public static TouchManager Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new TouchManager();
                }
                return mInstance;
            }
        }

        public TouchManager()
        {
        }

        public void DetectTouch(Image<Gray, Int32> pImage, Image<Gray, Int32> pReferenceImage, double pTouchDepthMin, double pTouchDepthMax, double pTouchMinArea = 10, double pTouchMaxArea = 45)
        {

            mForeground = pReferenceImage - pImage;
            mTouch = mForeground.Cmp(pTouchDepthMin, Emgu.CV.CvEnum.CmpType.GreaterThan) & mForeground.Cmp(pTouchDepthMax, Emgu.CV.CvEnum.CmpType.LessThan);
            CvInvoke.FindContours(mTouch, mContours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            System.Drawing.PointF[] touchpoint_array = new System.Drawing.PointF[mContours.Size];
            TouchPoints = new VectorOfPointF();
            for (int i = 0; i < mContours.Size; i++)
            {
                if (CvInvoke.ContourArea(mContours[i]) > pTouchMinArea)
                {
                    MCvScalar center = CvInvoke.Mean(mContours[i]);
                    touchpoint_array[i] = new System.Drawing.PointF((float)center.V0, (float)center.V1);

                    // adjust coordinates to the projector resolution
                    Rectangle projectorResolution = ScreenManager.getProjectorResolution();
                    // scale to resolution
                    Vector2 scaleFactor = new Vector2(projectorResolution.Width / pImage.Width, projectorResolution.Height / pImage.Height);

                    double x = (touchpoint_array[i].X) * scaleFactor.X;
                    double y = (touchpoint_array[i].Y) * scaleFactor.Y;
                    touchpoint_array[i].X = (float)x;
                    touchpoint_array[i].Y = (float)y;
                    //Console.WriteLine("X: " + touchpoint_array[i].X + " Y: " + touchpoint_array[i].Y);
                }
            }
            // push unprocessed "raw" touch points into the exposed array
            TouchPoints.Push(touchpoint_array);
            ProcessTUIO();
        }

        public void ProcessTUIO()
        {
            if (TuioServer == null)
                return;

            // init
            TuioServer.initFrame(TuioTime.SessionTime);
            for (int i = 0; i < TouchPoints.Size; i++)
            {
                if ((TouchPoints[i].X != 0) && (TouchPoints[i].Y != 0))
                {
                    TuioCursor tcur_new = TuioServer.addTuioCursor(TouchPoints[i].X, TouchPoints[i].Y);
                }
            }

            // commit TUIO cursors
            TuioServer.stopUntouchedMovingCursors();
            TuioServer.removeUntouchedStoppedCursors();
            TuioServer.commitFrame();
        }

        public void ClearTUIO()
        {
            TuioServer.Clear();
        }

        public List<TuioCursor> GetCursors()
        {
            return this.TuioServer.getTuioCursors();
        }

        public VectorOfPointF TouchPoints { get => mTouchPoints; set => mTouchPoints = value; }
        public TuioServer TuioServer { get => mTuioServer; set => mTuioServer = value; }
    }

}
