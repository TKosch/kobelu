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

        public string Call { get => mCall; set => mCall = value; }
        public int CurrentWorkingStepNumber { get => mCurrentWorkingStepNumber; set => mCurrentWorkingStepNumber = value; }
    }
}
