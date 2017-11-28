using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoBeLUAdmin.Serialization
{
    class StartWorkflowSerialization
    {

        private string mCall;
        private string mWorkflowId;

        public StartWorkflowSerialization()
        { }

        public string Call { get => mCall; set => mCall = value; }
        public string WorkflowId { get => mWorkflowId; set => mWorkflowId = value; }
    }
}
