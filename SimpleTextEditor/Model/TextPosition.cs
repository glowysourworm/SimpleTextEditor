using SimpleTextEditor.Model.Interface;

namespace SimpleTextEditor.Model
{
    /// <summary>
    /// Represents a position of text for both source and visual representations
    /// </summary>
    public class TextPosition : ITextPosition
    {
        public int ElementIndex { get; }
        public int SourceOffset { get; }
        public int SourceLineNumber { get; }
        public int VisualColumn { get; }
        public int VisualLineNumber { get; }
        public int ParagraphNumber { get; }
        public bool IsAtEndOfLine { get; }

        public TextPosition()
        {

        }
        public TextPosition(int sourceOffset, int elementIndex, bool isAtEndOfLine, int sourceLineNumber, int visualColumn, int visualLineNumber, int paragraphNumber)
        {
            this.ElementIndex = elementIndex;
            this.SourceOffset = sourceOffset;
            this.IsAtEndOfLine = isAtEndOfLine;
            this.SourceLineNumber = sourceLineNumber;
            this.VisualColumn = visualColumn;
            this.VisualLineNumber = visualLineNumber;
            this.ParagraphNumber = paragraphNumber;
        }
    }
}
