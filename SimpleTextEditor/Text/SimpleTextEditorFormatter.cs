using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

using SimpleTextEditor.Model;
using SimpleTextEditor.Text.Interface;
using SimpleTextEditor.Text.Source.Interface;
using SimpleTextEditor.Text.Visualization;

namespace SimpleTextEditor.Text
{
    /// <summary>
    /// Formats lines of text from the text source, and prepares rendered TextLine objects for the IDocument
    /// </summary>
    public class SimpleTextEditorFormatter
    {
        // (see MSFT Advanced Text Formatting)
        TextFormatter _formatter;

        VisualOutputData _lastOutputData;

        // Input data to the formatter
        readonly VisualInputData _visualInputData;

        // Primary text store
        readonly ITextSource _textSource;

        // Primary TextRun source
        readonly ITextRunProvider _textRunProvider;

        // Supposedly helps improve performance
        readonly TextRunCache _textRunCache;

        public SimpleTextEditorFormatter(ITextRunProvider textRunProvider, ITextSource textSource, VisualInputData visualInputData)
        {
            _formatter = TextFormatter.Create(TextFormattingMode.Display);
            _visualInputData = visualInputData;
            _textSource = textSource;
            _textRunProvider = textRunProvider;
            _textRunCache = new TextRunCache();
        }

        /// <summary>
        /// Returns last visual output. Must run MesaureText to produce visual output.
        /// </summary>
        public VisualOutputData GetLastOutput()
        {
            return _lastOutputData;
        }

        /// <summary>
        /// MSFT TextFormatter TextRunCache API for updating the text run cache. Use as offset / addition (or)
        /// offset / removal. Set the other to -1.
        /// </summary>
        /// <param name="offset">Offset index in the text source for the addition / removal</param>
        /// <param name="additionLength">addition length</param>
        /// <param name="removalLength">removal length</param>
        public void UpdateCache(int offset, int additionLength, int removalLength)
        {
            //_textRunCache.Invalidate();
            _textRunCache.Change(offset, additionLength, removalLength);
        }

        /// <summary>
        /// Invalidates entire contents of cache, which will "re-draw" the formatted text.
        /// </summary>
        public void InvalidateCache()
        {
            _textRunCache.Invalidate();
        }

        public void MeasureText(Size constraint)
        {
            // Procedure: This must take into account several pre-conditions. The use of the text
            //            formatter depends on what's going on on the UI, so there are UI properties
            //            available here for processing the measurement. Also, the TextRunCache must
            //            (have been) updated prior to measuring the text (this is already done).
            //
            //            The MeasurementData will be used to track the measurement through any 
            //            sub-routines processing the output. 
            //
            // 1) Call FormatLine (1st Pass) to see whether or not any UI properties will intersect the text.
            //      - Yes:  Re-call FormatLine with updated UI properties
            //      - No:   Use the formatted text output and move on
            //
            // 2) Use the output of FormatLine to update MeasurementData. This will output TextRun
            //    based elements - which MAY NOT BE THE ENTIRE VISUAL LINE! So, the SimpleTextElement
            //    implies that this will be a portion of a visual line - with indices into the text 
            //    source to link the visual text tree to the data source text tree.
            //
            //      - CharacterOffset         (from the text source)
            //      - ParagraphNumber         (from the text source - this is TBD)
            //      - VisualLineOffset        (from the visual text source)            
            //      - VisualLineHeight        (UI line height MSFT property)
            //      
            // 3) Check UI Overlap, or special conditions
            //      - Manage our TextRunCache accordingly, in case there is a second pass
            //
            // 4) Call FormatLine (2nd Pass):  This will be the output, if it was needed
            //      

            var textElements = new List<SimpleTextElement>();
            var characterOffset = 0;
            var textLineIndex = 0;
            var textParagraphIndex = 0;
            var textVisualHeight = 0D;                              // Current Y-Position of the visual text
            var desiredHeight = 0D;
            var desiredWidth = 0D;

            // MSFT Advance Text Formatting: This can be tracked after it's understood how to use the EOL!
            //
            // 1) EOL text elements are added for each EOL character ('\r')
            // 2) EOP text elements are added ONLY for paragraph breaks, (AND) (ONE EXTRA ITERATION) (see SimpleTextRunProvider.cs)
            //
            while (characterOffset <= _textSource.GetLength())
            {
                // Pretty sure this will be for text wrapping! (see the TextSource for how EOL works)
                TextLineBreak? lastLineBreak = null;
                TextLine textElement = null;

                try
                {
                    // First Pass Measurement (MAKES MULTIPLE CALLS TO TextSource!!!)
                    //
                    textElement = _formatter.FormatLine(_textRunProvider as TextSource,               // TextStore sub-class
                                                        characterOffset,                              // Character offset to ITextSource
                                                        constraint.Width,                             // UI Width
                                                        _visualInputData
                                                            .GetProperties(TextPropertySet.Normal)
                                                            .ParagraphProperties,                     // Visual Properties (Default Properties)
                                                        lastLineBreak/*,                              // Last Line Break
                                                        _textRunCache*/);                         // TextRunCache (MSFT) stores output of formatter
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    throw new Exception("Error formatting text", ex);
                }

                // Get the number of line breaks
                var textEOL = textElement.GetTextRunSpans().Count(x => x.Value is TextEndOfLine);
                var textEOP = textElement.GetTextRunSpans().Count(x => x.Value is TextEndOfParagraph);

                // Text Visual Y-Position
                textVisualHeight = textLineIndex * textElement.TextHeight;

                // Text Bounds
                var textVisualBounds = new Rect(0, textVisualHeight, textElement.WidthIncludingTrailingWhitespace, textElement.TextHeight);

                // Update Desired Size
                desiredHeight += textElement.TextHeight;
                desiredWidth = Math.Max(desiredWidth, textElement.WidthIncludingTrailingWhitespace);

                if (lastLineBreak != null)
                    throw new Exception("Unhandled Line break detected!");

                // EOL / Line Breaks
                //var startEOL = (textElement.Length == 1) && (textEOL > 0);
                //var endEOL = _textSource.GetLength() > 0 && (_textSource.Get().Get().Last() == '\r') && (textEOL > 0);

                // Text Position (AppendPosition is only used for the caret)
                var startPosition = new TextPosition(characterOffset, textElements.Count, AppendPosition.None, 0, 0, textLineIndex + 1, textParagraphIndex + 1);
                var endPosition = new TextPosition(characterOffset + textElement.Length - 1, textElements.Count, textEOP > 0 ? AppendPosition.Append : AppendPosition.None, 0, 0, textLineIndex + 1, textParagraphIndex + 1);

                // Next Element
                //if (textEOP <= 0)
                textElements.Add(new SimpleTextElement(textElement, textVisualBounds, startPosition, endPosition));

                // NOT ADDING THE FINAL PARAGRAPH TEXT ELEMENT
                //else
                //textElements.Add(new SimpleTextElement(textElement, textVisualBounds, startPosition, startPosition));

                // Increment Indices
                characterOffset += textElement.Length;
                textLineIndex += textEOL;
                textParagraphIndex += textEOP;
            }


            _lastOutputData = new VisualOutputData(textElements,
                                                              constraint,
                                                              new Size(desiredWidth, desiredHeight),
                                                              _textSource.GetLength());
        }
    }
}
