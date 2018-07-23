using Affdex;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoBeLUAdmin.Backend
{
    public class AffectivaManager
    {

        private string mClassifierPath;
        CameraDetector mCameraDetector = new CameraDetector();

        public AffectivaManager(string pClassifierPath)
        {
            // check if directory exists
            if (!Directory.Exists(pClassifierPath))
            {
                Directory.CreateDirectory(pClassifierPath);
            }

            this.mClassifierPath = pClassifierPath;
            Console.WriteLine(pClassifierPath);
        }

    }
}
