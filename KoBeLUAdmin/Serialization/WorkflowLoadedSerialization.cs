using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoBeLUAdmin.Serialization
{
    public class WorkflowLoadedSerialization
    {
        private string mCall;

        public WorkflowLoadedSerialization()
        {

        }

        [JsonProperty(PropertyName = "call")]
        public string Call { get => mCall; set => mCall = value; }

    }
}
