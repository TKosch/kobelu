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

        private static AffectivaFaceDetector mInstance;

        /// <summary>
        /// Singleton Constructor
        /// </summary>
        public static AffectivaFaceDetector Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new AffectivaFaceDetector();
                }
                return mInstance;
            }
        }

        // constants
        private const int CAMID = 0;
        private const int CAMFPS = 60;

        private CameraActiveSerialization mCameraActiveSerialization = new CameraActiveSerialization();
        private bool mCancelCamera = false;
        CameraDetector mCameraDetector;

        public AffectivaFaceDetector()
        {
        }

        public void StartAffectivaFaceDetector()
        {
            //if camera detector is running don't start a new one
            if (mCameraDetector != null)
                return;
            mCameraDetector = new CameraDetector();
            // path to data files
            string affdexDataPath = "C:\\Program Files\\Affectiva\\AffdexSDK\\data";
            mCameraActiveSerialization.Call = "set_camera_active";
            mCameraActiveSerialization.IsActive = true;

            mCameraDetector.setClassifierPath(affdexDataPath);
            mCameraDetector.setCameraId(CAMID);
            mCameraDetector.setCameraFPS(CAMFPS);
            mCameraDetector.setImageListener(this);
            mCameraDetector.setFaceListener(this);
            mCancelCamera = false;
            mCameraDetector.start();

            Thread cameraThread = new Thread(CameraChecker);
            cameraThread.Start();
        }


        public void StopAffectivaFaceDetector()
        {
            CancelCamera = true;
        }

        private void CameraChecker()
        {
            while (mCameraDetector.isRunning())
            {
                if (CancelCamera)
                {
                    mCameraDetector.stop();
                    mCameraDetector.Dispose();
                    mCameraDetector = null;
                    string message = JsonConvert.SerializeObject(mCameraActiveSerialization);
                    NetworkManager.Instance.SendDataOverUDP(SettingsManager.Instance.Settings.UDPIPTarget, 20000, message);
                }
                if (mCameraDetector == null)
                    break;
            }
        }

        public void onImageResults(Dictionary<int, Face> faces, Frame frame)
        {
            if (faces.Count > 0)
            {
                if (mCameraActiveSerialization.IsActive)
                {
                    mCameraActiveSerialization.IsActive = false;
                    StopAffectivaFaceDetector();
                }
            }
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
        public bool CancelCamera { get => mCancelCamera; set => mCancelCamera = value; }
    }
}
