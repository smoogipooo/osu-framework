// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using SDL2;

namespace osu.Framework.Platform.SDL2
{
    /// <summary>
    /// Implementation of <see cref="PassthroughGraphicsBackend"/> that uses SDL's OpenGL bindings.
    /// </summary>
    public class SDL2GraphicsBackend : PassthroughGraphicsBackend
    {
        private IntPtr sdlWindowHandle;

        public override bool VerticalSync
        {
            get => SDL.SDL_GL_GetSwapInterval() != 0;
            set => SDL.SDL_GL_SetSwapInterval(value ? 1 : 0);
        }

        protected override IntPtr CreateContext()
        {
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, 2);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_ES);

            return SDL.SDL_GL_CreateContext(sdlWindowHandle);
        }

        protected override void MakeCurrent(IntPtr context) => SDL.SDL_GL_MakeCurrent(sdlWindowHandle, context);

        public override void SwapBuffers() => SDL.SDL_GL_SwapWindow(sdlWindowHandle);

        protected override IntPtr GetProcAddress(string symbol) => SDL.SDL_GL_GetProcAddress(symbol);

        public override void Initialise(IWindow window)
        {
            if (!(window is SDL2DesktopWindow sdlWindow))
                throw new ArgumentException("Unsupported window backend.", nameof(window));

            sdlWindowHandle = sdlWindow.SDLWindowHandle;
            base.Initialise(window);
        }
    }
}
