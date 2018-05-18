using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoBeLUAdmin.Serialization
{
    class WorkflowLoadedSerialization
    {

        private string mCall;
        private string mWorkflowId;

        public WorkflowLoadedSerialization()
        { }

        [JsonProperty(PropertyName = "call")]
        public string Call { get => mCall; set => mCall = value; }
        [JsonProperty(PropertyName = "workflowid")]
        public string WorkflowId { get => mWorkflowId; set => mWorkflowId = value; }
    }
}
