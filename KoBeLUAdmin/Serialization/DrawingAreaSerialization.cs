using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoBeLUAdmin.Serialization
{

    class DrawingAreaSerialization
    {
        private string mCall;
        private double mDrawingAreaX;
        private double mDrawingAreaY;
        private double mDrawingAreaWidth;
        private double mDrawingAreaHeight;

        [JsonProperty(PropertyName = "call")]
        public string Call { get => mCall; set => mCall = value; }
        [JsonProperty(PropertyName = "drawingareax")]
        public double DrawingAreaX { get => mDrawingAreaX; set => mDrawingAreaX = value; }
        [JsonProperty(PropertyName = "drawingareay")]
        public double DrawingAreaY { get => mDrawingAreaY; set => mDrawingAreaY = value; }
        [JsonProperty(PropertyName = "drawingareawidth")]
        public double DrawingAreaWidth { get => mDrawingAreaWidth; set => mDrawingAreaWidth = value; }
        [JsonProperty(PropertyName = "drawingareaheight")]
        public double DrawingAreaHeight { get => mDrawingAreaHeight; set => mDrawingAreaHeight = value; }
    }
}
