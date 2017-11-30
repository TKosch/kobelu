using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoBeLUAdmin.Serialization
{
    class PreviousWorkingStepSerialization
    {

        private string mCall;
        private int mCurrentWorkingStepNumber;

        public PreviousWorkingStepSerialization()
        {
        }

        [JsonProperty(PropertyName = "call")]
        public string Call { get => mCall; set => mCall = value; }
        [JsonProperty(PropertyName = "currentworkingstepnumber")]
        public int CurrentWorkingStepNumber { get => mCurrentWorkingStepNumber; set => mCurrentWorkingStepNumber = value; }
    }
}
