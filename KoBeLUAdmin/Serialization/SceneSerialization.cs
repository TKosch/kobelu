using HciLab.Utilities;
using KoBeLUAdmin.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoBeLUAdmin.Serialization
{
    public class SceneSerialization
    {

        private int mCurrentWorkingStepNumber;
        private string mWorkflowPath;
        private CollectionWithItemNotify<SceneItem> mCurrentScene;

        public SceneSerialization()
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

        public CollectionWithItemNotify<SceneItem> CurrentSceneItems
        {
            get
            {
                return mCurrentScene;
            }

            set
            {
                mCurrentScene = value;
            }
        }
    }
}
