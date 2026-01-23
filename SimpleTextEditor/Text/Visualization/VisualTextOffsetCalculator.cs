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
        /// <summary>
        /// Returns the line height for a given visual line (this is MSFT's TextHeight, line height usually refers
        /// to a multiplier to the text height, so it's usually set to 1.0D.
        /// </summary>
        public static double GetVisualLineHeight(this VisualOutputData outputData, int visualParagraphNumber, int visualLineNumber)
        {
            if (outputData.VisualCollection.GetLineCount(visualParagraphNumber) == 0)
            {
                throw new Exception("Cannot calculate line height before there is visual output data");
            }

            var visualElement = outputData.VisualCollection.GetVisualLine(visualParagraphNumber, visualLineNumber);

            if (visualElement == null)
                throw new Exception("Invalid visual line number");

            return visualElement.Elements.First().VisualBounds.Height;
        }

        /// <summary>
        /// Returns ITextPosition for the previous line. Adds the appropriate append position for the caret.
        /// </summary>
        public static ITextPosition GetPreviousLineAtColumn(this VisualOutputData outputData, ITextPosition position)
        {
            if (outputData.VisualCollection.GetLineCount(position.ParagraphNumber) == 0)
                return TextPosition.CreateEmpty();

            if (outputData.VisualCollection.GetLineCount(position.ParagraphNumber) == 1)
                return outputData.VisualCollection.GetVisualLine(position.ParagraphNumber, position.VisualLineNumber).GetStartPosition();

            if (position.VisualLineNumber == 1)
                return outputData.VisualCollection.GetVisualLineForOffset(position.Offset).GetStartPosition();

            // Get previous line
            var currentLine = outputData.VisualCollection.GetVisualLine(position.ParagraphNumber, position.VisualLineNumber);
            var lastLine = outputData.VisualCollection.GetVisualLine(position.ParagraphNumber, position.VisualLineNumber - 1);

            // EOL
            if (lastLine.GetEndPosition().Offset < position.Offset)
                return lastLine.GetEndPosition();

            // EOL / Normal
            else
                return lastLine.GetPosition(lastLine.GetStartPosition().Offset + (position.Offset - currentLine.StartOffset));
        }

        /// <summary>
        /// Returns ITextPosition for the previous line. Adds the appropriate append position for the caret.
        /// </summary>
        public static ITextPosition GetNextLineAtColumn(this VisualOutputData outputData, ITextPosition position)
        {
            if (outputData.VisualCollection.GetLineCount(position.ParagraphNumber) == 0)
                return TextPosition.CreateEmpty();

            if (outputData.VisualCollection.GetLineCount(position.ParagraphNumber) == 1)
                return outputData.VisualCollection.GetVisualLine(position.ParagraphNumber, position.VisualLineNumber).GetStartPosition();

            if (outputData.VisualCollection.GetLineCount(position.ParagraphNumber) == position.VisualLineNumber)
                return outputData.VisualCollection.GetVisualLineForOffset(position.Offset).GetEndPosition();

            // Get previous line
            var currentLine = outputData.VisualCollection.GetVisualLine(position.ParagraphNumber, position.VisualLineNumber);
            var nextLine = outputData.VisualCollection.GetVisualLine(position.ParagraphNumber, position.VisualLineNumber + 1);

            // EOL
            if (nextLine.GetEndPosition().Offset < position.Offset)
                return nextLine.GetEndPosition();

            // EOL / Normal
            else
                return nextLine.GetPosition(nextLine.GetStartPosition().Offset + (position.Offset - currentLine.StartOffset));
        }

        /// <summary>
        /// Returns the closest ITextPosition for the provided point:  1) Pre-cursor would be above the
        /// text block, 2) Inside of the text block would return the closest glyph position, 3) After the
        /// text block would return the last glyph's position.
        /// </summary>
        public static ITextPosition VisualPointToTextPosition(this VisualOutputData outputData, Point point, out CaretPosition caretPosition)
        {
            if (outputData.VisualCollection.GetParagraphCount() == 0)
                throw new Exception("SimpleTextEditorFormatter Measure must be called to initilaize output data first");

            caretPosition = CaretPosition.BeforeCharacter;

            // ENTIRE TEXT BOUNDS
            var lastRectangle = new Rect(outputData.ConstraintSize);

            if (point.Y <= lastRectangle.Top || point.X <= lastRectangle.Left)
                return outputData.VisualCollection.GetFirstVisualLine().GetStartPosition();

            else if (point.Y >= lastRectangle.Bottom || point.X >= lastRectangle.Right)
            {
                caretPosition = CaretPosition.AfterCharacter;
                return outputData.VisualCollection.GetLastVisualLine().GetEndPosition();
            }


            // LINE BOUNDS
            else
            {
                // Containing Text Element
                var visualLine = outputData.VisualCollection
                                           .FirstLineWhereAnyElement(x => x.VisualBounds.Top <= point.Y && x.VisualBounds.Bottom >= point.Y);

                // None -> Take closest line of text
                if (visualLine == null)
                {
                    caretPosition = CaretPosition.AfterCharacter;
                    return outputData.VisualCollection.GetFirstVisualLine().GetEndPosition();
                }


                // Use TextBounds property of TextLine to calculate position
                // 
                var index = visualLine.Elements.SelectMany(x => x.CharacterBounds).IndexOf(x => x.Contains(point));

                if (index >= 0)
                {
                    caretPosition = CaretPosition.BeforeCharacter;
                    return visualLine.GetPosition(visualLine.StartOffset + index);
                }

                // Default:  Paragraph Append Position
                caretPosition = CaretPosition.AfterCharacter;
                return visualLine.GetEndPosition();
            }
        }

        /// <summary>
        /// Returns the top left point of the visual offset from the character offset. If append position is set, then
        /// the EOL position is taken, which is calculated from the line's text element(s).
        /// </summary>
        public static Point CharacterOffsetToVisualOffset(this VisualOutputData outputData, int characterOffset, CaretPosition caretPosition)
        {
            if (outputData.VisualCollection.GetParagraphCount() == 0)
            {
                if (characterOffset > 0)
                    throw new ArgumentException("For empty text sources please use ITextPosition.IsEmpty pattern");

                return new Point(0, 0);
            }

            // Character -> ITextPosition
            var visualLine = outputData.VisualCollection.GetVisualLineForOffset(characterOffset);

            if (visualLine == null)
                throw new Exception("No line elements found for offset " + characterOffset);

            // Get ITextElement from collection
            var positionX = 0D;
            var positionY = 0D;

            // Prepend
            var stopIndex = caretPosition == CaretPosition.BeforeCharacter ?
                            characterOffset - visualLine.StartOffset :
                            characterOffset - visualLine.StartOffset + 1;

            foreach (var visualElement in visualLine.Elements)
            {
                for (int index = 0; index < visualElement.CharacterBounds.Length; index++)
                {
                    var offsetIndex = index + visualLine.StartOffset;

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
