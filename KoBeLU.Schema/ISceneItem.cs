namespace KoBeLU.Schema
{
    public interface ISceneItem
    {
        /// <summary>
        /// ID for the scene item
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// Scene item type
        /// </summary>
        string Type { get; set; }

        /// <summary>
        /// X coordinate of the center
        /// </summary>
        float CenterX { get; set; }

        /// <summary>
        /// Y coordinate of the center
        /// </summary>
        float CenterY { get; set; }

        /// <summary>
        /// X coordinate of the position
        /// </summary>
        float PositionX { get; set; }

        /// <summary>
        /// Y coordinate of the position
        /// </summary>
        float PositionY { get; set; }

        /// <summary>
        /// Z coordinate of the position
        /// </summary>
        float PositionZ { get; set; }

        /// <summary>
        /// Rotation in degrees
        /// </summary>
        float Rotation { get; set; }

        /// <summary>
        /// Scale in X direction
        /// </summary>
        float ScaleX { get; set; }

        /// <summary>
        /// Scale in Y direction
        /// </summary>
        float ScaleY { get; set; }

        /// <summary>
        /// Scale in Z direction
        /// </summary>
        float ScaleZ { get; set; }

        /// <summary>
        /// Color information
        /// </summary>
        Color Color { get; set; }

        /// <summary>
        /// Flash information
        /// </summary>
        Flash Flash { get; set; }

        /// <summary>
        /// Return IAudioSceneItem if this is a scene item with audio, null otherwise
        /// </summary>
        IAudioSceneItem AsAudioSceneItem();

        /// <summary>
        /// Return ICircleSceneItem if this is a scene item with circle, null otherwise
        /// </summary>
        ICircleSceneItem AsCircleSceneItem();

        /// <summary>
        /// Return IImageSceneItem if this is a scene item with image, null otherwise
        /// </summary>
        IImageSceneItem AsImageSceneItem();

        /// <summary>
        /// Return IPolygonSceneItem if this is a scene item with polygon, null otherwise
        /// </summary>
        IPolygonSceneItem AsIPolygonSceneItem();

        /// <summary>
        /// Return IRectSceneItem if this is a scene item with rectangle, null otherwise
        /// </summary>
        IRectSceneItem AsIRectSceneItem();

        /// <summary>
        /// Return ITextSceneItem if this is a scene item with text, null otherwise
        /// </summary>
        ITextSceneItem AsITextSceneItem();

        /// <summary>
        /// Return IVideoSceneItem if this is a scene item with video, null otherwise
        /// </summary>
        IVideoSceneItem AsVideoSceneItem();
    }

    public class Color
    {
        public bool IsAlternating { get; set; }
        public float AlternatingDuration { get; set; }
        public string DefaultValue { get; set; }
        public string AlternateValue { get; set; }
    }

    public class Flash
    {
        //TODO: merge with color

        public int FlashTimeOff { get; set; }
        public int FlashTimeOn { get; set; }
        public bool IsFlashing { get; set; }
    }
}