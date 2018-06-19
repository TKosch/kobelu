namespace KoBeLU.Schema
{
    public interface IAudioSceneItem : ISceneItem
    {
        string SourceUrl { get; set; }
        bool AutoLoop { get; set; }
        bool AutoPlay { get; set; }
    }
}