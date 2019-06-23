// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.MathUtils;
using osuTK;

namespace osu.Framework.Graphics.Modelling
{
    public class Model : Drawable
    {
        private float z;

        public float Z
        {
            get => z;
            set
            {
                if (z == value) return;

                if (!Validation.IsFinite(value)) throw new ArgumentException($@"{nameof(Z)} must be finite, but is {value}.");

                z = value;

                Invalidate(Invalidation.MiscGeometry);
            }
        }

        public new Vector3 Position
        {
            get => new Vector3(X, Y, Z);
            set
            {
                base.Position = value.Xy;
                Z = value.Z;
            }
        }

        private float depth;

        public new float Depth
        {
            get => depth;
            set
            {
                if (depth == value) return;

                if (!Validation.IsFinite(value)) throw new ArgumentException($@"{nameof(Depth)} must be finite, but is {value}.");

                depth = value;

                Invalidate(Invalidation.DrawSize);
            }
        }

        public new Vector3 Size
        {
            get => new Vector3(Width, Height, Depth);
            set
            {
                base.Size = value.Xy;
                Depth = value.Z;
            }
        }

        private Vector3 rotation;

        public new Vector3 Rotation
        {
            get => rotation;
            set
            {
                if (value == rotation) return;

                if (!Validation.IsFinite(value)) throw new ArgumentException($@"{nameof(Rotation)} must be finite, but is {value}.");

                rotation = value;

                Invalidate(Invalidation.MiscGeometry);
            }
        }

        protected override DrawNode CreateDrawNode() => new ModelDrawNode(this);
    }

    public class ModelDrawNode : DrawNode
    {
        public new Model Source => (Model)base.Source;

        public ModelDrawNode(Model source)
            : base(source)
        {
        }
    }

    [Flags]
    public enum ModelAxes
    {
        None = 0,

        X = 1 << 0,
        Y = 1 << 1,
        Z = 1 << 2,

        All = X | Y | Z,
    }
}
