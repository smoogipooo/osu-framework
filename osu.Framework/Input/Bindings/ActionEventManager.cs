// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input.States;
using osu.Framework.Logging;

namespace osu.Framework.Input.Bindings
{
    public class ActionEventManager<TAction> : ButtonEventManager<TAction>
        where TAction : struct
    {
        public ActionEventManager(TAction action)
            : base(action)
        {
        }

        public bool HandleScroll(InputState state, float scrollAmount, bool isPrecise)
        {
            IDrawable handled = InputQueue.OfType<IScrollBindingHandler<TAction>>().FirstOrDefault(d => d.OnScroll(Button, scrollAmount, isPrecise));

            if (handled != null)
                Logger.Log($"Scrolled ({Button}) handled by {handled}.", LoggingTarget.Runtime, LogLevel.Debug);

            return handled != null;
        }

        protected override Drawable HandleButtonDown(InputState state, List<Drawable> targets)
        {
            Drawable handled = (Drawable)InputQueue.OfType<IKeyBindingHandler<TAction>>().FirstOrDefault(d => d.OnPressed(Button));

            if (handled != null)
                Logger.Log($"Pressed ({Button}) handled by {handled}.", LoggingTarget.Runtime, LogLevel.Debug);

            return handled;
        }

        protected override void HandleButtonUp(InputState state, List<Drawable> targets) => PropagateReleased();

        public void PropagateReleased()
        {
            var handledBy = ButtonDownInputQueue?.OfType<IKeyBindingHandler<TAction>>().FirstOrDefault(d => d.OnReleased(Button));

            if (handledBy != null)
                Logger.Log($"Released {Button} handled by {handledBy}.", LoggingTarget.Runtime, LogLevel.Debug);
        }
    }
}
