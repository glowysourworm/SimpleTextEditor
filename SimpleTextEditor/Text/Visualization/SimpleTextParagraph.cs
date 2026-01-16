namespace SimpleTextEditor.Text.Visualization
{
    /// <summary>
    /// Container for a breakdown of SimpleTextLine(s) by paragraph. The text line is truncated
    /// visually, but there are available data for both visual and source representations of the
    /// line.
    /// </summary>
    public class SimpleTextParagraph
    {
        // Dictionary of lines for this paragraph (by line number)
        Dictionary<int, SimpleTextElement> _lines;

        // Paragraph number
        int _paragraphNumber;


        public SimpleTextParagraph(int paragraphNumber)
        {
            _paragraphNumber = paragraphNumber;
            _lines = new Dictionary<int, SimpleTextElement>();
        }

        public void AddLine(SimpleTextElement line)
        {
            if (line.Position.ParagraphNumber != _paragraphNumber)
                throw new ArgumentException("Mismatching paragraph numbers!");

            _lines.Add(line.Position.VisualLineNumber, line);
        }
        public SimpleTextElement GetLine(int visualLineNumber)
        {
            return _lines[visualLineNumber];
        }
    }
}
