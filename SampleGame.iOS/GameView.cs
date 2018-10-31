extern alias IOS;

using IOS::Foundation;
using IOS::OpenGLES;
using IOS::ObjCRuntime;
using IOS::CoreAnimation;
using OpenTK.Platform.iPhoneOS;
using SixLabors.Primitives;

namespace SampleGame.iOS
{
    [Register("GameView")]
    public partial class GameView : iPhoneOSGameView
    {
        [Export("layerClass")]
        static Class LayerClass()
        {
            return iPhoneOSGameView.GetLayerClass();
        }

        protected override void ConfigureLayer(CAEAGLLayer eaglLayer)
        {
            eaglLayer.Opaque = true;
        }

        [Export("initWithFrame:")]
        public GameView(RectangleF frame)
            : base(frame)
        {
            LayerRetainsBacking = false;
            LayerColorFormat = EAGLColorFormat.RGBA8;
            ContextRenderingApi = EAGLRenderingAPI.OpenGLES3;
        }
    }
}
