// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using osu.Framework.Graphics.Containers;
using osu.Framework.Statistics;

namespace osu.Framework.Graphics
{
    public class LifetimeManager
    {
        public event Action<LifetimeEntry> OnBecomeAlive;
        public event Action<LifetimeEntry> OnBecomeDead;
        public event Action<LifetimeEntry, LifetimeBoundaryKind, LifetimeBoundaryCrossingDirection> OnBoundaryCrossed;

        private readonly HashSet<LifetimeEntry> newEntries = new HashSet<LifetimeEntry>();
        private readonly HashSet<LifetimeEntry> activeEntries = new HashSet<LifetimeEntry>();
        private readonly SortedSet<LifetimeEntry> futureEntries = new SortedSet<LifetimeEntry>(new LifetimeStartComparator());
        private readonly SortedSet<LifetimeEntry> pastEntries = new SortedSet<LifetimeEntry>(new LifetimeEndComparator());

        private readonly Queue<(LifetimeEntry, LifetimeBoundaryKind, LifetimeBoundaryCrossingDirection)> eventQueue =
            new Queue<(LifetimeEntry, LifetimeBoundaryKind, LifetimeBoundaryCrossingDirection)>();

        private ulong currentChildId;

        public void AddEntry(LifetimeEntry entry)
        {
            entry.RequestLifetimeUpdate += requestLifetimeUpdate;
            entry.ChildId = ++currentChildId;
            entry.State = LifetimeState.New;

            newEntries.Add(entry);
        }

        public bool RemoveEntry(LifetimeEntry entry)
        {
            entry.RequestLifetimeUpdate -= requestLifetimeUpdate;

            bool removed = false;

            switch (entry.State)
            {
                case LifetimeState.New:
                    removed = newEntries.Remove(entry);
                    break;

                case LifetimeState.Current:
                    removed = activeEntries.Remove(entry);

                    if (removed)
                        OnBecomeDead?.Invoke(entry);

                    break;

                case LifetimeState.Past:
                    removed = pastEntries.Remove(entry);
                    break;

                case LifetimeState.Future:
                    removed = futureEntries.Remove(entry);
                    break;
            }

            if (!removed)
                return false;

            entry.ChildId = 0;
            return true;
        }

        public void ClearEntries()
        {
            foreach (var entry in newEntries)
            {
                entry.RequestLifetimeUpdate -= requestLifetimeUpdate;
                entry.ChildId = 0;
            }

            foreach (var entry in pastEntries)
            {
                entry.RequestLifetimeUpdate -= requestLifetimeUpdate;
                entry.ChildId = 0;
            }

            foreach (var entry in activeEntries)
            {
                entry.RequestLifetimeUpdate -= requestLifetimeUpdate;
                entry.ChildId = 0;
            }

            foreach (var entry in futureEntries)
            {
                entry.RequestLifetimeUpdate -= requestLifetimeUpdate;
                entry.ChildId = 0;
            }

            pastEntries.Clear();
            activeEntries.Clear();
            futureEntries.Clear();
        }

        private void requestLifetimeUpdate(LifetimeEntry entry, double lifetimeStart, double lifetimeEnd)
        {
            var futureOrPastSet = futureOrPastEntries(entry.State);

            if (futureOrPastSet != null)
            {
                futureOrPastSet.Remove(entry);
                newEntries.Add(entry); // Enqueue the entry to update in the next frame.
            }

            entry.UpdateLifetime(lifetimeStart, lifetimeEnd);
        }

        [CanBeNull]
        private SortedSet<LifetimeEntry> futureOrPastEntries(LifetimeState state)
        {
            switch (state)
            {
                case LifetimeState.Future:
                    return futureEntries;

                case LifetimeState.Past:
                    return pastEntries;

                default:
                    return null;
            }
        }

        public bool Update(double time) => Update(time, time);

