using System.Collections.Generic;

namespace KoBeLU.Schema
{
    public class WorkingStep
    {
        public int StepNumber { get; set; }
        public IList<AdaptiveScene> Scenes { get; set; }
    }


}
