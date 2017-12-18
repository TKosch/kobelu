using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using HciLab.Kinect;
using System;
using System.Collections.Generic;
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

        public TouchManager()
        {
        }


        public void DetectTouch(double pTouchDepthMin, double pTouchDepthMax)
        {
            Image<Gray, byte> image = KinectManager.Instance.KinectConnector.DepthImgByte;
            touch = image.Cmp(pTouchDepthMin, Emgu.CV.CvEnum.CmpType.GreaterThan) & image.Cmp(pTouchDepthMax, Emgu.CV.CvEnum.CmpType.LessThan);
            CvInvoke.Imshow("Test", touch);
        }

    }
}
