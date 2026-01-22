using System.Windows.Media.TextFormatting;

namespace SimpleTextEditor.Text.Visualization
{
    /// <summary>
    /// Data needed to represent a single visual line
    /// </summary>
    public class VisualLineData
    {
        public int StartOffset { get; }
        public int EndOffset { get; }
        public int LineNumber { get; }
        public IEnumerable<TextLine> Elements { get { return _elements; } }

        List<TextLine> _elements;

        public VisualLineData(int start, int end, int lineNumber)
        {
            this.StartOffset = start;
            this.EndOffset = end;
            this.LineNumber = lineNumber;
            _elements = new List<TextLine>();
        }

        public void AddElement(TextLine element)
        {
            _elements.Add(element);
        }

        public void ClearElements()
        {
            _elements.Clear();
        }
    }
}
