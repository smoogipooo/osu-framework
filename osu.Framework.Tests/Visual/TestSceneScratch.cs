// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osuTK;

namespace osu.Framework.Tests.Visual
{
    public class TestSceneScratch : FrameworkTestScene
    {
        private readonly CurveEasingFunction easingFunction;
        private readonly Box box;

        private readonly BindableList<Vector2> path = new BindableList<Vector2>();

        public TestSceneScratch()
        {
            easingFunction = new CurveEasingFunction
            {
                Path = { BindTarget = path }
            };

            AddRange(new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Padding = new MarginPadding
                    {
                        Left = 10,
                        Top = 10,
                        Right = 60
                    },
                    Child = box = new Box
                    {
                        RelativePositionAxes = Axes.Both,
                        Size = new Vector2(50)
                    }
                },
                new PathCreator
                {
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    Y = 200,
                    Size = new Vector2(500, 300),
                    Path = { BindTarget = path }
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            box.MoveTo(Vector2.Zero)
               .TransformTo(nameof(Position), new Vector2(1, 0), 1000, easingFunction)
               .Then().Delay(200)
               .Loop();
        }

        private class PathCreator : CompositeDrawable
        {
            public readonly BindableList<Vector2> Path = new BindableList<Vector2>();

            private readonly List<Vector2> controlPoints = new List<Vector2>
            {
                Vector2.Zero,
                Vector2.One
            };

            private readonly SmoothPath smoothPath;

            public PathCreator()
            {
                InternalChildren = new Drawable[]
                {
                    smoothPath = new SmoothPath
                    {
                        AutoSizeAxes = Axes.None,
                        RelativeSizeAxes = Axes.Both,
                        PathRadius = 2,
                    }
                };
            }

            protected override void Update()
            {
                base.Update();

                Path.Clear();
                Path.AddRange(PathApproximator.ApproximateBezier(controlPoints.ToArray()));

                var effectiveWidth = DrawWidth - smoothPath.PathRadius / 2;
                var effectiveHeight = DrawHeight - smoothPath.PathRadius;

                smoothPath.Vertices = Path.Select(p => new Vector2(p.X * effectiveWidth, p.Y * effectiveHeight)).ToArray();
            }

            protected override bool OnClick(ClickEvent e)
            {
                var pos = Vector2.Divide(ToLocalSpace(e.ScreenSpaceMouseDownPosition), DrawSize);

                for (int i = 0; i < controlPoints.Count; i++)
                {
                    if (controlPoints[i].X > pos.X)
                    {
                        controlPoints.Insert(i, pos);
                        break;
                    }
                }

                return true;
            }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;
        }

        private class CurveEasingFunction : IEasingFunction
        {
            public readonly BindableList<Vector2> Path = new BindableList<Vector2>();

            public double Apply(double time)
            {
                if (Path.Count == 0)
                    return 0;

                int start = 0;

                for (start = 0; start < Path.Count; start++)
                {
                    if (Path[start].X > time)
                    {
                        start--;
                        break;
                    }
                }

                start = Math.Max(0, start);
                int end = Math.Min(Path.Count - 1, start + 1);

                var startVector = Path[start];
                var endVector = Path[end];

                // Interpolate between the two points
                return Interpolation.ValueAt(time, startVector.Y, endVector.Y, startVector.X, endVector.X, Easing.None);
            }
        }
    }
}
