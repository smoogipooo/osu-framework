// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace osu.Framework.Graphics.Containers
{
    public class LifetimeManagementContainer : CompositeDrawable
    {
        private readonly Dictionary<Drawable, DrawableLifetimeEntry> drawableMap = new Dictionary<Drawable, DrawableLifetimeEntry>();
        private readonly LifetimeManager lifetimeManager = new LifetimeManager();

        public LifetimeManagementContainer()
        {
            lifetimeManager.OnBecomeAlive += onBecomeAlive;
            lifetimeManager.OnBecomeDead += onBecomeDead;
            lifetimeManager.OnBoundaryCrossed += onBoundaryCrossed;
        }

        protected internal override void AddInternal(Drawable drawable)
        {
            Trace.Assert(!drawable.RemoveWhenNotAlive, $"{nameof(RemoveWhenNotAlive)} is not supported for {nameof(LifetimeManagementContainer)}");

            var entry = new DrawableLifetimeEntry(drawable);
            drawableMap[drawable] = entry;

            lifetimeManager.AddEntry(entry);
            base.AddInternal(drawable);
        }

        protected internal override bool RemoveInternal(Drawable drawable)
        {
            if (!drawableMap.TryGetValue(drawable, out var entry))
                return false;

            entry.Dispose();

            drawableMap.Remove(drawable);
            lifetimeManager.RemoveEntry(entry);
            base.RemoveInternal(drawable);

            return true;
        }

        protected internal override void ClearInternal(bool disposeChildren = true)
        {
            foreach (var (_, entry) in drawableMap)
                entry.Dispose();

            drawableMap.Clear();
            lifetimeManager.ClearEntries();
            base.ClearInternal(disposeChildren);
        }

        protected override bool CheckChildrenLife() => lifetimeManager.Update(Time.Current);

        private void onBecomeAlive(LifetimeEntry entry) => MakeChildAlive(((DrawableLifetimeEntry)entry).Drawable);

        private void onBecomeDead(LifetimeEntry entry)
        {
            bool removed = MakeChildDead(((DrawableLifetimeEntry)entry).Drawable);
            Trace.Assert(!removed, $"{nameof(RemoveWhenNotAlive)} is not supported for children of {nameof(LifetimeManagementContainer)}");
        }

        private void onBoundaryCrossed(LifetimeEntry entry, LifetimeBoundaryKind kind, LifetimeBoundaryCrossingDirection direction)
            => OnChildLifetimeBoundaryCrossed(new LifetimeBoundaryCrossedEvent(((DrawableLifetimeEntry)entry).Drawable, kind, direction));

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
