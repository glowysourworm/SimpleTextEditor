using System.Windows;

using SimpleWpf.Extensions;

namespace SimpleTextEditor.Text.Visualization
{
    public class VisualOutputData
    {
        /// <summary>
        /// Render size constraint from the control
        /// </summary>
        public Size ConstraintSize { get; }

        /// <summary>
        /// Desired render size via MSFT TextFormatting (see TextFormatter.FormatLine(...))
        /// </summary>
        public Size DesiredSize { get; }

        /// <summary>
        /// Length (in characters) of text output
        /// </summary>
        public int SourceLength { get; }

        /// <summary>
        /// Number of text elements in the output data
        /// </summary>
        public int ElementCount { get; }

        /// <summary>
        /// This is returned by any line of text by the formatter, even for an empty line, and used
        /// for caret calculations.
        /// </summary>
        public double DefaultTextHeight { get; }

        /// <summary>
        /// Returns collection of visual elements
        /// </summary>
        public VisualTextCollection VisualCollection { get; }

        public VisualOutputData(VisualTextCollection visualCollection, Size constraint, Size desiredSize, int sourceLength, double defaultTextHeight)
        {
            this.DefaultTextHeight = defaultTextHeight;
            this.ConstraintSize = constraint;
            this.DesiredSize = desiredSize;
            this.SourceLength = sourceLength;
            this.VisualCollection = visualCollection;
        }

        public override string ToString()
        {
            return this.FormatToString();
        }
    }
}
