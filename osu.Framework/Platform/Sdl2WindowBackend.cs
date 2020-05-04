// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using osu.Framework.Caching;
using osu.Framework.Extensions;
using osu.Framework.Input.StateChanges;
using osu.Framework.Threading;
using Veldrid;
using Veldrid.Sdl2;
using Key = osuTK.Input.Key;
using MouseButton = osuTK.Input.MouseButton;
using MouseEvent = Veldrid.MouseEvent;
using Point = System.Drawing.Point;
using TKVector2 = osuTK.Vector2;

namespace osu.Framework.Platform
{
    /// <summary>
    /// Implementation of <see cref="IWindowBackend"/> that uses an SDL2 window.
    /// </summary>
    public class Sdl2WindowBackend : IWindowBackend
    {
        private const int default_width = 1366;
        private const int default_height = 768;

        private Sdl2Window implementation;
        private InputSnapshot inputSnapshot;
        private readonly Scheduler scheduler = new Scheduler();

        #region Internal Properties

        internal IntPtr SdlWindowHandle => implementation?.SdlWindowHandle ?? IntPtr.Zero;

        #endregion

        #region IWindowBackend.Properties

        private string title = "";

        public string Title
        {
            get => title;
            set
            {
                title = value;

                scheduler.Add(() => implementation.Title = $"{value} (SDL)");
            }
        }

        private bool visible;

        public bool Visible
        {
            get => implementation?.Visible ?? visible;
            set
            {
                visible = value;

                scheduler.Add(() => implementation.Visible = value);
            }
        }

        public bool Exists => implementation?.Exists ?? false;

        private Point position = Point.Empty;

        public Point Position
        {
            get => implementation == null ? position : new Point(implementation.X, implementation.Y);
            set
            {
                position = value;

                scheduler.Add(() =>
                {
                    implementation.X = value.X;
                    implementation.Y = value.Y;
                });
            }
        }

        private Size size = new Size(default_width, default_height);

        public Size Size
        {
            get => implementation == null ? size : new Size(implementation.Width, implementation.Height);
            set
            {
                size = value;

                scheduler.Add(() =>
                {
                    implementation.Width = value.Width;
                    implementation.Height = value.Height;
                });
            }
        }

        private readonly Cached<float> scale = new Cached<float>();

        public float Scale
        {
            get
            {
                if (scale.IsValid)
                    return scale.Value;

                float realWidth = implementation.Width;
                float scaledWidth = Sdl2Functions.SDL_GL_GetDrawableSize(SdlWindowHandle).X;
                scale.Value = scaledWidth / realWidth;
                return scale.Value;
            }
        }

        private bool cursorVisible = true;

        public bool CursorVisible
        {
            get => implementation?.CursorVisible ?? cursorVisible;
            set
            {
                cursorVisible = value;

                scheduler.Add(() => implementation.CursorVisible = value);
            }
        }

        public bool CursorConfined { get; set; }

        private WindowState windowState;

        public WindowState WindowState
        {
            get => implementation?.WindowState.ToFramework() ?? windowState;
            set
            {
                windowState = value;

                scheduler.Add(() => implementation.WindowState = value.ToVeldrid());
            }
        }

        public IEnumerable<Display> Displays => Enumerable.Range(0, Sdl2Functions.SDL_GetNumVideoDisplays()).Select(displayFromSDL);

        public Display PrimaryDisplay => Displays.First();

        private int previousDisplayIndex = -1;

        public Display CurrentDisplay
        {
            get => Displays.ElementAtOrDefault(currentDisplayIndex);
            set
            {
                if (value.Index == currentDisplayIndex)
                    return;

                scheduler.Add(() =>
                {
                    int x = value.Bounds.Left + value.Bounds.Width / 2 - implementation.Width / 2;
                    int y = value.Bounds.Top + value.Bounds.Height / 2 - implementation.Height / 2;
                    Sdl2Native.SDL_SetWindowPosition(SdlWindowHandle, x, y);
                });
            }
        }

        public DisplayMode CurrentDisplayMode => displayModeFromSDL(Sdl2Functions.SDL_GetCurrentDisplayMode(currentDisplayIndex));

