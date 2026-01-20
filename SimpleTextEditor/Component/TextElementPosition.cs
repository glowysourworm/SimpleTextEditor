using System.Windows;

using SimpleTextEditor.Component.Interface;

namespace SimpleTextEditor.Component
{
    public class TextElementPosition : ITextElementPosition
    {
        public Rect VisualBounds { get; private set; }
        public int SourceOffset { get; private set; }
        public int SourceLineNumber { get; private set; }
        public int VisualColumn { get; private set; }
        public int VisualLineNumber { get; private set; }
        public int ParagraphNumber { get; private set; }

        public TextElementPosition()
        {

        }
        public TextElementPosition(Rect visualBounds, int sourceOffset, int sourceLineNumber, int visualColumn, int visualLineNumber, int paragraphNumber)
        {
            Update(visualBounds, sourceOffset, sourceLineNumber, visualColumn, visualLineNumber, paragraphNumber);
        }

        public void Update(Rect visualBounds, int sourceOffset, int sourceLineNumber, int visualColumn, int visualLineNumber, int paragraphNumber)
        {
            this.VisualBounds = visualBounds;
            this.SourceOffset = sourceOffset;
            this.SourceLineNumber = sourceLineNumber;
            this.VisualColumn = visualColumn;
            this.VisualLineNumber = visualLineNumber;
            this.ParagraphNumber = paragraphNumber;
        }
    }
}
