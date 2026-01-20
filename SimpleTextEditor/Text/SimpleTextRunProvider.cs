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

        //private string _currentLine;
        //private int _textLineOffset;
        //private int _textLineCharacterOffset;
        //private IDictionary<IndexRange, ITextProperties> _textLineProperties;

        public SimpleTextRunProvider(ITextSource textSource)
        {
            _textSource = textSource;
        }

        public void SetLineRunProperties(string currentLine,
                                         int textLineOffset,
                                         int textLineCharacterOffset,
                                         IDictionary<IndexRange, ITextProperties> textLineProperties)
        {
            //_currentLine = currentLine;
            //_textLineOffset = textLineOffset;
            //_textLineCharacterOffset = textLineCharacterOffset;
            //_textLineProperties = textLineProperties;
        }

        /// <summary>
        /// INPUT CONDITIONS:  The TextFormatter will recall this method until it has found a TextEndOfParagraph, or some
        ///                    end of line or paragraph TextRun. The character index may go over the end of the ITextSource.
        ///                    Making sure we exhaust the ITextSource properly requires that we know how the end of line
        ///                    is being tracked, at least by our code! 
        ///                    
        ///                    MSFT was not very clear about how to do all the nuts-and-bolts of text processing for a
        ///                    document-style application. But, the TextFormatter is the best performance you'll get for
        ///                    WPF.
        /// </summary>
        public override TextRun GetTextRun(int characterIndex)
        {
            // This will be needed to know how much text to output this call
            //
            var nextEOLIndex = characterIndex >= _textSource.GetLength() ? -1 : _textSource.Search('\r', characterIndex);

            // Look for alternate properties also
            //
            var nextPropertyRange = _textSource.GetNextPropertyRange(characterIndex, false);

            // HOW TO SIGNAL EOL FOR A SINGLE TEXT LINE!
            //
            // https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/advanced-text-formatting
            //
            if (characterIndex == _textSource.GetLength())
                return new TextEndOfParagraph(1);

            // These will be caught by the formatter so that the text measurement
            // can know where it is vertically.
            if (_textSource.Get().Get()[characterIndex] == '\r')
            {
                return new TextEndOfLine(1);
            }

            // Create TextCharacters using the current font rendering properties.
            if (characterIndex < _textSource.GetLength())
            {
                var currentPropertyLength = 0;
                var currentProperties = _textSource.GetProperties(characterIndex, out currentPropertyLength);

                // Render Length:  This will make sure to render the current character, up to but NOT INCLUDING, the EOL character.
                //                 The EOL character will be renedered on the next call to the GetTextRun method, which will get 
                //                 caught above and return the TextEndOfLine.
                //
                //                 Alternate text run properties are also handled here by checkingh them, next.
                //

                // Entire Line
                var renderLength = _textSource.GetLength() - characterIndex;

                // Next Alternate Character Properties
                if (nextPropertyRange != IndexRange.Empty)
                {
                    // EOL happens first
                    if (nextEOLIndex >= 0 && nextEOLIndex < nextPropertyRange.StartIndex)
                    {
                        renderLength = nextEOLIndex - characterIndex;
                    }

                    else
                        renderLength = nextPropertyRange.StartIndex - characterIndex;
                }
                else if (nextEOLIndex >= 0)
                {
                    renderLength = nextEOLIndex - characterIndex;
                }
                else if (currentPropertyLength > 0)
                {
                    renderLength = currentPropertyLength;
                }
                else
                    renderLength = _textSource.GetLength() - characterIndex;

                // TextCharacters requires an absolute index into the char[]. There may be a way to utilize char* for 
                // better (native) performance; but that would take some testing and playing around.
                //
                return new TextCharacters(_textSource.Get().Get(),
                                          characterIndex,
                                          renderLength,
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
