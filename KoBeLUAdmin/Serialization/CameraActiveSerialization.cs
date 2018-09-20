using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoBeLUAdmin.Serialization
{
    class CameraActiveSerialization
    {
        private string mCall;
        private bool mIsActive;

        public CameraActiveSerialization()
        {
        }

        [JsonProperty("call")]
        public string Call { get => mCall; set => mCall = value; }

        [JsonProperty("isactive")]
        public bool IsActive
        {
            get
            {
                return mIsActive;
            }

            set
            {
                mIsActive = value;
            }
        }
    }
}
