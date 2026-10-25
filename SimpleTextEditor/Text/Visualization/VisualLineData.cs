using System.Windows.Media.TextFormatting;

using SimpleTextEditor.Model;
using SimpleTextEditor.Model.Interface;
using SimpleTextEditor.Text.Visualization.Element.Interface;

using SimpleWpf.Extensions;

namespace SimpleTextEditor.Text.Visualization
{
    /// <summary>
    /// Data needed to represent a single visual line
    /// </summary>
    public class VisualLineData
    {
        public int ParagraphNumber { get; }
        public int LineNumber { get; }
        public int StartOffset { get { return _startOffset; } }
        public int EndOffset { get { return _endOffset; } }
        public int Length { get { return IsEmpty() ? 0 : _endOffset - _startOffset + 1; } }
        public IEnumerable<TextLine> VisualElements { get { return _visualElements; } }
        public IEnumerable<ITextElement> Elements { get { return _elements; } }
        public IEnumerable<ITextSpan> Spans { get { return _spans; } }

        List<TextLine> _visualElements;
        List<ITextElement> _elements;
        List<ITextSpan> _spans;

        // Calculated as elements are added
        int _startOffset;
        int _endOffset;

        /// <summary>
        /// Creates a visual line:  The "empty" or "default" line ITextPosition is used to locate the beginning of a 
        /// new line. This is used for the caret position; and for UI measurement. An empty line will have no text elements
        /// or spans.
        /// </summary>
        /// <param name="paragraphNumber">Paragraph number for the line</param>
        /// <param name="lineNumber">Line number for the line</param>
        /// <param name="offset">Offset for the first character (or caret in the case of an empty line)</param>
        public VisualLineData(int paragraphNumber, int lineNumber, int offset)
        {
            this.ParagraphNumber = paragraphNumber;
            this.LineNumber = lineNumber;

            _visualElements = new List<TextLine>();
            _elements = new List<ITextElement>();
            _spans = new List<ITextSpan>();

            _startOffset = offset;                      // Beginning of line
            _endOffset = offset;
        }

        public void AddElement(TextLine element, ITextElement textElement)
        {
            // Start Offset
            if (IsEmpty())
            {
                _startOffset = textElement.Position.Offset;
                _endOffset = textElement.Position.Offset + textElement.Length - 1;
            }

            // End Offset (Update) (careful with multiple segments!)
            else
                _endOffset += textElement.Length - 1;

            _visualElements.Add(element);
            _elements.Add(textElement);
            _spans.Add(textElement);            // ITextElement is also a ITextSpan (but with non-zero length)

            Validate();
        }
        public void AddSpan(ITextSpan textSpan)
        {
            if (textSpan is ITextElement)
                throw new ArgumentException("Trying to add ITextSpan as ITextElement! Please use AddElement for this operation!");

            _spans.Add(textSpan);

            // NOTE***
            //
            // 1) Visual Element not used for spans
            // 2) Offsets only used with text elements (ITextElement)
            //

            Validate();
        }

        /// <summary>
        /// Returns text position for the requested offset, or null if it does not exist in 
        /// this visual line.
        /// </summary>
        public ITextPosition GetPosition(int offset)
        {
            if (!ContainsOffset(offset))
                throw new ArgumentException("Offset out of range");

            if (IsEmpty())
                return new TextPosition(_startOffset, 1, this.LineNumber, this.ParagraphNumber);

            return _elements.First().Position.WithOffset(offset);
        }

        public ITextPosition GetStartPosition()
        {
            if (IsEmpty())
                return new TextPosition(_startOffset, 1, this.LineNumber, this.ParagraphNumber);

            return _elements.First().Position;
        }

        public ITextPosition GetEndPosition()
        {
            if (IsEmpty())
                return new TextPosition(_startOffset, 1, this.LineNumber, this.ParagraphNumber);

            return _elements.First().Position.WithOffset(this.EndOffset);
        }

        public bool ContainsOffset(int offset)
        {
            // Empty Lines:  The length of the empty line is one character - which is held by the
            //               default offset
            //
            if (IsEmpty())
                return offset == _startOffset;

            if (offset < _startOffset ||
                offset > _endOffset)
                return false;

            return true;
        }

        /// <summary>
        /// Returns true if this line represents an empty line
        /// </summary>
        public bool IsEmpty()
        {
            return _startOffset == _endOffset;
        }

        private void Validate()
        {
            if (_elements.Count == 0)
                return;

            // Validate Length
            if (this.Length > _elements.Sum(x => x.Length))
                throw new ArgumentException("Invalid length of text element! Check text formatting!");

            //if (this.Length >= _elements.Max(x => x.SpanPosition))
            //    throw new ArgumentException("Invalid length of text element! Check text formatting!");
        }

        public override string ToString()
        {
            return this.FormatToString();
        }
    }
}
