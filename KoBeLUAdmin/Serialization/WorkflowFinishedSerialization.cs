using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoBeLUAdmin.Serialization
{
    class WorkflowFinishedSerialization
    {
        private string mCall;

        public WorkflowFinishedSerialization() { }

        [JsonProperty(PropertyName = "call")]
        public string Call { get => mCall; set => mCall = value; }
    }
}
