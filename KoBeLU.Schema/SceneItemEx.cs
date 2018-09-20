using System;

namespace KoBeLU.Schema
{
    public partial class SceneItem :
        ISceneItem,
        IAudioSceneItem,
        ICircleSceneItem,
        IImageSceneItem,
        IPolygonSceneItem,
        IRectSceneItem,
        ITextSceneItem,
        IVideoSceneItem

    {
        public IAudioSceneItem AsAudioSceneItem()
            => IsSceneItem(SceneItemTypes.Audio) ? this : null;

        public ICircleSceneItem AsCircleSceneItem()
            => IsSceneItem(SceneItemTypes.Circle) ? this : null;

        public IImageSceneItem AsImageSceneItem()
            => IsSceneItem(SceneItemTypes.Image) ? this : null;

        public IPolygonSceneItem AsIPolygonSceneItem()
            => IsSceneItem(SceneItemTypes.Polygon) ? this : null;

        public IRectSceneItem AsIRectSceneItem()
            => IsSceneItem(SceneItemTypes.Rectangle) ? this : null;

        public ITextSceneItem AsITextSceneItem()
            => IsSceneItem(SceneItemTypes.Image) ? this : null;

        public IVideoSceneItem AsVideoSceneItem()
            => IsSceneItem(SceneItemTypes.Video) ? this : null;


        protected bool IsSceneItem(string sceneItemType)
        {
            var type = Type;
            if (type == null)
                return false;

            var result = type.StartsWith(sceneItemType, StringComparison.OrdinalIgnoreCase);
            if (result)
                return type.Length == sceneItemType.Length;

            return result;
        }
    }
}
