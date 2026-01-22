using SimpleTextEditor.Text.Visualization.Element.Interface;

namespace SimpleTextEditor.Text.Visualization.Element
{
    public class TextEndOfLineElement : ITextSpan
    {
        public int SpanPosition { get; }

        public TextEndOfLineElement(int spanPosition)
        {
            this.SpanPosition = spanPosition;
        }
    }
}
