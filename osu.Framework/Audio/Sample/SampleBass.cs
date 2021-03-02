// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Handles;

namespace osu.Framework.Audio.Sample
{
    internal sealed class SampleBass : Sample
    {
        public override bool IsLoaded => handle.IsLoaded;

        private readonly SafeBassSampleHandle handle;

        internal SampleBass(SafeBassSampleHandle handle)
        {
            this.handle = handle;
        }

        protected override SampleChannel CreateChannel() => new SampleChannelBass(new SafeBassSampleHandle(handle, false));

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            handle?.Dispose();

            base.Dispose(disposing);
        }
    }
}
