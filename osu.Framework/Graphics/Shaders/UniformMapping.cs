// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace osu.Framework.Graphics.Shaders
{
    /// <summary>
    /// A mapping of a global uniform to many shaders which need to receive updates on a change.
    /// </summary>
    internal class UniformMapping<T> : IUniformMapping
        where T : struct, IEquatable<T>
    {
        public List<GlobalUniform<T>> LinkedUniforms = new List<GlobalUniform<T>>();

        public string Name { get; }

        public readonly T[] Value;

        public UniformMapping(string name)
            : this(name, 1)
        {
        }

        public UniformMapping(string name, int count)
        {
            Name = name;
            Value = new T[count];
        }

        public void LinkShaderUniform(IUniform uniform)
        {
            var typedUniform = (GlobalUniform<T>)uniform;

            typedUniform.UpdateValue(this);
            LinkedUniforms.Add(typedUniform);
        }

        public void UnlinkShaderUniform(IUniform uniform)
        {
            var typedUniform = (GlobalUniform<T>)uniform;
            LinkedUniforms.Remove(typedUniform);
        }

        public void SetValue(ref T value)
        {
            Value[0] = value;

            for (int i = 0; i < LinkedUniforms.Count; i++)
                LinkedUniforms[i].UpdateValue(this);
        }

        public void SetValue(in ReadOnlySpan<T> span)
        {
            Debug.Assert(span.Length == Value.Length);

            span.CopyTo(Value);

            for (int i = 0; i < LinkedUniforms.Count; i++)
                LinkedUniforms[i].UpdateValue(this);
        }

        public ref T GetValueByRef() => ref Value[0];
    }
}
