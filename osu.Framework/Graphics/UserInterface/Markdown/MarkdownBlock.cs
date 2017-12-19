using Markdig.Syntax;
using OpenTK;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Framework.Graphics.UserInterface.Markdown
{
    public class MarkdownBlock : CustomizableTextContainer
    {
        public MarkdownBlock(IBlock block)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Spacing = new Vector2(0, 15);

            write(block);
        }

        private void write(IBlock block)
        {
            if (block == null)
                return;

            bool parseContainer = true;

            switch (block)
            {
                // We always want to handle the document
                case MarkdownDocument document:
                    break;
                // Never handle HTML blocks (for now?)
                case HtmlBlock htmlBlock:
                    return;
                // Add lines for paragraphs
                case ParagraphBlock paragraph:
                    AddText($"[{AddPlaceholder(new Container())}]");
                    break;
                // Display the name for any ContainerBlock types not handled above
                case ContainerBlock container:
                    AddText($"{{{block.GetType()}}}");
                    NewLine();
                    parseContainer = false;
                    break;
            }

            switch (block)
            {
                case ContainerBlock container:
                    if (!parseContainer)
                        break;

                    foreach (var nestedBlock in container)
                        write(nestedBlock);
                    break;
                case LeafBlock span:
                    AddText($"[{AddPlaceholder(new MarkdownSpan(span.Inline))}]");
                    break;
                default:
                    break;
            }
        }
    }
}
