namespace KoBeLU.Schema
{
    public class AssemblyZone
    {
        public int Area { get; set; }

        public bool UseDepthMask { get; set; }

        public int[][] Depth { get; set; }

        public bool[][] DepthMask { get; set; }

        public float MatchOffset { get; set; }
    }
}
