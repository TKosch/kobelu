using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using HciLab.Kinect;
using HciLab.Kinect.DepthSmoothing;
using HciLab.Utilities.Mathematics.Core;
using KoBeLUAdmin.ContentProviders;
using KoBeLUAdmin.Frontend;
using KoBeLUAdmin.GUI;
using KoBeLUAdmin.Model.Process;
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


        public void DetectTouch(Image<Gray, Int32> pImage, Image<Gray, Int32> pReferenceImage, double pTouchDepthMin, double pTouchDepthMax, double pTouchMinArea, double pTouchMaxArea)
        {

            if (pReferenceImage.ROI.Equals(pImage.ROI))
            {
                mForeground = pReferenceImage - pImage;
                mTouch = mForeground.Cmp(pTouchDepthMin, Emgu.CV.CvEnum.CmpType.GreaterThan) & mForeground.Cmp(pTouchDepthMax, Emgu.CV.CvEnum.CmpType.LessThan);
                CvInvoke.FindContours(mTouch, mContours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
                System.Drawing.PointF[] touchpoint_array = new System.Drawing.PointF[mContours.Size];
                TouchPoints = new VectorOfPointF();


                if (SettingsManager.Instance.Settings.SettingsTable.DisplayTouchVideoFeed)
                {
                    CvInvoke.Imshow("Touch Video Feed", mTouch);
                }

                for (int i = 0; i < mContours.Size; i++)
                {
                    if (CvInvoke.ContourArea(mContours[i]) > pTouchMinArea && CvInvoke.ContourArea(mContours[i]) < pTouchMaxArea)
                    {
                        MCvScalar center = CvInvoke.Mean(mContours[i]);
                        touchpoint_array[i] = new System.Drawing.PointF((float)center.V0, (float)center.V1);
                        double x = touchpoint_array[i].X / pImage.Width;
                        double y = 1 - (touchpoint_array[i].Y / pImage.Height);
                        touchpoint_array[i].X = (float)x;
                        touchpoint_array[i].Y = (float)y;
                        if (SettingsManager.Instance.Settings.SettingsTable.DisplayTouchDebugCoordinates)
                        {
                            Console.WriteLine("X: " + touchpoint_array[i].X + " Y: " + touchpoint_array[i].Y);
                        }
                    }
                }
                // push unprocessed "raw" touch points into the exposed array
                TouchPoints.Push(touchpoint_array);
                ProcessTUIO();
            }

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
                    TuioCursor cursor = TuioServer.getClosestTuioCursor(TouchPoints[i].X, TouchPoints[i].Y);
                    if (cursor == null || cursor.TuioTime == TuioTime.SessionTime)
                    {
                        TuioServer.addTuioCursor(TouchPoints[i].X, TouchPoints[i].Y);
                    }
                    else
                    {
                        TuioServer.updateTuioCursor(cursor, TouchPoints[i].X, TouchPoints[i].Y);
                    }
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
