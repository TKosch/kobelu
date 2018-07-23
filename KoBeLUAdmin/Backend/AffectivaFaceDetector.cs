﻿using Affdex;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoBeLUAdmin.Backend
{
    public class AffectivaFaceDetector : Affdex.ImageListener, Affdex.FaceListener
    {

        // constants
        private const int CAMID = 10;
        private const int CAMFPS = 60;

        private string mClassifierPath;
        private CameraDetector mCameraDetector = new CameraDetector();

        public AffectivaFaceDetector(string pClassifierPath)
        {
            this.mClassifierPath = pClassifierPath;

            mCameraDetector.setClassifierPath(pClassifierPath);

            mCameraDetector.setCameraId(CAMID);
            mCameraDetector.setCameraFPS(CAMFPS);

            mCameraDetector.setImageListener(this);
            mCameraDetector.setFaceListener(this);

        }

        public void onImageResults(Dictionary<int, Face> faces, Frame frame)
        {
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
