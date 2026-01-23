using System.Windows;
using System.Windows.Media.TextFormatting;

using SimpleTextEditor.Text.Source.Interface;
using SimpleTextEditor.Text.Visualization;

namespace SimpleTextEditor.Text.Formatting
{
    /// <summary>
    /// Helper class for the ITextFormatter. This should be used as part of the in-line 
    /// text formatting process; and keeps tabs on all the incrementers and builds a 
    /// VisualTextCollection as its output.
    /// </summary>
    public class VisualOutputBuilder
    {
        readonly ITextSource _textSource;

        public VisualTextCollection VisualText { get; set; }
        public Size DesiredSize { get { return _desiredSize; } }
        public Point VisualOffset { get { return _visualOffset; } }
        public int BackendOffset { get; set; }
        public int Offset { get; set; }
        public int LineOffset { get; set; }
        public int LineIndex { get; set; }
        public int ParagraphIndex { get; set; }
        public double DefaultTextHeight { get; set; }

        Size _constraintSize;
        Size _desiredSize;
        Point _visualOffset;

        public VisualOutputBuilder(ITextSource textSource, Size constraintSize)
        {
            _textSource = textSource;

            this.VisualText = new VisualTextCollection();
            _constraintSize = constraintSize;
            _desiredSize = new Size();
            _visualOffset = new Point();
            this.BackendOffset = 0;
            this.Offset = 0;
            this.LineOffset = 0;
            this.LineIndex = 0;
            this.ParagraphIndex = 0;
            this.DefaultTextHeight = 0;
        }

        /// <summary>
        /// Routine to run to complete the visual pass. Validates the output.
        /// </summary>
        public void Validate()
        {
            if (this.VisualText.AddingLine(this.ParagraphIndex + 1) ||
                this.VisualText.AddingParagraph())
                throw new Exception("VisualCollection not properly closed! Check EOP / EOL issues!");
        }

        public VisualOutputData BuildOutput()
        {
            Validate();

            return new VisualOutputData(this.VisualText,
                                        _constraintSize,
                                        this.DesiredSize,
                                        _textSource.GetLength(),
                                        this.DefaultTextHeight);
        }

        public void UpdateVisualSize(TextLine textElement)
        {
            // Default Text Height (works for empty text source)
            if (this.DefaultTextHeight == 0)
                this.DefaultTextHeight = textElement.TextHeight;

            // Text Visual Y-Position
            _visualOffset.Y = (this.LineIndex) * textElement.TextHeight;

            // Update Desired Size
            _desiredSize.Height = _visualOffset.Y;
            _desiredSize.Width = Math.Max(_desiredSize.Width, textElement.WidthIncludingTrailingWhitespace);
        }
    }
}
