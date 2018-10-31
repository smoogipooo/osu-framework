using OpenTK.Platform.iPhoneOS;
using osu.Framework.Configuration;
using osu.Framework.Platform;
using OpenTK.Graphics;

namespace SampleGame.iOS
{
    public class iOSGameWindow : GameWindow
    {
        private readonly iPhoneOSGameView gameView;

        public iOSGameWindow(iPhoneOSGameView gameView)
            : base(new iOSPlatformGameWindow(gameView))
        {
            this.gameView = gameView;
        }

        public override IGraphicsContext Context => gameView.GraphicsContext;

        public override void SetupWindow(FrameworkConfigManager config)
        {
            //throw new NotImplementedException();
        }
    }
}
