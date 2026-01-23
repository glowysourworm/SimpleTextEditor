using System.ComponentModel.DataAnnotations;
using System.Windows.Media.TextFormatting;

using SimpleTextEditor.Model.Interface;
using SimpleTextEditor.Text.Visualization.Element.Interface;

using SimpleWpf.SimpleCollections.Collection;

namespace SimpleTextEditor.Text.Visualization
{
    /// <summary>
    /// Keeps track of formatted text in several ways: 1) Spans of text or formatted text output back
    /// from the TextFormatter (which are the inner values of each TextLine), 2) ITextPosition lookup
    /// </summary>
    public class VisualTextCollection
    {
        // Visual Paragraphs
        SimpleDictionary<int, VisualParagraphData> _visualParagraphs;

        // BeginParagraph() -> BeginLine() ... EndLine() -> EndParagraph( {closing span} )
        int _lineCounter;               // Counts visual lines, starting at 1
        int _paragraphCounter;          // Counts visual paragraphs
        bool _addingLine;
        bool _addingParagraph;
        VisualParagraphData? _currentParagraph;
        VisualLineData? _currentLine;

        public VisualTextCollection()
        {
            _visualParagraphs = new SimpleDictionary<int, VisualParagraphData>();
            _lineCounter = 0;
            _paragraphCounter = 0;
            _addingLine = false;
            _addingParagraph = false;
            _currentLine = null;
            _currentParagraph = null;
        }

        #region (public) Builder Methods
        public void Clear()
        {
            if (_addingParagraph)
                throw new Exception("Working on visual paragraph! Must call EndParagraph() before this operation!");

            if (_addingLine)
                throw new Exception("Working on visual line! Must call EndLine() before this operation!");

            _visualParagraphs.Clear();
            _lineCounter = 0;
            _paragraphCounter = 0;
            _currentLine = null;
            _currentParagraph = null;
        }
        public void BeginParagraph()
        {
            if (_addingParagraph)
                throw new Exception("Working on visual paragraph! Must call EndParagraph() before this operation!");

            if (_addingLine)
                throw new Exception("Working on visual line! Must call EndLine() before this operation!");

            _addingParagraph = true;
            _paragraphCounter++;
            _currentParagraph = new VisualParagraphData(_paragraphCounter);
        }
        public void BeginLine()
        {
            if (!_addingParagraph)
                throw new Exception("Not working on visual paragraph! Must call BeginParagraph() before this operation!");

            if (_addingLine)
                throw new Exception("Working on visual line! Must call EndLine() before this operation!");

            _addingLine = true;
            _lineCounter++;
            _currentLine = new VisualLineData(_lineCounter);
        }

        /// <summary>
        /// DO NOT USE FOR ADDING PARAGRAPH EOP ELEMENTS!
        /// </summary>
        public void Add(TextLine visualElement, ITextElement element)
        {
            if (!_addingParagraph)
                throw new Exception("Not working on visual paragraph! Must call BeginParagraph() before this operation!");

            if (!_addingLine)
                throw new Exception("Not working on visual line! Must call BeginLine() before this operation!");

            _currentLine.AddElement(visualElement, element);
        }

        /// <summary>
        /// DO NOT USE FOR ADDING PARAGRAPH EOP ELEMENTS!
        /// </summary>
        public void Add(TextSpan<TextRun> visualSpan, ITextSpan span)
        {
            if (!_addingParagraph)
                throw new Exception("Not working on visual paragraph! Must call BeginParagraph() before this operation!");

            if (!_addingLine)
                throw new Exception("Not working on visual line! Must call BeginLine() before this operation!");

            _currentLine.AddSpan(span);
        }

