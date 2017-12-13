// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using Markdig.Renderers;
using Markdig.Syntax;

namespace osu.Framework.Graphics.UserInterface.Markdown.Renderers
{
    public abstract class ObjectRenderer<T> : MarkdownObjectRenderer<MarkdownRenderer, T>
        where T : MarkdownObject
    {
    }
}
