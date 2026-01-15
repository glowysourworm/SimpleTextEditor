using System.Globalization;
using System.Windows.Media.TextFormatting;

namespace SimpleTextEditor.Text
{
    // (MSFT Github, CustomTextSource)
    //
    // CustomTextSource is our implementation of TextSource.  This is required to use the WPF
    // text engine. This implementation is very simplistic as is DOES NOT monitor spans of text
    // for different properties. The entire text content is considered a single span and all 
    // changes to the size, alignment, font, etc. are applied across the entire text.
    //
    public class SimpleTextStore : TextSource
    {
        private string _text;

        // Font Rendering Properties
        private SimpleTextRunProperties _properties;

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public SimpleTextStore(double pixelsPerDip, SimpleTextRunProperties properties)
        {
            this.PixelsPerDip = pixelsPerDip;

            _properties = properties;
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

            if (characterIndex >= _text.Length)
            {
                return new TextEndOfParagraph(1);
            }

            // Create TextCharacters using the current font rendering properties.
            if (characterIndex < _text.Length)
            {
                return new TextCharacters(_text, characterIndex, _text.Length - characterIndex, _properties);
            }

            // Return an end-of-paragraph if no more text source.
            return new TextEndOfParagraph(1);
        }

        public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int characterIndexLimit)
        {
            var bufferRange = new CharacterBufferRange(_text, 0, characterIndexLimit);

            return new TextSpan<CultureSpecificCharacterBufferRange>(characterIndexLimit, new CultureSpecificCharacterBufferRange(CultureInfo.CurrentUICulture, bufferRange));
        }

        public override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int textSourceCharacterIndex)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
