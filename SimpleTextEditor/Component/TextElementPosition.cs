using System.Windows;

using SimpleTextEditor.Component.Interface;

namespace SimpleTextEditor.Component
{
    public class TextElementPosition : ITextElementPosition
    {
        public Point VisualPosition { get; }
        public int SourceOffset { get; }
        public int SourceLineNumber { get; }
        public int VisualColumn { get; }
        public int VisualLineNumber { get; }
        public int ParagraphNumber { get; }

        public TextElementPosition()
        {

        }
        public TextElementPosition(Point visualPosition, int sourceOffset, int sourceLineNumber, int visualColumn, int visualLineNumber, int paragraphNumber)
        {
            this.VisualPosition = visualPosition;
            this.SourceOffset = sourceOffset;
            this.SourceLineNumber = sourceLineNumber;
            this.VisualColumn = visualColumn;
            this.VisualLineNumber = visualLineNumber;
            this.ParagraphNumber = paragraphNumber;
        }
    }
}