        public bool Update(double startTime, double endTime)
        {
            endTime = Math.Max(endTime, startTime);

            bool aliveChildrenChanged = false;

            // Check for newly-added entries.
            foreach (var entry in newEntries)
                aliveChildrenChanged |= updateChildEntry(startTime, endTime, entry, true, true);
            newEntries.Clear();

            // Check for newly alive entries when time is increased.
            while (futureEntries.Count > 0)
            {
                FrameStatistics.Increment(StatisticsCounterType.CCL);

                var entry = futureEntries.Min;
                Debug.Assert(entry.State == LifetimeState.Future);

                if (CompareRanges((entry.LifetimeStart, entry.LifetimeEnd), (startTime, endTime)) == LifetimeState.Future)
                    break;

                futureEntries.Remove(entry);
                aliveChildrenChanged |= updateChildEntry(startTime, endTime, entry, false, true);
            }

            // Check for newly alive entries when time is decreased.
            while (pastEntries.Count > 0)
            {
                FrameStatistics.Increment(StatisticsCounterType.CCL);

                var entry = pastEntries.Max;
                Debug.Assert(entry.State == LifetimeState.Past);

                if (CompareRanges((entry.LifetimeStart, entry.LifetimeEnd), (startTime, endTime)) == LifetimeState.Past)
                    break;

                pastEntries.Remove(entry);
                aliveChildrenChanged |= updateChildEntry(startTime, endTime, entry, false, true);
            }

            // Checks for newly dead entries when time is increased/decreased.
            foreach (var entry in activeEntries)
            {
                FrameStatistics.Increment(StatisticsCounterType.CCL);
                aliveChildrenChanged |= updateChildEntry(startTime, endTime, entry, false, false);
            }

            // Remove all newly-dead entries.
            activeEntries.RemoveWhere(e => e.State != LifetimeState.Current);

            while (eventQueue.Count != 0)
            {
                var (entry, kind, direction) = eventQueue.Dequeue();
                OnBoundaryCrossed?.Invoke(entry, kind, direction);
            }

            return aliveChildrenChanged;
        }

        private bool updateChildEntry(double startTime, double endTime, LifetimeEntry entry, bool fromLifetimeChange, bool mutateActive)
        {
            LifetimeState oldState = entry.State;

            Debug.Assert(!futureEntries.Contains(entry) && !pastEntries.Contains(entry));
            Debug.Assert(oldState != LifetimeState.Current || activeEntries.Contains(entry));

            LifetimeState newState = CompareRanges((entry.LifetimeStart, entry.LifetimeEnd), (startTime, endTime));

            // If the state hasn't changed...
            if (newState == oldState)
            {
                // Then we need to re-insert to future/past entries if updating from a lifetime change event.
                if (fromLifetimeChange)
                    futureOrPastEntries(newState)?.Add(entry);
                // Otherwise, we should only be here if we're updating the active entries.
                else
                    Debug.Assert(newState == LifetimeState.Current);

                return false;
            }

            bool aliveEntriesChanged = false;

            if (newState == LifetimeState.Current)
            {
                if (mutateActive)
                    activeEntries.Add(entry);

                OnBecomeAlive?.Invoke(entry);
                aliveEntriesChanged = true;
            }
            else if (oldState == LifetimeState.Current)
            {
                if (mutateActive)
                    activeEntries.Remove(entry);

                OnBecomeDead?.Invoke(entry);
                aliveEntriesChanged = true;
            }

            entry.State = newState;
            futureOrPastEntries(newState)?.Add(entry);
            enqueueEvents(entry, oldState, newState);

            return aliveEntriesChanged;
        }

        public static LifetimeState CompareRanges((double start, double end) a, (double start, double end) b)
        {
            if (a.end < b.start)
                return LifetimeState.Past;

            if (a.start >= b.end)
                return LifetimeState.Future;

            return LifetimeState.Current;
        }

