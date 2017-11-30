using KoBeLUAdmin.Scene;
using Newtonsoft.Json;
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
        private SceneItem sceneItemProperties;
        private string mWorkflowPath;

        public CurrentWorkingStepSerialization()
        {
        }

        [JsonProperty(PropertyName = "currentworkingstepnumber")]
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

        [JsonProperty(PropertyName = "sceneitemtype")]
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

        [JsonProperty(PropertyName = "sceneitemproperties")]
        public SceneItem SceneItemProperties
        {
            get
            {
                return sceneItemProperties;
            }

            set
            {
                sceneItemProperties = value;
            }
        }

        [JsonProperty(PropertyName = "workflowpath")]
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
