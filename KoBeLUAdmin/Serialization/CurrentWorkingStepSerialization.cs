using KoBeLUAdmin.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoBeLUAdmin.Serialization
{
    public class CurrentWorkingStepSerialization
    {

        private int mCurrentWorkingStepNumber;
        private string mSceneItemType;
        private string mWorkflowPath;

        public CurrentWorkingStepSerialization()
        {
        }


        public int CurrentWorkingStepNumber
        {
            get
            {
                return mCurrentWorkingStepNumber;
            }

            set
            {
                mCurrentWorkingStepNumber = value;
            }
        }

        public string SceneItemType
        {
            get
            {
                return mSceneItemType;
            }

            set
            {
                mSceneItemType = value;
            }
        }

        public string WorkflowPath
        {
            get
            {
                return mWorkflowPath;
            }

            set
            {
                mWorkflowPath = value;
            }
        }
    }
}
