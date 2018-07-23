using Affdex;
using KoBeLUAdmin.ContentProviders;
using KoBeLUAdmin.Network;
using KoBeLUAdmin.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace KoBeLUAdmin.Backend
{
    public class AffectivaFaceDetector : Affdex.ImageListener, Affdex.FaceListener
    {

        // constants
        private const int CAMID = 0;
        private const int CAMFPS = 60;

        private string mClassifierPath;
        private CameraDetector mCameraDetector = new CameraDetector();
        private CameraActiveSerialization mCameraActiveSerialization = new CameraActiveSerialization();
        private static int usingResource = 0;

        public AffectivaFaceDetector(string pClassifierPath)
        {
            this.mClassifierPath = pClassifierPath;

            mCameraActiveSerialization.Call = "camera_active";
            mCameraActiveSerialization.IsActive = true;

            mCameraDetector.setClassifierPath(pClassifierPath);
            mCameraDetector.setCameraId(CAMID);
            mCameraDetector.setCameraFPS(CAMFPS);
            mCameraDetector.setImageListener(this);
            mCameraDetector.setFaceListener(this);
        }


        public void onImageResults(Dictionary<int, Face> faces, Frame frame)
        {
            if (faces.Count > 0)
            {
                if (mCameraActiveSerialization.IsActive)
                {
                    
                        mCameraActiveSerialization.IsActive = false;
                        string message = JsonConvert.SerializeObject(mCameraActiveSerialization);
                        NetworkManager.Instance.SendDataOverUDP(SettingsManager.Instance.Settings.UDPIPTarget, 20000, message);
                        ReleaseCameraResources();
                }
            }
        }


        private bool ReleaseCameraResources()
        {
            if (0 == Interlocked.Exchange(ref usingResource, 1))
            {
                mCameraDetector.reset();
                Interlocked.Exchange(ref usingResource, 0);
                return true;
            }
            return false;
        }


        public void onImageCapture(Frame frame)
        {
        }

        public void onFaceFound(float timestamp, int faceId)
        {
        }

        public void onFaceLost(float timestamp, int faceId)
        {
        }

        public CameraDetector CameraDetector { get => mCameraDetector; set => mCameraDetector = value; }
    }
}