        private static Display displayFromSDL(int displayIndex)
        {
            var displayModes = Enumerable.Range(0, Sdl2Functions.SDL_GetNumDisplayModes(displayIndex))
                                         .Select(modeIndex => displayModeFromSDL(Sdl2Functions.SDL_GetDisplayMode(displayIndex, modeIndex)))
                                         .ToArray();

            return new Display(displayIndex, Sdl2Functions.SDL_GetDisplayName(displayIndex), Sdl2Functions.SDL_GetDisplayBounds(displayIndex), displayModes);
        }

        private static DisplayMode displayModeFromSDL(SDL_DisplayMode mode)
        {
            Sdl2Functions.SDL_PixelFormatEnumToMasks(mode.Format, out var bpp, out _, out _, out _, out _);
            return new DisplayMode(Sdl2Functions.SDL_GetPixelFormatName(mode.Format), new Size(mode.Width, mode.Height), bpp, mode.RefreshRate);
        }

        private int currentDisplayIndex => Sdl2Functions.SDL_GetWindowDisplayIndex(SdlWindowHandle);

        #endregion

        #region IWindowBackend.Events

        public event Action Update;
        public event Action Resized;
        public event Action WindowStateChanged;
        public event Func<bool> CloseRequested;
        public event Action Closed;
        public event Action FocusLost;
        public event Action FocusGained;
        public event Action Shown;
        public event Action Hidden;
        public event Action MouseEntered;
        public event Action MouseLeft;
        public event Action<Point> Moved;
        public event Action<MouseScrollRelativeInput> MouseWheel;
        public event Action<MousePositionAbsoluteInput> MouseMove;
        public event Action<MouseButtonInput> MouseDown;
        public event Action<MouseButtonInput> MouseUp;
        public event Action<KeyboardKeyInput> KeyDown;
        public event Action<KeyboardKeyInput> KeyUp;
        public event Action<char> KeyTyped;
        public event Action<string> DragDrop;
        public event Action<Display> DisplayChanged;

        #endregion

        #region Event Invocation

        protected virtual void OnUpdate() => Update?.Invoke();
        protected virtual void OnResized() => Resized?.Invoke();
        protected virtual void OnWindowStateChanged() => WindowStateChanged?.Invoke();
        protected virtual bool OnCloseRequested() => CloseRequested?.Invoke() ?? false;
        protected virtual void OnClosed() => Closed?.Invoke();
        protected virtual void OnFocusLost() => FocusLost?.Invoke();
        protected virtual void OnFocusGained() => FocusGained?.Invoke();
        protected virtual void OnShown() => Shown?.Invoke();
        protected virtual void OnHidden() => Hidden?.Invoke();
        protected virtual void OnMouseEntered() => MouseEntered?.Invoke();
        protected virtual void OnMouseLeft() => MouseLeft?.Invoke();
        protected virtual void OnMoved(Point point) => Moved?.Invoke(point);
        protected virtual void OnMouseWheel(MouseScrollRelativeInput evt) => MouseWheel?.Invoke(evt);
        protected virtual void OnMouseMove(MousePositionAbsoluteInput args) => MouseMove?.Invoke(args);
        protected virtual void OnMouseDown(MouseButtonInput evt) => MouseDown?.Invoke(evt);
        protected virtual void OnMouseUp(MouseButtonInput evt) => MouseUp?.Invoke(evt);
        protected virtual void OnKeyDown(KeyboardKeyInput evt) => KeyDown?.Invoke(evt);
        protected virtual void OnKeyUp(KeyboardKeyInput evt) => KeyUp?.Invoke(evt);
        protected virtual void OnKeyTyped(char c) => KeyTyped?.Invoke(c);
        protected virtual void OnDragDrop(string file) => DragDrop?.Invoke(file);
        protected virtual void OnDisplayChanged(Display display) => DisplayChanged?.Invoke(display);

        #endregion

        #region IWindowBackend.Methods

