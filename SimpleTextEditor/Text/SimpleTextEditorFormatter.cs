using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

using SimpleTextEditor.Component;
using SimpleTextEditor.Component.Interface;
using SimpleTextEditor.Text.Interface;
using SimpleTextEditor.Text.Visualization;

using SimpleWpf.Extensions.Collection;

namespace SimpleTextEditor.Text
{
    /// <summary>
    /// Formats lines of text from the text source, and prepares rendered TextLine objects for the IDocument
    /// </summary>
    public class SimpleTextEditorFormatter
    {
        // (see MSFT Advanced Text Formatting)
        TextFormatter _formatter;

        SimpleTextVisualOutputData _lastOutputData;

        // Input data to the formatter
        readonly SimpleTextVisualInputData _visualInputData;

        // Primary text store
        readonly ITextSource _textSource;

        // Primary TextRun source
        readonly ITextRunProvider _textRunProvider;

        // Supposedly helps improve performance
        readonly TextRunCache _textRunCache;

        public SimpleTextEditorFormatter(ITextRunProvider textRunProvider, ITextSource textSource, SimpleCaretTracker caretTracker, SimpleTextVisualInputData visualInputData)
        {
            _formatter = TextFormatter.Create(TextFormattingMode.Display);
            _visualInputData = visualInputData;
            _textSource = textSource;
            _textRunProvider = textRunProvider;
            _textRunCache = new TextRunCache();
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

                // First Pass Measurement (MAKES MULTIPLE CALLS TO TextSource!!!)
                //
                var textElement = _formatter.FormatLine(_textRunProvider as TextSource,               // TextStore sub-class
                                                        characterOffset,                              // Character offset to ITextSource
                                                        constraint.Width,                             // UI Width
                                                        _visualInputData
                                                            .GetProperties(TextPropertySet.Normal)
                                                            .ParagraphProperties,                     // Visual Properties (Default Properties)
                                                        lastLineBreak/*,                              // Last Line Break
                                                            _textRunCache*/);                         // TextRunCache (MSFT) stores output of formatter

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

                // Text Position
                var textPosition = new TextElementPosition(textVisualBounds, characterOffset, 0, 0, textLineIndex + 1, textParagraphIndex + 1);

                // Next Element
                textElements.Add(new SimpleTextElement(textElement, textPosition));

                // Increment Indices
                characterOffset += textElement.Length;
                textLineIndex += textEOL;
                textParagraphIndex += textEOP;
            }


            _lastOutputData = new SimpleTextVisualOutputData(textElements,
                                                              constraint,
                                                              new Size(desiredWidth, desiredHeight),
                                                              _textSource.GetLength());
        }

        public SimpleTextVisualOutputData GetLastOutput()
        {
            return _lastOutputData;
        }

        public ITextPosition CharacterOffsetToTextPosition(int characterOffset)
        {
            if (_lastOutputData == null)
                throw new Exception("SimpleTextEditorFormatter Measure must be called to initilaize output data first");

            if (characterOffset < 0 ||
                characterOffset >= _lastOutputData.SourceLength)
                throw new ArgumentOutOfRangeException();

            foreach (var visualElement in _lastOutputData.VisualElements)
            {
                if (visualElement.Position.SourceOffset <= characterOffset &&
                    visualElement.Position.SourceOffset + visualElement.Length > characterOffset)
                {
                    // Reutrn the offset for the requested character
                    return new TextPosition(characterOffset,
                                            visualElement.Position.SourceLineNumber,
                                            0,
                                            visualElement.Position.VisualLineNumber,
                                            visualElement.Position.ParagraphNumber);
                }
            }

            // Default, return the last visual elment's Top-Right corner
            return _lastOutputData.VisualElements.Last().Position;
        }

        /// <summary>
        /// Returns the line height for a given visual line (this is MSFT's TextHeight, line height usually refers
        /// to a multiplier to the text height, so it's usually set to 1.0D.
        /// </summary>
        public double GetVisualLineHeight(int visualLineNumber)
        {
            if (_lastOutputData == null)
                throw new Exception("SimpleTextEditorFormatter Measure must be called to initilaize output data first");

            var visualElement = _lastOutputData.VisualElements
                                               .FirstOrDefault(x => x.Position.VisualLineNumber == visualLineNumber);

            if (visualElement == null)
                throw new Exception("Invalid visual line number");

            return visualElement.Element.TextHeight;
        }

        /// <summary>
        /// Returns the top left point of the visual offset fromt he character offset. This top-left corner of the character's glyph, 
        /// or the top-left corner of the caret UI location. If getTrailingCaretPosition is set to true, then the point will return
        /// from the final glyph location's top-right corner, or a (0,0) point for an empty text source.
        /// </summary>
        /// <param name="characterOffset">Offset of the character in the text source</param>
        public Point CharacterOffsetToVisualOffset(int characterOffset, bool getTrailingCaretPosition = false)
        {
            if (_lastOutputData == null)
                throw new Exception("SimpleTextEditorFormatter Measure must be called to initilaize output data first");

            if (characterOffset < 0 ||
                characterOffset >= _lastOutputData.SourceLength &&
                !getTrailingCaretPosition)
                throw new ArgumentOutOfRangeException();

            // Verify Caret Position:  [0, Length], (not) [0, Length) (we only allow one character overflow)
            //
            if (characterOffset > _lastOutputData.SourceLength)
                throw new ArgumentOutOfRangeException();

            foreach (var visualElement in _lastOutputData.VisualElements)
            {
                if (visualElement.Position.SourceOffset <= characterOffset &&
                    visualElement.Position.SourceOffset + visualElement.Length > characterOffset)
                {
                    // Get glyph run(s) for this element (there is typically only one actual glyph run per line)
                    foreach (var glyphRun in visualElement.Element.GetIndexedGlyphRuns())
                    {
                        if (glyphRun.TextSourceCharacterIndex <= characterOffset &&
                            glyphRun.TextSourceCharacterIndex + glyphRun.TextSourceLength > characterOffset)
                        {
                            // Reutrn the offset for the requested character
                            var width = glyphRun.GlyphRun.AdvanceWidths.Sum(visualElement.Position.SourceOffset, characterOffset, x => x);

                            // Set result to top-left of the character offset
                            return new Point(width, visualElement.Position.VisualBounds.Top);
                        }
                    }
                }
            }

            // NEXT CARET: return the last visual elment's Top-Right corner
            if (getTrailingCaretPosition)
                return _lastOutputData.VisualElements.Last().Position.VisualBounds.TopRight;

            else
                throw new Exception("Caret position not found! Unable to find proper glyph offset");
        }
    }
}
