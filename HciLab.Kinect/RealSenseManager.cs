using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HciLab.Kinect
{
    public class RealSenseManager
    {

        private static RealSenseManager mInstance;

        public static RealSenseManager Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new RealSenseManager();
                }
                return mInstance;
            }
        }

    }
}
