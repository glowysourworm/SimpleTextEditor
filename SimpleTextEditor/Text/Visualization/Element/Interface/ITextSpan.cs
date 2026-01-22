namespace SimpleTextEditor.Text.Visualization.Element.Interface
{
    public interface ITextSpan
    {
        /// <summary>
        /// This is a special position for the TextLine (MSFT) output element. It is not used for
        /// our ITextSource!
        /// </summary>
        int SpanPosition { get; }
    }
}