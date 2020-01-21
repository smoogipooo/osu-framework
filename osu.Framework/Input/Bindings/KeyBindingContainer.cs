// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Input.StateChanges;
using osu.Framework.Input.States;
using osuTK;

namespace osu.Framework.Input.Bindings
{
    /// <summary>
    /// Maps input actions to custom action data of type <typeparamref name="T"/>. Use in conjunction with <see cref="Drawable"/>s implementing <see cref="IKeyBindingHandler{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the custom action.</typeparam>
    public abstract class KeyBindingContainer<T> : KeyBindingContainer
        where T : struct
    {
        private readonly SimultaneousBindingMode simultaneousMode;
        private readonly KeyCombinationMatchingMode matchingMode;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="simultaneousMode">Specify how to deal with multiple matches of <see cref="KeyCombination"/>s and <typeparamref name="T"/>s.</param>
        /// <param name="matchingMode">Specify how to deal with exact <see cref="KeyCombination"/> matches.</param>
        protected KeyBindingContainer(SimultaneousBindingMode simultaneousMode = SimultaneousBindingMode.None, KeyCombinationMatchingMode matchingMode = KeyCombinationMatchingMode.Any)
        {
            RelativeSizeAxes = Axes.Both;

            this.simultaneousMode = simultaneousMode;
            this.matchingMode = matchingMode;
        }

        private readonly List<KeyBinding> pressedBindings = new List<KeyBinding>();

        private readonly List<T> pressedActions = new List<T>();

        /// <summary>
        /// All actions in a currently pressed state.
        /// </summary>
        public IEnumerable<T> PressedActions => pressedActions;

        private readonly List<Drawable> queue = new List<Drawable>();

        /// <summary>
        /// The input queue to be used for processing key bindings. Based on the non-positional <see cref="InputManager.NonPositionalInputQueue"/>.
        /// Can be overridden to change priorities.
        /// </summary>
        protected virtual IEnumerable<Drawable> KeyBindingInputQueue
        {
            get
            {
                queue.Clear();
                BuildNonPositionalInputQueue(queue, false);
                queue.Reverse();

                return queue;
            }
        }

        protected override void Update()
        {
            base.Update();

            // aggressively clear to avoid holding references.
            queue.Clear();
        }

        /// <summary>
        /// Override to enable or disable sending of repeated actions (disabled by default).
        /// Each repeated action will have its own pressed/released event pair.
        /// </summary>
        protected virtual bool SendRepeats => false;

        /// <summary>
        /// Whether this <see cref="KeyBindingContainer"/> should attempt to handle input before any of its children.
        /// </summary>
        protected virtual bool Prioritised => false;

        internal override bool BuildNonPositionalInputQueue(List<Drawable> queue, bool allowBlocking = true)
        {
            if (!base.BuildNonPositionalInputQueue(queue, allowBlocking))
                return false;

            if (Prioritised)
            {
                queue.Remove(this);
                queue.Add(this);
            }

            return true;
        }

        protected override bool Handle(UIEvent e)
        {
            var state = e.CurrentState;

            switch (e)
            {
                case MouseDownEvent mouseDown:
                    return handleNewPressed(state, KeyCombination.FromMouseButton(mouseDown.Button), false);

                case MouseUpEvent mouseUp:
                    handleNewReleased(state, KeyCombination.FromMouseButton(mouseUp.Button));
                    return false;

                case KeyDownEvent keyDown:
                    if (keyDown.Repeat && !SendRepeats)
                        return pressedBindings.Count > 0;

                    return handleNewPressed(state, KeyCombination.FromKey(keyDown.Key), keyDown.Repeat);

                case KeyUpEvent keyUp:
                    handleNewReleased(state, KeyCombination.FromKey(keyUp.Key));
                    return false;

                case JoystickPressEvent joystickPress:
                    return handleNewPressed(state, KeyCombination.FromJoystickButton(joystickPress.Button), false);

                case JoystickReleaseEvent joystickRelease:
                    handleNewReleased(state, KeyCombination.FromJoystickButton(joystickRelease.Button));
                    return false;

                case ScrollEvent scroll:
                {
                    var key = KeyCombination.FromScrollDelta(scroll.ScrollDelta);
                    if (key == InputKey.None) return false;

                    bool handled = handleNewPressed(state, key, false, scroll.ScrollDelta, scroll.IsPrecise);
                    handleNewReleased(state, key);

                    return handled;
                }
            }

            return false;
        }

        private bool handleNewPressed(InputState state, InputKey newKey, bool repeat, Vector2? scrollDelta = null, bool isPrecise = false)
        {
            float scrollAmount = 0;
            if (newKey == InputKey.MouseWheelUp)
                scrollAmount = scrollDelta?.Y ?? 0;
            else if (newKey == InputKey.MouseWheelDown)
                scrollAmount = -(scrollDelta?.Y ?? 0);

            var pressedCombination = KeyCombination.FromInputState(state, scrollDelta);

            var bindings = (repeat ? KeyBindings : KeyBindings?.Except(pressedBindings)) ?? Enumerable.Empty<KeyBinding>();
            var newlyPressed = bindings.Where(m =>
                m.KeyCombination.Keys.Contains(newKey) // only handle bindings matching current key (not required for correct logic)
                && m.KeyCombination.IsPressed(pressedCombination, matchingMode));

            if (KeyCombination.IsModifierKey(newKey))
                // if the current key pressed was a modifier, only handle modifier-only bindings.
                newlyPressed = newlyPressed.Where(b => b.KeyCombination.Keys.All(KeyCombination.IsModifierKey));

            // we want to always handle bindings with more keys before bindings with less.
            newlyPressed = newlyPressed.OrderByDescending(b => b.KeyCombination.Keys.Length).ToList();

            if (!repeat)
                pressedBindings.AddRange(newlyPressed);

            // exact matching may result in no pressed (new or old) bindings, in which case we want to trigger releases for existing actions
            if (simultaneousMode == SimultaneousBindingMode.None && (matchingMode == KeyCombinationMatchingMode.Exact || matchingMode == KeyCombinationMatchingMode.Modifiers))
            {
                // only want to release pressed actions if no existing bindings would still remain pressed
                if (pressedBindings.Count > 0 && !pressedBindings.Any(m => m.KeyCombination.IsPressed(pressedCombination, matchingMode)))
                    releasePressedActions();
            }

            bool handled = false;

            foreach (var newBinding in newlyPressed)
            {
                handled |= handlePressed(getEventManager(newBinding), scrollAmount, isPrecise);

                // we only want to handle the first valid binding (the one with the most keys) in non-simultaneous mode.
                if (simultaneousMode == SimultaneousBindingMode.None && handled)
                    break;
            }

            return handled;
        }

        private bool handlePressed(ActionEventManager<T> eventManager, float scrollAmount = 0, bool isPrecise = false)
        {
            // if there already is an existing action and concurrency is not allowed, release the existing actions.
            if (simultaneousMode == SimultaneousBindingMode.None)
                releasePressedActions();

            // under unique concurrency mode, only one such action can be triggered at a time.
            if (simultaneousMode == SimultaneousBindingMode.Unique && pressedActions.Contains(eventManager.Button))
                return false;

            pressedActions.Add(eventManager.Button);

            return eventManager.HandleScroll(null, scrollAmount, isPrecise)
                   || eventManager.HandleButtonStateChange(null, ButtonStateChangeKind.Pressed);
        }

        /// <summary>
        /// Releases all pressed actions.
        /// Note that the relevant key bindings remain in a pressed state by the user and are not released by this method.
        /// </summary>
        private void releasePressedActions()
        {
            foreach (var manager in keyBindingEventManagers.Values)
                manager.HandleButtonStateChange(null, ButtonStateChangeKind.Released);

            pressedActions.Clear();
        }

        private void handleNewReleased(InputState state, InputKey releasedKey)
        {
            var pressedCombination = KeyCombination.FromInputState(state);

            // we don't want to consider exact matching here as we are dealing with bindings, not actions.
            var newlyReleased = pressedBindings.Where(b => !b.KeyCombination.IsPressed(pressedCombination, KeyCombinationMatchingMode.Any)).ToList();

            Trace.Assert(newlyReleased.All(b => b.KeyCombination.Keys.Contains(releasedKey)));

            foreach (var binding in newlyReleased)
            {
                pressedBindings.Remove(binding);

                handleReleased(getEventManager(binding));
            }
        }

        private void handleReleased(ActionEventManager<T> eventManager)
        {
            pressedActions.Remove(eventManager.Button);

            eventManager.HandleButtonStateChange(null, ButtonStateChangeKind.Released);
        }

        private readonly List<ActionEventManager<T>> manualEventManagers = new List<ActionEventManager<T>>();

        public void TriggerReleased(T released)
        {
            var manager = manualEventManagers.FirstOrDefault(m => EqualityComparer<T>.Default.Equals(m.Button, released));
            if (manager == null)
                throw new InvalidOperationException($"An action that hasn't been pressed cannot be released ({released}).");

            manualEventManagers.Remove(manager);
            handleReleased(manager);
        }

        public void TriggerPressed(T pressed)
        {
            var manager = createEventManager(pressed);

            manualEventManagers.Add(manager);
            handlePressed(manager);
        }

        private readonly Dictionary<KeyBinding, ActionEventManager<T>> keyBindingEventManagers = new Dictionary<KeyBinding, ActionEventManager<T>>();

        private ActionEventManager<T> createEventManager(T action) => new ActionEventManager<T>(action)
        {
            GetInputQueue = () => KeyBindingInputQueue
        };

        private ActionEventManager<T> getEventManager(KeyBinding binding)
        {
            if (keyBindingEventManagers.TryGetValue(binding, out var existing))
                return existing;

            return keyBindingEventManagers[binding] = createEventManager(binding.GetAction<T>());
        }
    }

