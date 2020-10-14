// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Framework.Graphics.Containers
{
    public class LifetimeManagementContainer : CompositeDrawable
    {
        protected readonly LifetimeManager LifetimeManager = new LifetimeManager();
        private readonly Dictionary<Drawable, DrawableLifetimeEntry> drawableMap = new Dictionary<Drawable, DrawableLifetimeEntry>();

        public LifetimeManagementContainer()
        {
            LifetimeManager.OnBecomeAlive += OnBecomeAlive;
            LifetimeManager.OnBecomeDead += OnBecomeDead;
            LifetimeManager.OnBoundaryCrossed += OnBoundaryCrossed;
        }

        protected internal override void AddInternal(Drawable drawable)
        {
            var entry = new DrawableLifetimeEntry(drawable);
            drawableMap[drawable] = entry;

            LifetimeManager.AddEntry(entry);
            base.AddInternal(drawable);
        }

        protected internal override bool RemoveInternal(Drawable drawable)
        {
            if (!drawableMap.TryGetValue(drawable, out var entry))
                return base.RemoveInternal(drawable);

            entry.Dispose();

            drawableMap.Remove(drawable);
            LifetimeManager.RemoveEntry(entry);
            base.RemoveInternal(drawable);

            return true;
        }

        protected internal override void ClearInternal(bool disposeChildren = true)
        {
            foreach (var (_, entry) in drawableMap)
                entry.Dispose();

            drawableMap.Clear();
            LifetimeManager.ClearEntries();
            base.ClearInternal(disposeChildren);
        }

        /// <summary>
        /// Forwards directly to to <see cref="CompositeDrawable.AddInternal"/> without tracking lifetime.
        /// </summary>
        protected internal virtual void AddInternalAlwaysAlive(Drawable drawable)
        {
            base.AddInternal(drawable);
            MakeChildAlive(drawable);
        }

        protected override bool CheckChildrenLife() => LifetimeManager.Update(Time.Current);

        protected virtual void OnBecomeAlive(LifetimeEntry entry)
        {
            var drawable = GetDrawableFor(entry);

            if (drawable.Parent != this)
                base.AddInternal(drawable);

            MakeChildAlive(drawable);
        }

        protected virtual void OnBecomeDead(LifetimeEntry entry) => MakeChildDead(GetDrawableFor(entry));

        protected virtual void OnBoundaryCrossed(LifetimeEntry entry, LifetimeBoundaryKind kind, LifetimeBoundaryCrossingDirection direction)
            => OnChildLifetimeBoundaryCrossed(new LifetimeBoundaryCrossedEvent(GetDrawableFor(entry), kind, direction));

        protected virtual Drawable GetDrawableFor(LifetimeEntry entry) => ((DrawableLifetimeEntry)entry).Drawable;

        /// <summary>
        /// Called when the clock is crossed child lifetime boundary.
        /// If child's lifetime is changed during this callback and that causes additional crossings,
        /// those events are queued and this method will be called later (on the same frame).
        /// </summary>
        protected virtual void OnChildLifetimeBoundaryCrossed(LifetimeBoundaryCrossedEvent e)
        {
        }
    }

    public class DrawableLifetimeEntry : LifetimeEntry, IDisposable
    {
        public readonly Drawable Drawable;

        public DrawableLifetimeEntry(Drawable drawable)
        {
            Drawable = drawable;

            Drawable.LifetimeChanged += drawableLifetimeChanged;
            drawableLifetimeChanged(Drawable);
        }

        private void drawableLifetimeChanged(Drawable drawable)
        {
            LifetimeStart = drawable.LifetimeStart;
            LifetimeEnd = drawable.LifetimeEnd;
        }

        public void Dispose()
        {
            if (Drawable != null)
                Drawable.LifetimeChanged -= drawableLifetimeChanged;
        }
    }

    /// <summary>
    /// Represents a direction of lifetime boundary crossing.
    /// </summary>
    public enum LifetimeBoundaryCrossingDirection
    {
        /// <summary>
        /// A crossing from past to future.
        /// </summary>
        Forward,

        /// <summary>
        /// A crossing from future to past.
        /// </summary>
        Backward,
    }

    /// <summary>
    /// Represents one of boundaries of lifetime interval.
    /// </summary>
    public enum LifetimeBoundaryKind
    {
        /// <summary>
        /// <see cref="Drawable.LifetimeStart"/>.
        /// </summary>
        Start,

        /// <summary>
        /// <see cref="Drawable.LifetimeEnd"/>.
        /// </summary>
        End,
    }

    /// <summary>
    /// Represents that the clock is crossed <see cref="LifetimeManagementContainer"/>'s child lifetime boundary i.e. <see cref="Drawable.LifetimeStart"/> or <see cref="Drawable.LifetimeEnd"/>,
    /// </summary>
    public readonly struct LifetimeBoundaryCrossedEvent
    {
        /// <summary>
        /// The drawable.
        /// </summary>
        public readonly Drawable Child;

        /// <summary>
        /// Which lifetime boundary is crossed.
        /// </summary>
        public readonly LifetimeBoundaryKind Kind;

        /// <summary>
        /// The direction of the crossing.
        /// </summary>
        public readonly LifetimeBoundaryCrossingDirection Direction;

        public LifetimeBoundaryCrossedEvent(Drawable child, LifetimeBoundaryKind kind, LifetimeBoundaryCrossingDirection direction)
        {
            Child = child;
            Kind = kind;
            Direction = direction;
        }

        public override string ToString() => $"({Child.ChildID}, {Kind}, {Direction})";
    }
}
