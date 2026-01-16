using System.Windows;

namespace SimpleTextEditor.Text.Visualization
{
    public class SimpleTextVisualOutputData
    {
        // Visual lines for the visualization
        List<SimpleTextElement> _visualElements;

        /// <summary>
        /// Render size constraint from the control
        /// </summary>
        public Size ConstraintSize { get; }

        /// <summary>
        /// Desired render size via MSFT TextFormatting (see TextFormatter.FormatLine(...))
        /// </summary>
        public Size DesiredSize { get; }

        /// <summary>
        /// Desired render bounds for the caret
        /// </summary>
        public Rect CaretBounds { get; }

        /// <summary>
        /// Length (in characters) of text output
        /// </summary>
        public int SourceLength { get; }

        /// <summary>
        /// Number of text elements in the output data
        /// </summary>
        public int ElementCount { get; }

        /// <summary>
        /// Returns collection of visual elements
        /// </summary>
        public IEnumerable<SimpleTextElement> VisualElements { get { return _visualElements; } }

        public SimpleTextVisualOutputData(IEnumerable<SimpleTextElement> visualElements, Size constraint, Size desiredSize, Rect caretBounds, int sourceLength)
        {
            this.ConstraintSize = constraint;
            this.DesiredSize = desiredSize;
            this.CaretBounds = caretBounds;
            this.SourceLength = sourceLength;

            _visualElements = new List<SimpleTextElement>(visualElements);

        }

        public SimpleTextElement GetElement(int index)
        {
            return _visualElements[index];
        }
    }
}
