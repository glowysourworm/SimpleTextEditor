using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Media.TextFormatting;

using SimpleTextEditor.Text.Source.Interface;
using SimpleTextEditor.Text.Visualization;
using SimpleTextEditor.Text.Visualization.Element;
using SimpleTextEditor.Text.Visualization.Element.Interface;

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

        public Size DesiredSize { get { return _desiredSize; } }
        public Point VisualOffset { get { return _visualOffset; } }
        public int Offset { get { return _offset; } }
        public int LineOffset { get { return _lineOffset; } }
        public int LineIndex
        {
            get { return _lineCount == 0 ? 0 : _lineCount - 1; }
        }
        public int ParagraphIndex
        {
            get { return _paragraphCount == 0 ? 0 : _paragraphCount - 1; }
        }
        public double DefaultTextHeight { get; set; }

        public bool IsAddingParagraph { get { return _visualText.AddingParagraph(); } }
        public bool IsAddingLine { get { return _visualText.AddingLine(this.ParagraphIndex + 1); } }

        VisualTextCollection _visualText;
        int _offset;                            // NOTE*** Offset is actually the CURRENT offset, which implies the NEXT character (will overflow by one)
        int _lineOffset;
        int _lineCount;
        int _paragraphCount;
        Size _constraintSize;
        Size _desiredSize;
        Point _visualOffset;
        bool _trailingLine;

        public VisualOutputBuilder(ITextSource textSource, Size constraintSize)
        {
            _textSource = textSource;

            _visualText = new VisualTextCollection();
            _constraintSize = constraintSize;
            _desiredSize = new Size();
            _visualOffset = new Point();
            _offset = 0;
            _lineOffset = 0;
            _lineCount = 0;
            _trailingLine = false;
            _paragraphCount = 0;
            this.DefaultTextHeight = 0;
        }
        public void ProcessAddCharacters(TextLine visualElement, TextSpan<TextRun> span, ITextElement textElement)
        {
            // EOP / EOL Tracking
            BeginAddElement(span, textElement, false);

            // Visual Collection
            _visualText.Add(visualElement, textElement);

            // Offset
            _offset += textElement.Length;
        }
        public void ProcessEOL(TextSpan<TextRun> visualElement, ITextSpan textElement)
        {
            // EOP / EOL Tracking
            BeginAddElement(visualElement, textElement, false);

            // Visual Collection
            _visualText.Add(visualElement, new TextEndOfLineElement());

            // VisualCollection -> EndLine() (VALIDATE) (NOTE*** Offset is AHEAD BY ONE!)
            _visualText.EndLine(_offset - _lineOffset);

            // OFFSET (this is the '\r' character)
            _offset++;

            // LINE INCREMENTERS
            _lineOffset = _offset;
            _lineCount++;
        }
        public void ProcessEOP(TextSpan<TextRun> visualElement, ITextSpan textElement)
        {
            BeginAddElement(visualElement, textElement, true);

            // VisualCollection -> EndParagraph() (VALIDATE) (also validates current line which is not terminated with EOL!)
            var trailingLine = _visualText.EndParagraph(visualElement,
                                                        textElement,
                                                        _offset - _lineOffset,
                                                        _visualText.AddingLine(this.ParagraphIndex + 1));

            // NON-TERMINATED LINE
            _lineCount++;
            _trailingLine = trailingLine;

            // PARAGRAPH INCREMENTERS
            _paragraphCount++;
        }

        private void BeginAddElement(TextSpan<TextRun> visualElement, ITextSpan textElement, bool isEOP)
        {
            // VisualTextCollection -> BeginParagraph() -> BeginLine() -> Add Elements... -> EndLine() -> EndParagraph(...)
            //
            if (!_visualText.AddingParagraph())
            {
                _visualText.BeginParagraph();
            }
            if (!_visualText.AddingLine(_visualText.GetParagraphCount()) && !isEOP)
            {
                _visualText.BeginLine();
            }
        }


        /// <summary>
        /// Routine to run to complete the visual pass. Validates the output.
        /// </summary>
        public void Validate()
        {
            if (_offset < 0)
                throw new IndexOutOfRangeException("Offset not intiialized correctly");

            if (_lineOffset < 0)
                throw new IndexOutOfRangeException("Line offset not initialized properly");

            if (_visualText.AddingLine(_paragraphCount) ||
                _visualText.AddingParagraph())
                throw new ValidationException("VisualCollection not properly closed! Check EOP / EOL issues!");

            // EMPTY TEXT CASE
            if (_visualText.GetCharCount(_paragraphCount) == 0)
            {
                if (_paragraphCount != 1)
                    throw new ValidationException("Paragraph count should be 1 for empty text case!");

                if (_lineCount != 1)
                    throw new ValidationException("Line count should be 1 for empty text case!");

                if (_offset != 0)
                    throw new ValidationException("Invalid char count for empty text case!");

                if (_trailingLine)
                    throw new ValidationException("Invalid trailing line for empty text case!");

                if (_visualText.GetParagraphCount() != 1)
                    throw new ValidationException("Invalid paragraph count:  VisualOutputBuilder");

                if (_visualText.GetLineCount(_paragraphCount) != 0)
                    throw new ValidationException("Invalid line count:  VisualOutputBuilder");
            }

            // EXPECTED CHAR COUNT
            else
            {
                if (_visualText.GetParagraphCount() != _paragraphCount)
                    throw new ValidationException("Invalid paragraph count:  VisualOutputBuilder");

                if (_visualText.GetLineCount(_paragraphCount) != _lineCount)
                    throw new ValidationException("Invalid line count:  VisualOutputBuilder");

                // Trailing line is not terminated in an EOL character. The ITextSource must follow
                // the offset accurately. (NOTE*** Offset for this builder is ahead by one!)
                //
                var expectedCharCount = _trailingLine ? _offset - 1 : _offset;

                if (_visualText.GetCharCount(_paragraphCount) != _offset)
                    throw new ValidationException("Invalid char count:  VisualOutputBuilder");
            }

        }

        public VisualOutputData BuildOutput()
        {
            Validate();

            return new VisualOutputData(_visualText,
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
