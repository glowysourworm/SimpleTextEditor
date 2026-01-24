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

        /// <summary>
        /// Closes the current paragraph. The user code must specify if there was an end of line character so that
        /// the text collection can handle the current line appropriately. It will use the information during validation.
        /// Returns true if there was a trailing line present, otherwise returns false.
        /// </summary>
        /// <param name="closingSpan">EOP span</param>
        /// <param name="span">The ITextSpan marker for this end of paragraph</param>
        /// <param name="lastLineLengthExpected">The length of the last (or currently working) line. Validates previous line entry.</param>
        /// <param name="noEOLCharacterLastLine">Set to true if there was no EOL character on the previous line (making it impossible to close the line out)</param>
        /// <exception cref="Exception">Throws validation exceptions for: 1) The current state of the builder, and 2) EOL validation</exception>
        public bool EndParagraph(TextSpan<TextRun> closingSpan, ITextSpan span, int lastLineLengthExpected, bool noEOLCharacterLastLine)
        {
            var trailingLine = false;

            if (!_addingParagraph)
                throw new Exception("Not working on visual paragraph! Must call BeginParagraph() before this operation!");

            if (_addingLine)
            {
                // Non-Terminated Line
                if (noEOLCharacterLastLine)
                {
                    if (_currentLine == null)
                        throw new Exception("Improper use of VisualTextCollection:  Indicated an end of paragraph before beginning a paragraph.");

                    EndLine(lastLineLengthExpected);

                    trailingLine = true;
                }

                // Invalid EOP
                else
                    throw new Exception("Working on visual line! Must call EndLine() before this operation!");
            }


            if (!_visualParagraphs.ContainsKey(_paragraphCounter))
                _visualParagraphs.Add(_paragraphCounter, _currentParagraph);

            // EOL Issue:  1) Line ends in \r, 2) Line ends with another character
            //
            // We have to keep track of both the visual text and the ITextSource
            // offset.
            //
            // Both cases should have complete closed lines. But, the \r case must
            // impose a new line is added to the visual text source.
            //
            // Finally, the position calculations (all the public user functions) 
            // must assume that the user is unaware of any additional visual lines
            // added to the source!
            //

            _visualParagraphs[_paragraphCounter].SetClosingSpan(span);

            _addingParagraph = false;
            _currentParagraph = null;

            return trailingLine;
        }
        #endregion

        #region (public) Safe Access Methods
        public int GetCharCount(int paragraphNumber)
        {
            if (_visualParagraphs.Count == 0)
                return 0;

            return _visualParagraphs[paragraphNumber].Lines.Sum(x => x.Length);
        }
        public int GetLineCount(int paragraphNumber)
        {
            if (_visualParagraphs.Count == 0)
                return 0;

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
                                .GetEndPosition();
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
