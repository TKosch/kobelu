using System.Collections.Generic;

namespace KoBeLU.Schema
{
    public partial class SceneItem
    {
        public int Id { get; set; }
        public string Type { get; set; }

        public float PositionX { get; set; }

        public float PositionY { get; set; }

        public float PositionZ { get; set; }

        public float ScaleX { get; set; }
        public float ScaleY { get; set; }
        public float ScaleZ { get; set; }

        public float Rotation { get; set; }

        public float CenterX { get; set; }
        public float CenterY { get; set; }

        public Color Color { get; set; }
        public Flash Flash { get; set; }

        public string Text { get; set; }
        public string FontFamily { get; set; }
        public string FontSize { get; set; }

        public string SourceUrl { get; set; }
        public bool AutoLoop { get; set; }
        public bool AutoPlay { get; set; }

        public float Radius { get; set; }
        public float StartAngle { get; set; }
        public float EndAngle { get; set; }

        public IList<IList<float>> Points { get; set; }


    }
}