    /// <summary>
    /// Maps input actions to custom action data.
    /// </summary>
    public abstract class KeyBindingContainer : Container
    {
        protected IEnumerable<KeyBinding> KeyBindings;

        public abstract IEnumerable<KeyBinding> DefaultKeyBindings { get; }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            ReloadMappings();
        }

        protected virtual void ReloadMappings()
        {
            KeyBindings = DefaultKeyBindings;
        }
    }

    public enum SimultaneousBindingMode
    {
        /// <summary>
        /// One action can be in a pressed state at once.
        /// If a new matching binding is encountered, any existing binding is first released.
        /// </summary>
        None,

        /// <summary>
        /// Unique actions are allowed to be pressed at the same time. There may therefore be more than one action in an actuated state at once.
        /// If one action has multiple bindings, only the first will trigger an <see cref="IKeyBindingHandler{T}.OnPressed"/>.
        /// The last binding to be released will trigger an <see cref="IKeyBindingHandler{T}.OnReleased(T)"/>.
        /// </summary>
        Unique,

        /// <summary>
        /// Unique actions are allowed to be pressed at the same time, as well as multiple times from different bindings. There may therefore be
        /// more than one action in an pressed state at once, as well as multiple consecutive <see cref="IKeyBindingHandler{T}.OnPressed"/> events
        /// for a single action (followed by an eventual balancing number of <see cref="IKeyBindingHandler{T}.OnReleased(T)"/> events).
        /// </summary>
        All,
    }
}
