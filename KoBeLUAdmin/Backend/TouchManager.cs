using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using HciLab.Kinect;
using HciLab.Kinect.DepthSmoothing;
using KoBeLUAdmin.ContentProviders;
using KoBeLUAdmin.Frontend;
using KoBeLUAdmin.GUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCD.System.TUIO;

namespace KoBeLUAdmin.Backend
{
    public class TouchManager : TuioListener
    {

        private const long SESSIONID = 1;
        private const int CURSORID = 1;

        private Image<Gray, byte> mTouch;
        private Image<Gray, Int32> mForeground;
        private VectorOfVectorOfPoint mContours = new VectorOfVectorOfPoint();
        private VectorOfPointF mTouchPoints;
        private TuioClient mClient;
        private Dictionary<long, TuioTouchObject> mObjectList;
        private Dictionary<long, TuioCursor> mCursorList;
        private object cursorSync = new object();
        private object objectSync = new object();
        private bool verbose;

        public TouchManager(int pTUIOPort)
        {
            verbose = true;
            mObjectList = new Dictionary<long, TuioTouchObject>();
            mCursorList = new Dictionary<long, TuioCursor>();

            Client = new TuioClient(pTUIOPort);
            Client.addTuioListener(this);
            Client.connect();
        }

        public void addTuioCursor(TuioCursor tcur)
        {
            lock (cursorSync)
            {
                mCursorList.Add(tcur.getSessionID(), tcur);
            }
            if (verbose) Console.WriteLine("add cur " + tcur.getCursorID() + " (" + tcur.getSessionID() + ") " + tcur.getX() + " " + tcur.getY());
        }

        public void addTuioObject(TuioObject tobj)
        {
            lock (objectSync)
            {
                mObjectList.Add(tobj.getSessionID(), new TuioTouchObject(tobj));
            }
            if (verbose) Console.WriteLine("add obj " + tobj.getSymbolID() + " (" + tobj.getSessionID() + ") " + tobj.getX() + " " + tobj.getX() + " " + tobj.getAngle());
        }

        public void refresh(TuioTime ftime)
        {
            TableWindow3D.Instance.InvalidateVisual();
        }

        public void removeTuioCursor(TuioCursor tcur)
        {
            lock (cursorSync)
            {
                mCursorList.Remove(tcur.getSessionID());
            }
            if (verbose) Console.WriteLine("del cur " + tcur.getCursorID() + " (" + tcur.getSessionID() + ")");
        }

        public void removeTuioObject(TuioObject tobj)
        {
            lock (objectSync)
            {
                mObjectList.Remove(tobj.getSessionID());
            }
            if (verbose) Console.WriteLine("del obj " + tobj.getSymbolID() + " (" + tobj.getSessionID()+ ")");
        }

        public void updateTuioCursor(TuioCursor tcur)
        {
            if (verbose) Console.WriteLine("set cur " + tcur.getCursorID() + " (" + tcur.getSessionID() + ") " + tcur.getX() + " " + tcur.getY() + " " + tcur.getMotionSpeed() + " " + tcur.getMotionAccel());
        }

        public void updateTuioObject(TuioObject tobj)
        {
            lock (objectSync)
            {
                mObjectList[tobj.getSessionID()].update(tobj);
            }
            if (verbose) Console.WriteLine("set obj " + tobj.getSessionID() + " " + tobj.getSessionID() + " " + tobj.getX() + " " + tobj.getY() + tobj.getMotionSpeed() + " " + tobj.getMotionAccel());
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

                    // update TUIO cursor
                    this.updateTuioCursor(new TuioCursor(SESSIONID, CURSORID, touchpoint_array[i].X, touchpoint_array[i].Y));
                }
            }
            TouchPoints = new VectorOfPointF();
            TouchPoints.Push(touchpoint_array);

        }

        public TuioClient Client { get => mClient; set => mClient = value; }
        public VectorOfPointF TouchPoints { get => mTouchPoints; set => mTouchPoints = value; }
    }


    public class TuioTouchObject : TuioObject
    {

        public TuioTouchObject(long s_id, int f_id, float xpos, float ypos, float angle) : base(s_id, f_id, xpos, ypos, angle)
        {
        }

        public TuioTouchObject(TuioObject o) : base(o)
        {
        }

    }

}