        public void Create()
        {
            SDL_WindowFlags flags = SDL_WindowFlags.OpenGL |
                                    SDL_WindowFlags.Resizable |
                                    SDL_WindowFlags.AllowHighDpi |
                                    getWindowFlags(WindowState);

            implementation = new Sdl2Window(Title, position.X, position.Y, size.Width, size.Height, flags, false);

            // force a refresh of the size and position now that we can calculate the scale
            scale.Invalidate();
            Size = size;
            Position = position;

            implementation.MouseDown += implementation_OnMouseDown;
            implementation.MouseUp += implementation_OnMouseUp;
            implementation.MouseMove += implementation_OnMouseMove;
            implementation.MouseWheel += implementation_OnMouseWheel;
            implementation.KeyDown += implementation_OnKeyDown;
            implementation.KeyUp += implementation_OnKeyUp;
            implementation.FocusGained += OnFocusGained;
            implementation.FocusLost += OnFocusLost;
            implementation.Resized += implementation_Resized;
            implementation.Moved += implementation_OnMoved;
            implementation.MouseEntered += OnMouseEntered;
            implementation.MouseLeft += OnMouseLeft;
            implementation.Hidden += OnHidden;
            implementation.Shown += OnShown;
            implementation.Closed += OnClosed;
            implementation.DragDrop += implementation_DragDrop;
        }

        public void Run()
        {
            while (implementation.Exists)
            {
                inputSnapshot = implementation.PumpEvents();

                foreach (var c in inputSnapshot.KeyCharPresses)
                    OnKeyTyped(c);

                OnUpdate();

                scheduler.Update();
            }
        }

        public void Close()
        {
            // TODO: Fix OnCloseRequested
            // The Sdl2Window implementation does not currently have a way of aborting a manual close request.
            // The best we can do for now is abort any programmatic close requests if required.
            if (!OnCloseRequested())
                scheduler.Add(implementation.Close);
        }

        #endregion

        /// <summary>
        /// Don't propagate keys that osu!framework should not attempt to handle.
        /// osuTK does not report capslock key up/down events, but SDL2 does.
        /// This can cause issues where the capslock down is detected but not up.
        /// </summary>
        /// <param name="key">The key to validate.</param>
        private bool isKeyValid(Veldrid.Key key) => key != Veldrid.Key.Unknown && key != Veldrid.Key.CapsLock;

        private void implementation_OnMoved(Veldrid.Point point)
        {
            checkCurrentDisplay();
            OnMoved(new Point(point.X, point.Y));
        }

        private void implementation_OnMouseWheel(MouseWheelEventArgs args) =>
            OnMouseWheel(new MouseScrollRelativeInput { Delta = new TKVector2(0, args.WheelDelta) });

        private void implementation_OnMouseDown(MouseEvent evt) =>
            OnMouseDown(new MouseButtonInput((MouseButton)evt.MouseButton, evt.Down));

        private void implementation_OnMouseUp(MouseEvent evt) =>
            OnMouseUp(new MouseButtonInput((MouseButton)evt.MouseButton, evt.Down));

        private void implementation_OnMouseMove(MouseMoveEventArgs args) =>
            OnMouseMove(new MousePositionAbsoluteInput { Position = args.MousePosition.ToOsuTK() * Scale });

        private void implementation_OnKeyDown(KeyEvent evt)
        {
            if (isKeyValid(evt.Key))
                OnKeyDown(new KeyboardKeyInput((Key)evt.Key, evt.Down));
        }

        private void implementation_OnKeyUp(KeyEvent evt)
        {
            if (isKeyValid(evt.Key))
                OnKeyUp(new KeyboardKeyInput((Key)evt.Key, evt.Down));
        }

        private void implementation_DragDrop(DragDropEvent evt) =>
            OnDragDrop(evt.File);

        private void implementation_Resized()
        {
            scale.Invalidate();

            checkCurrentDisplay();

            if (implementation.WindowState.ToFramework() != windowState)
                OnWindowStateChanged();

            OnResized();
        }

        private void checkCurrentDisplay()
        {
            if (previousDisplayIndex == currentDisplayIndex)
                return;

            scale.Invalidate();
            previousDisplayIndex = currentDisplayIndex;
            OnDisplayChanged(CurrentDisplay);
        }

        private static SDL_WindowFlags getWindowFlags(WindowState state)
        {
            switch (state)
            {
                case WindowState.Normal:
                    return 0;

                case WindowState.Fullscreen:
                    return SDL_WindowFlags.Fullscreen;

                case WindowState.Maximised:
                    return SDL_WindowFlags.Maximized;

                case WindowState.Minimised:
                    return SDL_WindowFlags.Minimized;

                case WindowState.FullscreenBorderless:
                    return SDL_WindowFlags.FullScreenDesktop;
            }

            return 0;
        }
    }
}
