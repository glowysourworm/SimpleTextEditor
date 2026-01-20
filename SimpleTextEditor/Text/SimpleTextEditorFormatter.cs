using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

using SimpleTextEditor.Model;
using SimpleTextEditor.Model.Interface;
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

        public SimpleTextEditorFormatter(ITextRunProvider textRunProvider, ITextSource textSource, SimpleTextVisualInputData visualInputData)
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

                // EOL / Line Breaks
                var startEOL = (textElement.Length == 1) && (textEOL > 0);
                var endEOL = _textSource.GetLength() > 0 && (_textSource.Get().Get().Last() == '\r') && (textEOL > 0);

                // Text Position
                var startPosition = new TextPosition(characterOffset, textElements.Count, startEOL, 0, 0, textLineIndex + 1, textParagraphIndex + 1);
                var endPosition = new TextPosition(characterOffset + textElement.Length - 1, textElements.Count, endEOL, 0, 0, textLineIndex + 1, textParagraphIndex + 1);

                // Next Element
                textElements.Add(new SimpleTextElement(textElement, textVisualBounds, startPosition, endPosition));

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

        public ITextPosition CharacterOffsetToTextPosition(int characterOffset, bool isAppendPosition)
        {
            if (_lastOutputData == null)
                throw new Exception("SimpleTextEditorFormatter Measure must be called to initilaize output data first");

            if (isAppendPosition)
            {
                if (characterOffset < 0 ||
                   (characterOffset > _lastOutputData.SourceLength))
                    throw new ArgumentOutOfRangeException();
            }

            else
            {
                if (characterOffset < 0 ||
                    characterOffset >= _lastOutputData.SourceLength)
                    throw new ArgumentOutOfRangeException();
            }

            if (!isAppendPosition)
            {
                foreach (var visualElement in _lastOutputData.VisualElements)
                {
                    if (visualElement.Contains(characterOffset))
                    {
                        // Reutrn the offset for the requested character
                        return new TextPosition(characterOffset,
                                                visualElement.Start.ElementIndex,
                                                visualElement.Start.IsAtEndOfLine,
                                                visualElement.Start.SourceLineNumber,
                                                0,
                                                visualElement.Start.VisualLineNumber,
                                                visualElement.Start.ParagraphNumber);
                    }
                }

                throw new Exception("Character offset not found!");
            }
            else
            {
                foreach (var visualElement in _lastOutputData.VisualElements)
                {
                    if (visualElement.Contains(characterOffset))
                    {
                        return new TextPosition(characterOffset,
                                                visualElement.Start.ElementIndex,
                                                characterOffset == visualElement.End.SourceOffset,
                                                visualElement.Start.SourceLineNumber,
                                                visualElement.Start.VisualColumn,
                                                visualElement.Start.VisualLineNumber,
                                                visualElement.Start.ParagraphNumber);
                    }
                }

                return _lastOutputData.VisualElements.Last().End;
            }
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
                                               .FirstOrDefault(x => x.Start.VisualLineNumber == visualLineNumber);

            if (visualElement == null)
                throw new Exception("Invalid visual line number");

            return visualElement.Element.TextHeight;
        }

        /// <summary>
        /// Returns the closest ITextPosition for the provided point:  1) Pre-cursor would be above the
        /// text block, 2) Inside of the text block would return the closest glyph position, 3) After the
        /// text block would return the last glyph's position.
        /// </summary>
        public ITextPosition VisualPointToTextPosition(Point point, out bool isAppendPosition)
        {
            if (_lastOutputData == null)
                throw new Exception("SimpleTextEditorFormatter Measure must be called to initilaize output data first");

            if (!_lastOutputData.VisualElements.Any())
                throw new Exception("SimpleTextEditorFormatter Measure must be called to initilaize output data first");

            isAppendPosition = false;

            // ENTIRE TEXT BOUNDS
            var lastRectangle = new Rect(_lastOutputData.ConstraintSize);

            if (point.Y <= lastRectangle.Top || point.X <= lastRectangle.Left)
                return _lastOutputData.VisualElements.First().Start;

            else if (point.Y >= lastRectangle.Bottom || point.X >= lastRectangle.Right)
            {
                isAppendPosition = true;

                return _lastOutputData.VisualElements.Last().End;
            }


            // LINE BOUNDS
            else
            {
                // Containing Text Element
                var textElement = _lastOutputData.VisualElements
                                                 .FirstOrDefault(x => x.VisualBounds.Top <= point.Y && x.VisualBounds.Bottom >= point.Y);

                // None -> Take closest line of text
                if (textElement == null)
                {
                    isAppendPosition = true;

                    return _lastOutputData.VisualElements.Last().End;
                }

                // Search glyph run to see where point lies
                var currentWidth = 0D;
                var currentIndex = 0;

                foreach (var glyphRun in textElement.Element.GetIndexedGlyphRuns())
                {
                    foreach (var advanceWidth in glyphRun.GlyphRun.AdvanceWidths)
                    {
                        // Found Text Position
                        if (textElement.VisualBounds.Left + currentWidth >= point.X)
                        {
                            isAppendPosition = (textElement.Start.SourceOffset + currentIndex) == textElement.End.SourceOffset;

                            return new TextPosition(textElement.Start.SourceOffset + currentIndex,
                                                    textElement.Start.ElementIndex,
                                                    isAppendPosition,
                                                    textElement.Start.SourceLineNumber,
                                                    textElement.Start.VisualColumn,
                                                    textElement.Start.VisualLineNumber,
                                                    textElement.Start.ParagraphNumber);
                        }

                        currentIndex++;
                        currentWidth += advanceWidth;
                    }
                }

                //throw new Exception("SimpleTextEditorFormatter unable to locate text element for provided point!");

                isAppendPosition = true;

                return textElement.End;
            }
        }

        /// <summary>
        /// Returns the top left point of the visual offset from the character offset. If append position is set, then
        /// the EOL position is taken, which is calculated from the line's text element(s).
        /// </summary>
        public Point CharacterOffsetToVisualOffset(int characterOffset, bool isAppendPosition)
        {
            // Character -> ITextPosition
            var position = CharacterOffsetToTextPosition(characterOffset, isAppendPosition);

            // Get ITextElement from collection
            var visualElement = _lastOutputData.GetElement(position.ElementIndex);
            var positionX = 0D;

            // Get glyph run(s) for this element (there is typically only one actual glyph run per line)
            foreach (var glyphRun in visualElement.Element.GetIndexedGlyphRuns())
            {
                // Prepend
                if (glyphRun.TextSourceCharacterIndex < characterOffset)
                {
                    // Reutrn the offset for the requested character
                    for (int index = 0; index < glyphRun.GlyphRun.AdvanceWidths.Count &&
                                        index + visualElement.Start.SourceOffset < characterOffset; index++)
                    {
                        positionX += glyphRun.GlyphRun.AdvanceWidths[index];
                    }
                }
            }

            // Character Position (Insert)
            if (!isAppendPosition)
                return new Point(positionX, visualElement.VisualBounds.Top);

            // Append Position: EOL, or top-right character position if it is at the end of the line
            else
            {
                return visualElement.VisualBounds.TopRight;
            }
        }
    }
}
