using System.Collections.Generic;

namespace KoBeLU.Schema
{
    public interface IPolygonSceneItem : ISceneItem
    {
        IList<IList<float>> Points { get; set; }   
    }
}