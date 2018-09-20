namespace KoBeLU.Schema
{
    public interface ICircleSceneItem : ISceneItem
    {
        float Radius { get; set; }
        float StartAngle { get; set; }
        float EndAngle { get; set; }
    }
}