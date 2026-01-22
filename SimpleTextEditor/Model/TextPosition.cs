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
        public int LineOffset { get; }
        public int LineFirstOffset { get; }
        public int ParagraphNumber { get; }
        public int VisualElementIndex { get; }
        public int VisualColumnNumber { get; }
        public int VisualLineNumber { get; }
        public AppendPosition AppendPosition { get; }

        public TextPosition()
        {

        }
        public TextPosition(int sourceOffset,
                            int lineOffset,
                            int lineFirstOffset,
                            int visualColumnNumber,
                            int visualLineNumber,
                            int paragraphNumber,
                            int elementIndex,
                            AppendPosition appendPosition)
        {
            this.Offset = sourceOffset;
            this.LineOffset = lineOffset;
            this.LineFirstOffset = lineFirstOffset;
            this.VisualColumnNumber = visualColumnNumber;
            this.VisualLineNumber = visualLineNumber;
            this.ParagraphNumber = paragraphNumber;
            this.VisualElementIndex = elementIndex;
            this.AppendPosition = appendPosition;
        }

        /// <summary>
        /// Constructs new text position from a line, using the provided append position (which must be valid!), and returns 
        /// a character text position.
        /// </summary>
        /// <param name="linePosition">Line's text position</param>
        /// <param name="characterOffset">Desired character offset</param>
        public static TextPosition FromLine(ITextPosition linePosition, int characterOffset, int visualColumnNumber)
        {
            return new TextPosition(characterOffset,                    // Offsets
                                    linePosition.LineOffset,
                                    linePosition.LineFirstOffset,
                                    visualColumnNumber,                 // Numbers
                                    linePosition.VisualLineNumber,
                                    linePosition.ParagraphNumber,
                                    linePosition.VisualElementIndex,    // Line's visual element index
                                    linePosition.AppendPosition);       // Append Position
        }

        public ITextPosition AsAppend(AppendPosition appendPosition, bool modifyOffset)
        {
            var offsetChange = modifyOffset ? CalculateAppendPositionModification(this, appendPosition) : 0;
            var offset = modifyOffset ? this.Offset + offsetChange : this.Offset;

            return new TextPosition(offset,
                                    this.LineOffset,
                                    this.LineFirstOffset,
                                    this.VisualColumnNumber,
                                    this.VisualLineNumber,
                                    this.ParagraphNumber,
                                    this.VisualElementIndex,
                                    appendPosition);
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
