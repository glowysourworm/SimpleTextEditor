using System.Windows;

namespace SimpleTextEditor.Text.Visualization
{
    public class SimpleTextVisualOutputData
    {
        // Visual lines for the visualization
        Dictionary<int, SimpleTextLine> _visualLines;

        /// <summary>
        /// Resulting visual lines
        /// </summary>
        public IEnumerable<SimpleTextLine> VisualLines { get { return _visualLines.Values; } }

        /// <summary>
        /// Render size constraint from the control
        /// </summary>
        public Size ConstraintSize { get; set; }

        /// <summary>
        /// Desired render size via MSFT TextFormatting (see TextFormatter.FormatLine(...))
        /// </summary>
        public Size DesiredSize { get; set; }

        /// <summary>
        /// Desired render bounds for the caret
        /// </summary>
        public Rect CaretBounds { get; set; }

        public SimpleTextVisualOutputData(IEnumerable<SimpleTextLine> visualLines, Size constraint, Size desiredSize, Rect caretBounds)
        {
            this.ConstraintSize = constraint;
            this.DesiredSize = desiredSize;
            this.CaretBounds = caretBounds;

            _visualLines = new Dictionary<int, SimpleTextLine>();

            foreach (var line in visualLines)
                _visualLines.Add(line.Position.VisualLineNumber, line);
        }

        public SimpleTextLine GetLine(int visualLineNumber)
        {
            return _visualLines[visualLineNumber];
        }

        public SimpleTextParagraphProperties GetParagraphProperties(int visualLineNumber)
        {
            return _visualLines[visualLineNumber].Properties;
        }
    }
}
