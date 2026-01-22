using System.Windows;

using SimpleTextEditor.Model;
using SimpleTextEditor.Model.Interface;

using SimpleWpf.Extensions.Collection;

namespace SimpleTextEditor.Text.Visualization
{
    /// <summary>
    /// Class for calculating source offset <--> ITextPosition <--> Point coordinates. These are extension methods for
    /// the SimpleTextVisualOutputData class.
    /// </summary>
    public static class VisualTextOffsetCalculator
    {
        public static ITextPosition GetStartOfLine(this VisualOutputData outputData, int visualLineNumber)
        {
            return outputData.VisualCollection.GetLineElements(visualLineNumber).First().Position;
        }

        public static ITextPosition GetEndOfLine(this VisualOutputData outputData, int visualLineNumber)
        {
            var lineLength = outputData.VisualCollection.GetLineElements(visualLineNumber).Sum(x => x.Length);
            var result = outputData.VisualCollection.GetLineElements(visualLineNumber).Last().Position;

            return new TextPosition(result.Offset + lineLength - 1, lineLength, visualLineNumber, result.ParagraphNumber);
        }

        public static ITextPosition GetLineOffset(this VisualOutputData outputData, int visualLineNumber, int offset)
        {
            var result = outputData.VisualCollection.GetLineElements(visualLineNumber).First().Position;

            return new TextPosition(offset, offset + 1, visualLineNumber, result.ParagraphNumber);
        }

        /// <summary>
        /// Returns the line height for a given visual line (this is MSFT's TextHeight, line height usually refers
        /// to a multiplier to the text height, so it's usually set to 1.0D.
        /// </summary>
        public static double GetVisualLineHeight(this VisualOutputData outputData, int visualLineNumber)
        {
            if (outputData.VisualCollection.LineCount == 0)
            {
                return outputData.VisualCollection.GetSpan(0).Count;
            }

            var visualElement = outputData.VisualCollection.GetVisualLine(visualLineNumber);

            if (visualElement == null)
                throw new Exception("Invalid visual line number");

            return visualElement.TextHeight;
        }

        /// <summary>
        /// Returns ITextPosition for the previous line. Adds the appropriate append position for the caret.
        /// </summary>
        public static ITextPosition GetLastLineAtColumn(this VisualOutputData outputData, ITextPosition position)
        {
            if (outputData.VisualCollection.LineCount == 0)
                return TextPosition.CreateEmpty();

            if (outputData.VisualCollection.LineCount == 1)
                return outputData.GetStartOfLine(position.VisualLineNumber);

            // Get previous line
            var lastLine = outputData.VisualCollection.GetLineElements(position.VisualLineNumber - 1).Last();

            // EOL
            if (lastLine.Position.Offset < position.Offset)
                return outputData.GetEndOfLine(lastLine.Position.VisualLineNumber);

            // EOL / Normal
            else
                return new TextPosition(lastLine.Position.Offset + position.VisualColumnNumber - 1,
                                        position.VisualColumnNumber,
                                        lastLine.Position.VisualLineNumber,
                                        lastLine.Position.ParagraphNumber);
        }

        /// <summary>
        /// Returns ITextPosition for the previous line. Adds the appropriate append position for the caret.
        /// </summary>
        public static ITextPosition GetNextLineAtColumn(this VisualOutputData outputData, ITextPosition position)
        {
            if (outputData.VisualCollection.LineCount == 0)
                return TextPosition.CreateEmpty();

            if (outputData.VisualCollection.LineCount == 1)
                return outputData.GetEndOfLine(position.VisualLineNumber);

            // Get previous line
            var nextLine = outputData.VisualCollection.GetLineElements(position.VisualLineNumber + 1).Last();

            // EOL
            if (nextLine.Position.Offset < position.Offset)
                return outputData.GetEndOfLine(nextLine.Position.VisualLineNumber);

            // EOL / Normal
            else
                return new TextPosition(nextLine.Position.Offset + position.VisualColumnNumber - 1,
                                        position.VisualColumnNumber,
                                        nextLine.Position.VisualLineNumber,
                                        nextLine.Position.ParagraphNumber);
        }

        /// <summary>
        /// Returns the closest ITextPosition for the provided point:  1) Pre-cursor would be above the
        /// text block, 2) Inside of the text block would return the closest glyph position, 3) After the
        /// text block would return the last glyph's position.
        /// </summary>
        public static ITextPosition VisualPointToTextPosition(this VisualOutputData outputData, Point point, out CaretPosition caretPosition)
        {
            if (outputData.VisualCollection.LineCount == 0)
                throw new Exception("SimpleTextEditorFormatter Measure must be called to initilaize output data first");

            caretPosition = CaretPosition.BeforeCharacter;

            // ENTIRE TEXT BOUNDS
            var lastRectangle = new Rect(outputData.ConstraintSize);

            if (point.Y <= lastRectangle.Top || point.X <= lastRectangle.Left)
                return outputData.GetStartOfLine(1);

            else if (point.Y >= lastRectangle.Bottom || point.X >= lastRectangle.Right)
            {
                caretPosition = CaretPosition.AfterCharacter;
                return outputData.GetEndOfLine(1);
            }


            // LINE BOUNDS
            else
            {
                // Containing Text Element
                var textElements = outputData.VisualCollection
                                            .SearchLines(x => x.VisualBounds.Top <= point.Y && x.VisualBounds.Bottom >= point.Y);

                // None -> Take closest line of text
                if (!textElements.Any())
                {
                    caretPosition = CaretPosition.AfterCharacter;
                    return outputData.GetEndOfLine(1);
                }


                // Use TextBounds property of TextLine to calculate position
                // 
                var index = textElements.SelectMany(x => x.CharacterBounds).IndexOf(x => x.Contains(point));

                if (index >= 0)
                {
                    caretPosition = CaretPosition.BeforeCharacter;
                    return outputData.GetLineOffset(textElements.First().Position.VisualLineNumber, index);
                }

                // Default:  Paragraph Append Position
                caretPosition = CaretPosition.AfterCharacter;
                return outputData.GetEndOfLine(textElements.First().Position.VisualLineNumber);
            }
        }

        /// <summary>
        /// Returns the top left point of the visual offset from the character offset. If append position is set, then
        /// the EOL position is taken, which is calculated from the line's text element(s).
        /// </summary>
        public static Point CharacterOffsetToVisualOffset(this VisualOutputData outputData, int characterOffset, CaretPosition caretPosition)
        {
            if (outputData.VisualCollection.LineCount == 0)
            {
                if (characterOffset > 0)
                    throw new ArgumentException("For empty text sources please use ITextPosition.IsEmpty pattern");

                return new Point(0, 0);
            }

            // Character -> ITextPosition
            var lineElements = outputData.VisualCollection.SearchLines(characterOffset);

            // Get ITextElement from collection
            var positionX = 0D;
            var positionY = 0D;

            // Prepend
            var stopIndex = caretPosition == CaretPosition.BeforeCharacter ?
                            characterOffset - lineElements.First().Position.Offset :
                            characterOffset - lineElements.First().Position.Offset + 1;

            foreach (var visualElement in lineElements)
            {
                for (int index = 0; index < visualElement.CharacterBounds.Length; index++)
                {
                    var offsetIndex = index + lineElements.First().Position.Offset;

                    if (offsetIndex < stopIndex)
                    {
                        positionX += visualElement.CharacterBounds[index].Width;
                        positionY = visualElement.CharacterBounds[index].Top;
                    }
                    else
                        break;
                }
            }

            return new Point(positionX, positionY);
        }
    }
}
