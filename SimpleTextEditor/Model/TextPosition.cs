using SimpleTextEditor.Model.Interface;

using SimpleWpf.Extensions;

namespace SimpleTextEditor.Model
{
    /// <summary>
    /// Represents a position of text for both source and visual representations
    /// </summary>
    public class TextPosition : ITextPosition
    {
        public int Offset { get; }
        public int LineNumber { get; }
        public int ParagraphNumber { get; }
        public int VisualElementIndex { get; }
        public int VisualColumnNumber { get; }
        public int VisualLineNumber { get; }
        public AppendPosition AppendPosition { get; }

        public TextPosition()
        {

        }
        public TextPosition(int sourceOffset, int elementIndex, AppendPosition appendPosition, int sourceLineNumber, int visualColumn, int visualLineNumber, int paragraphNumber)
        {
            this.VisualElementIndex = elementIndex;
            this.Offset = sourceOffset;
            this.AppendPosition = appendPosition;
            this.LineNumber = sourceLineNumber;
            this.VisualColumnNumber = visualColumn;
            this.VisualLineNumber = visualLineNumber;
            this.ParagraphNumber = paragraphNumber;
        }

        /// <summary>
        /// Constructs new text position from a line, using the provided append position (which must be valid!), and returns 
        /// a character text position.
        /// </summary>
        /// <param name="linePosition">Line's text position</param>
        /// <param name="characterOffset">Desired character offset</param>
        public static TextPosition FromLine(ITextPosition linePosition, int characterOffset, int visualColumnNumber)
        {
            return new TextPosition(characterOffset,                    // Source offset
                                    linePosition.VisualElementIndex,    // Line's visual element index
                                    linePosition.AppendPosition,        // Append Position
                                    linePosition.LineNumber,            // Line number to source
                                    visualColumnNumber,                 // Visual Column Number (must be pre-calculated)
                                    linePosition.VisualLineNumber,      // Visual Line Number (shold follow line position)
                                    linePosition.ParagraphNumber);      // Paragraph Number (should follow line position)
        }

        public ITextPosition AsAppend(AppendPosition appendPosition, bool modifyOffset)
        {
            var offsetChange = modifyOffset ? CalculateAppendPositionModification(this, appendPosition) : 0;
            var offset = modifyOffset ? this.Offset + offsetChange : this.Offset;

            return new TextPosition(offset,
                                    this.VisualElementIndex,
                                    appendPosition,
                                    this.LineNumber,
                                    this.VisualColumnNumber,
                                    this.VisualLineNumber,
                                    this.ParagraphNumber);
        }

        private static int CalculateAppendPositionModification(ITextPosition position, AppendPosition appendPosition)
        {
            switch (appendPosition)
            {
                case AppendPosition.None:
                case AppendPosition.EndOfLine:
                {
                    if (position.AppendPosition == AppendPosition.Append)
                        return -1;
                }
                break;
                case AppendPosition.Append:
                {
                    if (position.AppendPosition != AppendPosition.Append)
                        return 1;
                }
                break;
                default:
                    throw new Exception("Unhandled AppendPosition in TextPosition");
            }

            return 0;
        }

        public override string ToString()
        {
            return this.FormatToString();
        }
    }
}
