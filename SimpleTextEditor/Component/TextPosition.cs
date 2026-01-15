using SimpleTextEditor.Component.Interface;

namespace SimpleTextEditor.Component
{
    /// <summary>
    /// Represents a position of text for both source and visual representations
    /// </summary>
    public class TextPosition : ITextPosition
    {
        public int SourceOffset { get; }
        public int SourceLineNumber { get; }
        public int VisualColumn { get; }
        public int VisualLineNumber { get; }
        public int ParagraphNumber { get; }

        public TextPosition()
        {

        }
        public TextPosition(int sourceOffset, int sourceLineNumber, int visualColumn, int visualLineNumber, int paragraphNumber)
        {
            this.SourceOffset = sourceOffset;
            this.SourceLineNumber = sourceLineNumber;
            this.VisualColumn = visualColumn;
            this.VisualLineNumber = visualLineNumber;
            this.ParagraphNumber = paragraphNumber;
        }
    }
}
