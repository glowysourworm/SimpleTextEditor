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
    public class SimpleTextRunProvider : TextSource, ITextRunProvider
    {
        // Text data structure for Get / Set (range). This may be upgraded to handle the 
        // rest of the text properties; and should use a common character array source. Currently,
        // the TextString is not shared between nodes.
        private ITextSource _textSource;

        private string _currentLine;
        private int _textLineOffset;
        private int _textLineCharacterOffset;
        private IDictionary<IndexRange, ITextProperties> _textLineProperties;

        public SimpleTextRunProvider(ITextSource textSource)
        {
            _textSource = textSource;
        }

        public void SetLineRunProperties(string currentLine,
                                         int textLineOffset,
                                         int textLineCharacterOffset,
                                         IDictionary<IndexRange, ITextProperties> textLineProperties)
        {
            _currentLine = currentLine;
            _textLineOffset = textLineOffset;
            _textLineCharacterOffset = textLineCharacterOffset;
            _textLineProperties = textLineProperties;
        }

        /// <summary>
        /// Used by the TextFormatter object to retrieve a run of text from the text source. 
        /// </summary>
        public override TextRun GetTextRun(int characterIndex)
        {
            if (_textLineProperties == null)
                throw new Exception("Must call SetLineRunProperties before using TextFormatter with SimpleTextRunProvider");

            // STILL TRYING TO FIGURE OUT HOW TO SIGNAL EOL FOR A SINGLE TEXT LINE!
            //
            // https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/advanced-text-formatting
            //
            if (characterIndex == _currentLine.Length)
                return new TextEndOfParagraph(1);

            var currentRange = _textLineProperties.Keys.FirstOrDefault(x => x.Contains(characterIndex));

            if (currentRange == null)
                throw new Exception("Trying to format text outside the bounds of the current line set by calling SimpleTextRunProvider");

            if (characterIndex >= _textLineCharacterOffset + _currentLine.Length)
                throw new ArgumentOutOfRangeException("Internal failure:  ITextSource prepared inaccurate index data!");

            // Make sure text source index is in bounds.
            if (characterIndex == _textLineCharacterOffset + _currentLine.Length - 1)
            {
                return new TextEndOfLine(1);
            }

            // End of Paragraph
            //if (characterIndex >= _textSource.GetLength())
            //{
            //    return new TextEndOfParagraph(1);
            //}

            // Create TextCharacters using the current font rendering properties.
            if (characterIndex < _textSource.GetLength())
            {
                var currentProperties = _textLineProperties[currentRange];

                if (currentRange.StartIndex != characterIndex)
                    Console.WriteLine("Possible character indexing out of place:  SimpleTextRunProvider.cs");

                // TextCharacters requires an absolute index into the char[]. There may be a way to utilize char* for 
                // better (native) performance; but that would take some testing and playing around.
                //
                return new TextCharacters(_currentLine,
                                          0,
                                          _currentLine.Length,
                                          currentProperties.Properties);
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