        /// <summary>
        /// Finalizes visual line. Use the line length validation to validate the line just created.
        /// </summary>
        /// <param name="lineLengthExpected">Line length from the formatter</param>
        public void EndLine(int lineLengthExpected)
        {
            if (!_addingParagraph)
                throw new Exception("Not working on visual paragraph! Must call BeginParagraph() before this operation!");

            if (!_addingLine)
                throw new Exception("Not working on visual line! Must call BeginLine() before this operation!");

            if (_currentLine == null)
                throw new Exception("Not working on visual line! Must call BeginLine() before this operation!");

            if (_currentParagraph == null)
                throw new Exception("Not working on a paragraph! Must call BeginParagraph() before this operation!");

            if (_currentLine.Length != lineLengthExpected)
                throw new ValidationException(string.Format("Line length invalid:  Expected={0} Actual={1}", lineLengthExpected, _currentLine.Length));

            if (!_visualParagraphs.ContainsKey(_paragraphCounter))
                _visualParagraphs.Add(_paragraphCounter, _currentParagraph);

            _visualParagraphs[_paragraphCounter].AddLine(_currentLine);

            _addingLine = false;
            _currentLine = null;
        }
        public void EndParagraph(TextSpan<TextRun> closingSpan, ITextSpan span)
        {
            if (!_addingParagraph)
                throw new Exception("Not working on visual paragraph! Must call BeginParagraph() before this operation!");

            if (_addingLine)
                throw new Exception("Working on visual line! Must call EndLine() before this operation!");

            if (!_visualParagraphs.ContainsKey(_paragraphCounter))
                _visualParagraphs.Add(_paragraphCounter, _currentParagraph);

            _visualParagraphs[_paragraphCounter].SetClosingSpan(span);

            _addingParagraph = false;
            _currentParagraph = null;
        }
        #endregion

        #region (public) Safe Access Methods
        public int GetLineCount(int paragraphNumber)
        {
            return _visualParagraphs[paragraphNumber].Lines.Count();
        }
        public int GetParagraphCount()
        {
            return _paragraphCounter;
        }
        public bool AddingLine(int paragraphNumber)
        {
            return _addingLine;
        }
        public bool AddingParagraph()
        {
            return _addingParagraph;
        }
        #endregion

        #region (public) User Methods
        public VisualLineData GetFirstVisualLine()
        {
            ValidateForUsing();

            return _visualParagraphs.First().Value.Lines.First();
        }
        public VisualLineData GetLastVisualLine()
        {
            ValidateForUsing();

            return _visualParagraphs.Last().Value.Lines.Last();
        }
        public VisualLineData GetVisualLine(int visualParagraphNumber, int visualLineNumber)
        {
            ValidateForUsing();

            return _visualParagraphs[visualParagraphNumber].GetLine(visualLineNumber);
        }
        public VisualLineData GetVisualLineForOffset(int anyOffset)
        {
            ValidateForUsing();

            foreach (var paragraph in _visualParagraphs.Values)
            {
                foreach (var line in paragraph.Lines)
                {
                    if (line.ContainsOffset(anyOffset))
                        return line;
                }
            }

            throw new Exception("Unable to locate visual line for offset " + anyOffset);
        }
        public VisualLineData? FirstLineWhere(Func<VisualLineData, bool> predicate)
        {
            ValidateForUsing();

            foreach (var paragraph in _visualParagraphs.Values)
            {
                foreach (var line in paragraph.Lines)
                {
                    if (predicate(line))
                        return line;
                }
            }

            return null;
        }
        public VisualLineData? FirstLineWhereAnyElement(Func<ITextElement, bool> predicate)
        {
            ValidateForUsing();

            foreach (var paragraph in _visualParagraphs.Values)
            {
                foreach (var line in paragraph.Lines)
                {
                    foreach (var element in line.Elements)
                    {
                        if (predicate(element))
                            return line;
                    }
                }
            }

            return null;
        }
        public ITextPosition? GetAppendPosition()
        {
            ValidateForUsing();

            var lastParagraph = _visualParagraphs.LastOrDefault();

            if (lastParagraph.Value == null)
                return null;

            return lastParagraph.Value
                                .Lines
                                .LastOrDefault()?
                                .Elements?
                                .LastOrDefault()?
                                .Position;
        }
        public ITextPosition? GetAppendPosition(int paragraphNumber)
        {
            ValidateForUsing();

            return _visualParagraphs[paragraphNumber]
                                .Lines
                                .LastOrDefault()?
                                .Elements?
                                .LastOrDefault()?
                                .Position;
        }
        #endregion

        #region (private) Methods

        // Makes sure that the user has properly closed lines and paragraphs before calling other
        // methods. 
        private void ValidateForUsing()
        {
            if (_addingParagraph)
                throw new Exception("Working on visual paragraph! Must call EndParagraph() before this operation!");

            if (_addingLine)
                throw new Exception("Working on visual line! Must call EndLine() before this operation!");
        }
        #endregion
    }
}
