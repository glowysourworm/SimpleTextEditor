using System.Windows;

using SimpleTextEditor.Model;
using SimpleTextEditor.Model.Interface;

namespace SimpleTextEditor.Text.Visualization
{
    /// <summary>
    /// Class for calculating source offset <--> ITextPosition <--> Point coordinates. These are extension methods for
    /// the SimpleTextVisualOutputData class.
    /// </summary>
    public static class VisualTextOffsetCalculator
    {
        /// <summary>
        /// Method that creates an ITextPosition for the requested source offset. The append position is handled
        /// to supply caret tracking and input tracking.
        /// </summary>
        /// <param name="outputData">Most recent output data</param>
        /// <param name="characterOffset">ITextSource offset</param>
        /// <param name="appendPosition">Is this append position for the caret? (see AppendPosition)</param>
        /// <returns>ITextPosition instance for the requested operation</returns>
        /// <exception cref="ArgumentOutOfRangeException">Supplied inputs cause an out of range exception</exception>
        /// <exception cref="Exception">Operation failed to calculate the ITextPosition</exception>
        public static ITextPosition CharacterOffsetToTextPosition(this VisualOutputData outputData,
                                                                  int characterOffset,
                                                                  AppendPosition appendPosition)
        {
            if (characterOffset < 0 || (characterOffset > outputData.SourceLength))
                throw new ArgumentOutOfRangeException("Please see AppendPosition documentation");

            switch (appendPosition)
            {
                case AppendPosition.None:
                case AppendPosition.EndOfLine:
                {
                    if (characterOffset < 0 || characterOffset >= outputData.SourceLength)
                        throw new ArgumentOutOfRangeException();

                    foreach (var visualElement in outputData.VisualElements)
                    {
                        if (visualElement.Contains(characterOffset))
                        {
                            // Return the offset for the requested character
                            if (appendPosition == AppendPosition.EndOfLine)
                                return visualElement.End.AsAppend(appendPosition, false);

                            // AppendPosition.None
                            else
                                return TextPosition.FromLine(visualElement.End,
                                                             characterOffset,
                                                             visualElement.End.VisualColumnNumber)
                                                   .AsAppend(appendPosition, false);
                        }
                    }

                    throw new Exception("Character offset not found!");
                }
                case AppendPosition.Append:
                {
                    if (characterOffset < 0 || characterOffset > outputData.SourceLength)
                        throw new ArgumentOutOfRangeException();

                    return outputData.VisualElements.Last().End.AsAppend(appendPosition, true);
                }
                default:
                    throw new Exception("Unhandled AppendPosition");
            }
        }

        /// <summary>
        /// Returns the line height for a given visual line (this is MSFT's TextHeight, line height usually refers
        /// to a multiplier to the text height, so it's usually set to 1.0D.
        /// </summary>
        public static double GetVisualLineHeight(this VisualOutputData outputData, int visualLineNumber)
        {
            var visualElement = outputData.VisualElements
                                          .FirstOrDefault(x => x.Start.VisualLineNumber == visualLineNumber);

            if (visualElement == null)
                throw new Exception("Invalid visual line number");

            return visualElement.Element.TextHeight;
        }

        /// <summary>
        /// Returns the closest ITextPosition for the provided point:  1) Pre-cursor would be above the
        /// text block, 2) Inside of the text block would return the closest glyph position, 3) After the
        /// text block would return the last glyph's position.
        /// </summary>
        public static ITextPosition VisualPointToTextPosition(this VisualOutputData outputData, Point point)
        {
            if (!outputData.VisualElements.Any())
                throw new Exception("SimpleTextEditorFormatter Measure must be called to initilaize output data first");

            // ENTIRE TEXT BOUNDS
            var lastRectangle = new Rect(outputData.ConstraintSize);

            if (point.Y <= lastRectangle.Top || point.X <= lastRectangle.Left)
                return outputData.VisualElements.First().Start;

            else if (point.Y >= lastRectangle.Bottom || point.X >= lastRectangle.Right)
                return outputData.VisualElements.Last().End.AsAppend(AppendPosition.Append, true);

            // LINE BOUNDS
            else
            {
                // Containing Text Element
                var textElement = outputData.VisualElements
                                            .FirstOrDefault(x => x.VisualBounds.Top <= point.Y && x.VisualBounds.Bottom >= point.Y);

                // None -> Take closest line of text
                if (textElement == null)
                    return outputData.VisualElements.Last().End.AsAppend(AppendPosition.EndOfLine, true);

                // Search glyph run to see where point lies
                var currentWidth = 0D;
                var currentIndex = 0;

                foreach (var glyphRun in textElement.Element.GetIndexedGlyphRuns())
                {
                    foreach (var advanceWidth in glyphRun.GlyphRun.AdvanceWidths)
                    {
                        // Found Text Position
                        if (textElement.VisualBounds.Left + currentWidth >= point.X)
                        {
                            var appendPosition = (textElement.Start.Offset + currentIndex) == textElement.End.Offset ?
                                                 AppendPosition.Append : AppendPosition.None;

                            var result = TextPosition.FromLine(textElement.Start,
                                                               textElement.Start.Offset + currentIndex,
                                                               textElement.Start.VisualLineNumber);

                            return result.AsAppend(appendPosition, true);
                        }

                        currentIndex++;
                        currentWidth += advanceWidth;
                    }
                }

                // Default:  Paragraph Append Position
                return textElement.End.AsAppend(AppendPosition.Append, true);
            }
        }

        /// <summary>
        /// Returns the top left point of the visual offset from the character offset. If append position is set, then
        /// the EOL position is taken, which is calculated from the line's text element(s).
        /// </summary>
        public static Point CharacterOffsetToVisualOffset(this VisualOutputData outputData, int characterOffset, AppendPosition appendPosition)
        {
            // Character -> ITextPosition
            var position = CharacterOffsetToTextPosition(outputData, characterOffset, appendPosition);

            // Get ITextElement from collection
            var visualElement = outputData.GetElement(position.VisualElementIndex);
            var positionX = 0D;

            // Get glyph run(s) for this element (there is typically only one actual glyph run per line)
            foreach (var glyphRun in visualElement.Element.GetIndexedGlyphRuns())
            {
                // Prepend
                if (glyphRun.TextSourceCharacterIndex < characterOffset)
                {
                    // Reutrn the offset for the requested character
                    for (int index = 0; index < glyphRun.GlyphRun.AdvanceWidths.Count &&
                                        index + visualElement.Start.Offset < characterOffset; index++)
                    {
                        positionX += glyphRun.GlyphRun.AdvanceWidths[index];
                    }
                }
            }

            switch (appendPosition)
            {
                // Character Position (Insert)
                case AppendPosition.None:
                    return new Point(positionX, visualElement.VisualBounds.Top);

                // Top-Right last character position
                case AppendPosition.EndOfLine:
                    return visualElement.VisualBounds.TopRight;

                // Append:  Last visual character of paragraph, Top-Right
                case AppendPosition.Append:
                    return outputData.VisualElements.Last().VisualBounds.TopRight;
                default:
                    throw new Exception("Unhandled AppendPosition");
            }
        }
    }
}
