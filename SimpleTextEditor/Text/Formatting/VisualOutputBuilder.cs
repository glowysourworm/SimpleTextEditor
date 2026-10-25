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
        bool _trailingLine;

        Size _constraintSize;
        Size _desiredSize;
        Point _visualOffset;


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
            BeginAddElement(visualElement, span, textElement);

            // Visual Collection
            _visualText.Add(visualElement, textElement);

            // Offset
            _offset += textElement.Length;
        }
        public void ProcessEOL(TextLine visualElement, TextSpan<TextRun> span, ITextSpan textElement)
        {
            // TODO: VERIFY HOW THIS EOL WOULD APPEAR
            if (!_visualText.AddingParagraph())
                throw new Exception("Trying to process end of line before beginning paragraph");

            // Update Offsets
            BeginAddElement(visualElement, span, textElement);

            // Visual Collection
            _visualText.Add(span, new TextEndOfLineElement());

            // VisualCollection -> EndLine() (VALIDATE) (NOTE*** Offset is AHEAD BY ONE!)
            _visualText.EndLine(_offset - _lineOffset);

            // OFFSET (this is the '\r' character)
            _offset++;

            // LINE INCREMENTERS
            _lineOffset = _offset;
            _lineCount++;
        }
        public void ProcessEOP(TextLine visualElement, TextSpan<TextRun> span, ITextSpan textElement)
        {
            // Update Offsets
            BeginAddElement(visualElement, span, textElement);

            // VisualCollection -> EndParagraph() (VALIDATE) (also validates current line which is not terminated with EOL!)
            var trailingLine = _visualText.EndParagraph(span,
                                                        textElement,
                                                        _offset - _lineOffset,
                                                        _visualText.AddingLine(this.ParagraphIndex + 1));

            // NON-TERMINATED LINE
            _lineCount++;
            _trailingLine = trailingLine;

            // PARAGRAPH INCREMENTERS
            _paragraphCount++;
        }

        private void BeginAddElement(TextLine visualElement, TextSpan<TextRun> span, ITextSpan textElement)
        {
            // Visual Boundary:  Update to track addition of lines; and use the VisualTextCollection to 
            //                   get updated paragraph and line bounds.
            //

            // Default Text Height (works for empty text source)
            if (this.DefaultTextHeight == 0)
                this.DefaultTextHeight = visualElement.TextHeight;

            // VisualTextCollection -> BeginParagraph() -> BeginLine() -> Add Elements... -> EndLine() -> EndParagraph(...)
            //
            if (!_visualText.AddingParagraph())
            {
                _visualText.BeginParagraph();
            }
            if (!_visualText.AddingLine(_visualText.GetParagraphCount()))
            {
                _visualText.BeginLine(_visualOffset, _offset);
            }

            // Text Visual Y-Position
            _visualOffset.Y = (this.LineIndex) * visualElement.TextHeight;

            // Update Desired Size
            var boundary = _visualText.GetBoundary();

            _desiredSize.Height = boundary == Rect.Empty ? 0 : boundary.Height;
            _desiredSize.Width = boundary == Rect.Empty ? 0 : boundary.Width;
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

                //if (_trailingLine)
                //    throw new ValidationException("Invalid trailing line for empty text case!");

                if (_visualText.GetParagraphCount() != 1)
                    throw new ValidationException("Invalid paragraph count:  VisualOutputBuilder");

                if (_visualText.GetLineCount(_paragraphCount) != 0)
                {
                    for (int index = 0; index < _visualText.GetLineCount(_paragraphCount); index++)
                    {
                        if (!_visualText.GetVisualLine(_paragraphCount, index + 1).IsEmpty())
                            throw new ValidationException("Invalid Lines:  These must all be empty lines");
                    }
                }
            }

            // EXPECTED CHAR COUNT
            else
            {
                if (_visualText.GetParagraphCount() != _paragraphCount)
                    throw new ValidationException("Invalid paragraph count:  VisualOutputBuilder");

                //if (_visualText.GetLineCount(_paragraphCount) != _lineCount)
                //    throw new ValidationException("Invalid line count:  VisualOutputBuilder");

                // Trailing line is not terminated in an EOL character. The ITextSource must follow
                // the offset accurately. (NOTE*** Offset for this builder is ahead by one!)
                //
                //var expectedCharCount = _trailingLine ? _offset - 1 : _offset;

                //if (_visualText.GetCharCount(_paragraphCount) != _offset)
                //    throw new ValidationException("Invalid char count:  VisualOutputBuilder");
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
    }
}
