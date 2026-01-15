using System.Globalization;
using System.Windows.Media.TextFormatting;

using SimpleTextEditor.Model;
using SimpleTextEditor.Text.Interface;

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
        private Rope _textSource;

        // Font Rendering Properties
        private SimpleTextRunProperties _properties;

        public SimpleTextStore(double pixelsPerDip, SimpleTextRunProperties properties)
        {
            this.PixelsPerDip = pixelsPerDip;

            _textSource = new Rope();
            _properties = properties;
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
            _textSource.Append(text);
        }
        public void InsertText(int offset, string text)
        {
            _textSource.Insert(offset, text);
        }
        public void RemoveText(int offset, int count)
        {
            _textSource.Remove(offset, count);
        }

        /// <summary>
        /// Used by the TextFormatter object to retrieve a run of text from the text source. 
        /// </summary>
        /// <param name="characterIndex"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public override TextRun GetTextRun(int characterIndex)
        {
            // Make sure text source index is in bounds.
            if (characterIndex < 0)
                throw new ArgumentOutOfRangeException("characterIndex", "Value must be greater than 0.");

            if (characterIndex >= _textSource.GetLength())
            {
                return new TextEndOfParagraph(1);
            }

            // Create TextCharacters using the current font rendering properties.
            if (characterIndex < _textSource.GetLength())
            {
                // TextString.Get() returns a char[] for the text source w/o copying memory
                return new TextCharacters(_textSource.Get().Get(), characterIndex, _textSource.GetLength() - characterIndex, _properties);
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
    }
}
