// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System.Linq;
using Markdig.Extensions.Tables;
using osu.Framework.Graphics.Containers;
using OpenTK;

namespace osu.Framework.Graphics.UserInterface.Markdown.Renderers.Tables
{
    public class TableRenderer : ObjectRenderer<Table>
    {
        protected override void Write(MarkdownRenderer renderer, Table obj)
        {
            int rows = obj.Count;
            int cols = obj.Max(r => ((TableRow)r).Count);
            var cells = new Drawable[rows][];

            var rowDimensions = new Dimension[rows];
            var colDimensions = new Dimension[cols];

            for (int r = 0; r < rows; r++)
            {
                var tableRow = (TableRow)obj[r];

                cells[r] = new Drawable[tableRow.Count];
                rowDimensions[r] = new Dimension(GridSizeMode.AutoSize);

                for (int c = 0; c < tableRow.Count; c++)
                {
                    colDimensions[c] = new Dimension(GridSizeMode.AutoSize);

                    var anchor = Anchor.TopLeft;
                    switch (obj.ColumnDefinitions[c].Alignment)
                    {
                        case TableColumnAlign.Right:
                            anchor = Anchor.TopRight;
                            break;
                        case TableColumnAlign.Center:
                            anchor = Anchor.TopCentre;
                            break;
                    }

                    var cell = renderCell((TableCell)tableRow[c]);
                    cell.TextAnchor = anchor;

                    if (obj.ColumnDefinitions[c].Width != 0 && obj.ColumnDefinitions[c].Width != 1)
                    {
                        cell.AutoSizeAxes = Axes.Both;
                        cell.MaximumSize = new Vector2(obj.ColumnDefinitions[c].Width, float.MaxValue);
                    }
                    else
                    {
                        cell.AutoSizeAxes = Axes.Both;
                        // Todo: How?
                    }

                    cells[r][c] = cell;
                }
            }

            renderer.EnsureNewParagraph();
            renderer.Write(new GridContainer
            {
                AutoSizeAxes = Axes.Both,
                Content = cells,
                ColumnDimensions = colDimensions.ToArray(),
                RowDimensions = rowDimensions.ToArray()
            });
        }

        private TextFlowContainer renderCell(TableCell cell)
        {
            var renderer = new MarkdownRenderer();
            var document = (TextFlowContainer)renderer.Render(null);

            document.Padding = new MarginPadding { Horizontal = 4 };

            renderer.Write(cell);

            return document;
        }
    }
}
