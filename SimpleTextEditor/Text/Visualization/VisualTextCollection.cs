using System.Windows.Media.TextFormatting;

using SimpleTextEditor.Model.Interface;
using SimpleTextEditor.Text.Visualization.Element.Interface;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.SimpleCollections.Collection;

namespace SimpleTextEditor.Text.Visualization
{
    /// <summary>
    /// Keeps track of formatted text in several ways: 1) Spans of text or formatted text output back
    /// from the TextFormatter (which are the inner values of each TextLine), 2) ITextPosition lookup
    /// </summary>
    public class VisualTextCollection
    {
        public int LineCount { get { return _visualLines.Count; } }

        // Visual Line Data
        SimpleDictionary<ITextPosition, ITextElement> _positionLookup;
        SimpleDictionary<int, List<ITextElement>> _visualLineElements;

        // Visual Lines
        SimpleDictionary<int, VisualLineData> _visualLines;

        // ITextSpan Lookup
        SimpleDictionary<int, List<ITextSpan>> _textSpanLookup;

        public VisualTextCollection()
        {
            _positionLookup = new SimpleDictionary<ITextPosition, ITextElement>();
            _visualLineElements = new SimpleDictionary<int, List<ITextElement>>();
            _visualLines = new SimpleDictionary<int, VisualLineData>();
            _textSpanLookup = new SimpleDictionary<int, List<ITextSpan>>();
        }

        public void Clear()
        {
            _visualLineElements.Clear();
            _visualLines.Clear();
            _positionLookup.Clear();
            _textSpanLookup.Clear();
        }

        /// <summary>
        /// Adds span to span's offset position. May set multiple spans to same offset - which
        /// follows MSFT's EOL / EOP case. They have zero length.
        /// </summary>
        /// <param name="span"></param>
        public void AddSpan(ITextSpan span)
        {
            if (!_textSpanLookup.ContainsKey(span.SpanPosition))
                _textSpanLookup.Add(span.SpanPosition, new List<ITextSpan>() { span });

            else
                _textSpanLookup[span.SpanPosition].Add(span);
        }

        /// <summary>
        /// Also adds to span lookup!
        /// </summary>
        public void AddElement(ITextElement element)
        {
            if (!_visualLineElements.ContainsKey(element.Position.VisualLineNumber))
                _visualLineElements.Add(element.Position.VisualLineNumber, new List<ITextElement>() { element });

            else
                _visualLineElements[element.Position.VisualLineNumber].Add(element);

            _positionLookup.Add(element.Position, element);


            AddSpan(element);
        }

        public void AddLine(TextLine line, int startOffset, int endOffset, int lineNumber)
        {
            if (!_visualLines.ContainsKey(lineNumber))
                _visualLines.Add(lineNumber, new TextLineData(startOffset, endOffset, lineNumber));

            _visualLines[lineNumber].Elements.Add(line);
        }

        public IList<ITextSpan> GetSpan(int spanPosition)
        {
            return _textSpanLookup[spanPosition];
        }

        public ITextElement GetElement(ITextPosition position)
        {
            return _positionLookup[position];
        }

        public IList<ITextElement> GetLineElements(int visualLineNumber)
        {
            return _visualLineElements[visualLineNumber];
        }

        public IList<TextLine> GetVisualLine(int visualLineNumber)
        {
            return _visualLines[visualLineNumber].Elements;
        }

        public IEnumerable<ITextElement> GetVisualElements()
        {
            return _positionLookup.Values;
        }

        public IEnumerable<TextLineData> GetVisualLines()
        {
            return _visualLines.SelectMany(x => x.Value.Elements).Actualize();
        }

        /// <summary>
        /// Returns first line that contains the provided offset
        /// </summary>
        /// <param name="anyOffset">May be any offset in the ITextSource range</param>
        public IEnumerable<ITextElement> SearchLines(int anyOffset)
        {
            foreach (var pair in _visualLineElements)
            {
                var textLine = _visualLines.FirstOrDefault(x => x.Value.StartOffset <= anyOffset && x.Value.EndOffset >= anyOffset);

                if (textLine.Value == null)
                    continue;

                var element = pair.Value.FirstOrDefault(x => pair.Key == textLine.Key);

                if (element != null)
                    return pair.Value;
            }

            return Enumerable.Empty<ITextElement>();
        }

        public IEnumerable<ITextElement> SearchLines(Func<ITextElement, bool> predicate)
        {
            foreach (var list in _visualLineElements.Values)
            {
                var element = list.FirstOrDefault(x => predicate(x));

                if (element != null)
                    return list;
            }

            return Enumerable.Empty<ITextElement>();
        }

        public ITextElement? GetLastElement()
        {
            return _visualLineElements.Values.LastOrDefault()?.LastOrDefault();
        }

        public ITextElement? GetFirstElement()
        {
            return _visualLineElements.Values.FirstOrDefault()?.FirstOrDefault();
        }
    }
}
