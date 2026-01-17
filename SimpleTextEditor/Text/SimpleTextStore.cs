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

        // Keeps track of caret position (this will be the character AFTER to caret rendering). The last
        // position is off the end of the text source by one.
        private int _caretPosition;

        public SimpleTextStore(SimpleTextVisualInputData visualInputData)
        {
            _visualInputData = visualInputData;
            _textSource = new LinearTextSource(visualInputData.GetProperties(TextPropertySet.Normal));
        }

        public TextEditorString Get()
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
        public void SetProperties(IndexRange range, ITextProperties properties)
        {
            _textSource.SetProperties(range, properties);
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
            if (characterIndex < 0 || characterIndex >= _textSource.GetLength())
            {
                //throw new ArgumentOutOfRangeException("characterIndex", "Value must be greater than 0.");

                return new TextEndOfLine(1);
            }


            // PERFORMANCE PROBLEM:  WE HAVE TO PRODUCE A TEXT MEASUREMENT TO SEE IF THE MOUSE
            // IS HIGHLIGHTING THE TEXT! SO, THIS FORMATTED TEXT WILL ALREADY BE AVAILABLE.
            //
            //var text = _textSource.Get().GetSubString(characterIndex, _textSource.GetLength())
            //var formattedText = new FormattedText();

            // End of Line
            if (_textSource.Get().Get()[characterIndex] == '\r')
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
                // Find property changes in the text
                var propertyLength = 0;
                var textProperties = _textSource.GetProperties(characterIndex, out propertyLength);

                if (propertyLength < 0)
                    propertyLength = _textSource.GetLength();

                // Text Source returns a char[], which does not copy the source characters
                //
                return new TextCharacters(_textSource.Get().Get(),
                                          characterIndex,
                                          propertyLength,
                                          textProperties.Properties);
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

        public IndexRange[] GetPropertySlices()
        {
            return _textSource.GetPropertySlices();
        }

        public ITextProperties GetProperties(int offset, out int length)
        {
            return _textSource.GetProperties(offset, out length);
        }
    }
}
