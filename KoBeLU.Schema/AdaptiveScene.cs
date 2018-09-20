using System.Collections.Generic;

namespace KoBeLU.Schema
{
    public class AdaptiveScene
    {
        public int ExpectedDuration { get; set; }

        public AdaptivityLevel Level { get; set; }

        public IList<ISceneItem> Items { get; set; }
    }


}
