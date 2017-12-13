// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using Markdig;
using osu.Framework.Caching;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface.Markdown.Renderers;

namespace osu.Framework.Graphics.UserInterface.Markdown
{
    public class MarkdownTextContainer : CompositeDrawable
    {
        private string text;
        public string Text
        {
            get { return text; }
            set
            {
                if (text == value)
                    return;
                text = value;

                layoutCache.Invalidate();
            }
        }

        public new Axes AutoSizeAxes
        {
            get { return base.AutoSizeAxes; }
            set { base.AutoSizeAxes = value; }
        }

        private Cached layoutCache = new Cached();

        protected override void Update()
        {
            base.Update();

            if (!layoutCache.IsValid)
            {
                layoutText();
                layoutCache.Validate();
            }
        }

        private void layoutText()
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

            var document = (FillFlowContainer)Markdig.Markdown.Convert(text, new MarkdownRenderer(), pipeline);
            document.RelativeSizeAxes = Axes.X;
            document.AutoSizeAxes = Axes.Y;

            InternalChild = document;
        }
    }
}