        private void enqueueEvents(LifetimeEntry entry, LifetimeState oldState, LifetimeState newState)
        {
            Debug.Assert(oldState != newState);

            switch (oldState)
            {
                case LifetimeState.Future:
                    eventQueue.Enqueue((entry, LifetimeBoundaryKind.Start, LifetimeBoundaryCrossingDirection.Forward));
                    if (newState == LifetimeState.Past)
                        eventQueue.Enqueue((entry, LifetimeBoundaryKind.End, LifetimeBoundaryCrossingDirection.Forward));
                    break;

                case LifetimeState.Current:
                    eventQueue.Enqueue(newState == LifetimeState.Past
                        ? (entry, LifetimeBoundaryKind.End, LifetimeBoundaryCrossingDirection.Forward)
                        : (entry, LifetimeBoundaryKind.Start, LifetimeBoundaryCrossingDirection.Backward));
                    break;

                case LifetimeState.Past:
                    eventQueue.Enqueue((entry, LifetimeBoundaryKind.End, LifetimeBoundaryCrossingDirection.Backward));
                    if (newState == LifetimeState.Future)
                        eventQueue.Enqueue((entry, LifetimeBoundaryKind.Start, LifetimeBoundaryCrossingDirection.Backward));
                    break;
            }
        }

        /// <summary>
        /// Compare by <see cref="LifetimeEntry.LifetimeStart"/>.
        /// </summary>
        private sealed class LifetimeStartComparator : IComparer<LifetimeEntry>
        {
            public int Compare(LifetimeEntry x, LifetimeEntry y)
            {
                if (x == null) throw new ArgumentNullException(nameof(x));
                if (y == null) throw new ArgumentNullException(nameof(y));

                int c = x.LifetimeStart.CompareTo(y.LifetimeStart);
                return c != 0 ? c : x.ChildId.CompareTo(y.ChildId);
            }
        }

        /// <summary>
        /// Compare by <see cref="LifetimeEntry.LifetimeEnd"/>.
        /// </summary>
        private sealed class LifetimeEndComparator : IComparer<LifetimeEntry>
        {
            public int Compare(LifetimeEntry x, LifetimeEntry y)
            {
                if (x == null) throw new ArgumentNullException(nameof(x));
                if (y == null) throw new ArgumentNullException(nameof(y));

                int c = x.LifetimeEnd.CompareTo(y.LifetimeEnd);
                return c != 0 ? c : x.ChildId.CompareTo(y.ChildId);
            }
        }
    }

    public class LifetimeEntry
    {
        private double lifetimeStart = double.MinValue;

        public double LifetimeStart
        {
            get => lifetimeStart;
            set
            {
                if (lifetimeStart == value)
                    return;

                if (RequestLifetimeUpdate != null)
                    RequestLifetimeUpdate.Invoke(this, value, lifetimeEnd);
                else
                    UpdateLifetime(value, lifetimeEnd);
            }
        }

        private double lifetimeEnd = double.MaxValue;

        public double LifetimeEnd
        {
            get => lifetimeEnd;
            set
            {
                if (lifetimeEnd == value)
                    return;

                if (RequestLifetimeUpdate != null)
                    RequestLifetimeUpdate.Invoke(this, lifetimeStart, value);
                else
                    UpdateLifetime(lifetimeStart, value);
            }
        }

        internal event RequestLifetimeUpdateDelegate RequestLifetimeUpdate;

        /// <summary>
        /// Updates the stored lifetimes of this <see cref="LifetimeEntry"/>.
        /// </summary>
        /// <param name="start">The new <see cref="lifetimeStart"/> value.</param>
        /// <param name="end">The new <see cref="lifetimeEnd"/> value.</param>
        internal void UpdateLifetime(double start, double end)
        {
            lifetimeStart = start;
            lifetimeEnd = Math.Max(start, end); // Negative intervals are undesired.
        }

        /// <summary>
        /// The current state of this <see cref="LifetimeEntry"/>.
        /// </summary>
        internal LifetimeState State { get; set; }

        internal ulong ChildId { get; set; }
    }

    public enum LifetimeState
    {
        /// Not yet loaded.
        New,

        /// Currently dead and becomes alive in the future: current time &lt; <see cref="Drawable.LifetimeStart"/>.
        Future,

        /// Currently alive.
        Current,

        /// Currently dead and becomes alive if the clock is rewound: <see cref="Drawable.LifetimeEnd"/> &lt;= current time.
        Past,
    }

    internal delegate void RequestLifetimeUpdateDelegate(LifetimeEntry entry, double lifetimeStart, double lifetimeEnd);
}
