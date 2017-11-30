using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoBeLUAdmin.Serialization
{
    class NextWorkingStepSerialization
    {

        private string mCall;
        private int mCurrentWorkingStep;

        public NextWorkingStepSerialization()
        { }

        [JsonProperty(PropertyName = "call")]
        public string Call { get => mCall; set => mCall = value; }
        [JsonProperty(PropertyName = "currentworkingstep")]
        public int CurrentWorkingStep { get => mCurrentWorkingStep; set => mCurrentWorkingStep = value; }
    }
}
