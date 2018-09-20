namespace KoBeLU.Schema
{
    public interface ITextSceneItem : ISceneItem
    {
        string Text { get; set; }
        string FontFamily { get; set; }
        string FontSize { get; set; }
    }
}