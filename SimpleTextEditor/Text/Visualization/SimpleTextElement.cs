using System.Windows;
using System.Windows.Media.TextFormatting;

using SimpleTextEditor.Model.Interface;
using SimpleTextEditor.Text.Interface;

namespace SimpleTextEditor.Text.Visualization
{
    /// <summary>
    /// Container for MSFT (abstract) TextLine, which carries data linking the visualization of
    /// the text source, and the actual text source.
    /// </summary>
    public class SimpleTextElement : ITextElement
    {
        public ITextPosition Start { get; }
        public ITextPosition End { get; }
        public TextLine Element { get; }
        public Rect VisualBounds { get; }
        public int Length { get; }

        public SimpleTextElement(TextLine textElement, Rect visualBounds, ITextPosition startPosition, ITextPosition endPosition)
        {
            this.Start = startPosition;
            this.End = endPosition;
            this.VisualBounds = visualBounds;
            this.Element = textElement;
            this.Length = this.End.SourceOffset - this.Start.SourceOffset + 1;
        }

        public bool Contains(ITextPosition position)
        {
            return this.Start.SourceOffset <= position.SourceOffset && this.End.SourceOffset >= position.SourceOffset;
        }

        public bool Contains(int offset)
        {
            return this.Start.SourceOffset <= offset && this.End.SourceOffset >= offset;
        }
    }
}
