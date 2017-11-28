using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoBeLUAdmin.Serialization
{
    class LoadWorkflowSerialization
    {
        private string mCall;
        private string mWorkflowpath;

        public LoadWorkflowSerialization()
        {
        }

        public string Call { get => mCall; set => mCall = value; }
        public string Workflowpath { get => mWorkflowpath; set => mWorkflowpath = value; }
    }
}
