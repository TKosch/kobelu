namespace KoBeLU.Schema
{
    public interface IVideoSceneItem : ISceneItem
    {
        string SourceUrl { get; set; }
        bool AutoLoop { get; set; }
        bool AutoPlay { get; set; }
    }
}