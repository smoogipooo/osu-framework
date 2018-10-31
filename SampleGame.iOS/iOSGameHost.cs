using System.Collections.Generic;
using OpenTK.Platform.iPhoneOS;
using osu.Framework.Input;
using osu.Framework.Input.Handlers;
using osu.Framework.Platform;
using osu.Framework.Platform.Windows;

namespace SampleGame.iOS
{
    public class iOSGameHost : GameHost
    {
        public iOSGameHost(iPhoneOSGameView gameView)
        {
            Window = new iOSGameWindow(gameView);
        }

        public override ITextInputSource GetTextInput() => null;

        protected override IEnumerable<InputHandler> CreateAvailableInputHandlers()
        {
            yield break;
        }

        public override void OpenFileExternally(string filename)
        {
            throw new System.NotImplementedException();
        }

        public override void OpenUrlExternally(string url)
        {
            throw new System.NotImplementedException();
        }

        protected override Storage GetStorage(string baseName) => new WindowsStorage(baseName, this);
    }
}
