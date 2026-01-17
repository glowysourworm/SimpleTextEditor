using System.Globalization;
using System.Windows.Media.TextFormatting;

using SimpleTextEditor.Model;
using SimpleTextEditor.Text.Interface;
using SimpleTextEditor.Text.Visualization;

namespace SimpleTextEditor.Text
{
    // (MSFT Github, CustomTextSource)
    //
    // CustomTextSource is our implementation of TextSource.  This is required to use the WPF
    // text engine. This implementation utilizes a simple text "Rope" data structure to handle
    // fast insertions and searches.
    //
    public class SimpleTextStore : TextSource, ITextSource
    {
        // Text data structure for Get / Set (range). This may be upgraded to handle the 
        // rest of the text properties; and should use a common character array source. Currently,
        // the TextString is not shared between nodes.
        private ITextSource _textSource;

        // Font Rendering Properties
        private SimpleTextVisualInputData _visualInputData;
        private TextPropertySet _currentTextPropertySet;

        // Mouse Interaction Data
        private MouseData _mouseData;

        // Keeps track of caret position (this will be the character AFTER to caret rendering). The last
        // position is off the end of the text source by one.
        private int _caretPosition;

        public SimpleTextStore(SimpleTextVisualInputData visualInputData)
        {
            _visualInputData = visualInputData;
            _currentTextPropertySet = TextPropertySet.Normal;
            _textSource = new LinearTextSource();
            _mouseData = new MouseData();
        }

        public TextString Get()
        {
            return _textSource.Get();
        }
        /// <summary>
        /// Returns total length of text source
        /// </summary>
        public int GetLength()
        {
            return _textSource.GetLength();
        }
        public void AppendText(string text)
        {
            _textSource.AppendText(text);

            SetCaretPosition(_textSource.GetLength() - 1);
        }
        public void InsertText(int offset, string text)
        {
            _textSource.InsertText(offset, text);

            SetCaretPosition(offset);
        }
        public void RemoveText(int offset, int count)
        {
            _textSource.RemoveText(offset, count);

            SetCaretPosition(offset);
        }
        public void SetMouseInfo(MouseData mouseData)
        {
            _mouseData = mouseData;
        }

        /// <summary>
        /// Gets the position of the caret. The final caret position is off the end 
        /// of the text by one.
        /// </summary>
        public int GetCaretPosition()
        {
            return _caretPosition;
        }

        /// <summary>
        /// Sets the caret position based on text offset. The final caret position is off the
        /// end of the text by one
        /// </summary>
        public void SetCaretPosition(int textOffset)
        {
            _caretPosition = textOffset + 1;
        }

        /// <summary>
        /// Used by the TextFormatter object to retrieve a run of text from the text source. 
        /// </summary>
        public override TextRun GetTextRun(int characterIndex)
        {
            // Make sure text source index is in bounds.
            if (characterIndex < 0)
                throw new ArgumentOutOfRangeException("characterIndex", "Value must be greater than 0.");

            // PERFORMANCE PROBLEM:  WE HAVE TO PRODUCE A TEXT MEASUREMENT TO SEE IF THE MOUSE
            // IS HIGHLIGHTING THE TEXT! SO, THIS FORMATTED TEXT WILL ALREADY BE AVAILABLE.
            //
            //var text = _textSource.Get().GetSubString(characterIndex, _textSource.GetLength())
            //var formattedText = new FormattedText();

            // End of Line
            if (_textSource.Search('\r', characterIndex) >= 0)
            {
                return new TextEndOfLine(1);
            }

            // End of Paragraph
            if (characterIndex >= _textSource.GetLength())
            {
                return new TextEndOfParagraph(1);
            }

            // PERFORMANCE!  This needs to be built into the text source. 
            //if (_textSource.Search(' ', characterIndex) >= 0)
            //{

            //}

            // Create TextCharacters using the current font rendering properties.
            if (characterIndex < _textSource.GetLength())
            {
                // TextString.Get() returns a char[] for the text source w/o copying memory
                return new TextCharacters(_textSource.Get().Get(),
                                          characterIndex,
                                          _textSource.GetLength() - characterIndex,
                                          _visualInputData.GetProperties(_currentTextPropertySet));
            }

            // Return an end-of-paragraph if no more text source.
            return new TextEndOfParagraph(1);
        }

        public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int characterIndexLimit)
        {
            // TextString.Get() returns a char[] for the text source w/o copying memory
            var bufferRange = new CharacterBufferRange(_textSource.Get().Get(), 0, characterIndexLimit);

            return new TextSpan<CultureSpecificCharacterBufferRange>(characterIndexLimit, new CultureSpecificCharacterBufferRange(CultureInfo.CurrentUICulture, bufferRange));
        }

        public override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int textSourceCharacterIndex)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public int Search(char character, int startIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Selects text properties for next draw pass. Requires that the formatter be invalidated.
        /// </summary>
        public void SelectTextProperties(TextPropertySet propertySet)
        {
            _currentTextPropertySet = propertySet;
        }

        public TextPropertySet GetCurrentTextProperties()
        {
            return _currentTextPropertySet;
        }
    }
}
