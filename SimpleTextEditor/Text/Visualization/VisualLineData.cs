using System.Windows.Media.TextFormatting;

using SimpleTextEditor.Model.Interface;
using SimpleTextEditor.Text.Visualization.Element.Interface;

namespace SimpleTextEditor.Text.Visualization
{
    /// <summary>
    /// Data needed to represent a single visual line
    /// </summary>
    public class VisualLineData
    {
        public int LineNumber { get; }
        public int StartOffset { get { return _startOffset; } }
        public int EndOffset { get { return _endOffset; } }
        public int Length { get { return _startOffset >= 0 ? _endOffset - _startOffset + 1 : 0; } }
        public IEnumerable<TextLine> VisualElements { get { return _visualElements; } }
        public IEnumerable<ITextElement> Elements { get { return _elements; } }
        public IEnumerable<ITextSpan> Spans { get { return _spans; } }

        List<TextLine> _visualElements;
        List<ITextElement> _elements;
        List<ITextSpan> _spans;

        // Calculated as elements are added
        int _startOffset;
        int _endOffset;

        public VisualLineData(int lineNumber)
        {
            this.LineNumber = lineNumber;
            _visualElements = new List<TextLine>();
            _elements = new List<ITextElement>();
            _spans = new List<ITextSpan>();
            _startOffset = -1;
            _endOffset = -1;
        }

        public void AddElement(TextLine element, ITextElement textElement)
        {
            // Start Offset
            if (_startOffset == -1)
            {
                _startOffset = textElement.Position.Offset;
                _endOffset = _startOffset;
            }

            // End Offset (Update)
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

            return _elements.First().Position.WithOffset(offset);
        }

        public ITextPosition GetStartPosition()
        {
            if (_startOffset == -1)
                throw new Exception("Must add elements to VisualLineData before usage!");

            return _elements.First().Position;
        }

        public ITextPosition GetEndPosition()
        {
            if (_startOffset == -1)
                throw new Exception("Must add elements to VisualLineData before usage!");

            return _elements.First().Position.WithOffset(this.EndOffset);
        }

        public bool ContainsOffset(int offset)
        {
            if (_startOffset == -1)
                throw new Exception("Must add elements to VisualLineData before usage!");

            if (offset < _startOffset ||
                offset > _endOffset)
                return false;

            return true;
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
    }
}
