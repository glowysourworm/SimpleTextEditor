using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

using SimpleTextEditor.Component;
using SimpleTextEditor.Model;
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

        // Mouse data from the UI
        MouseData _mouseData;

        // Input data to the formatter
        readonly SimpleTextVisualInputData _visualInputData;

        // Primary text store
        readonly SimpleTextStore _textStore;

        // Empty text store used for formatting
        readonly SimpleTextStore _emptyTextStore;

        // Supposedly helps improve performance
        readonly TextRunCache _textRunCache;

        // Class for handling measurement pass
        protected class MeasurementData
        {
            List<SimpleTextElement> _visualElements;

            /// <summary>
            /// Absolute text source offset for the character
            /// </summary>
            public int CharacterOffset { get; private set; }

            /// <summary>
            /// Current visual line offset for the current element. (This will index into the visual lines collection)
            /// </summary>
            public int VisualLineNumber { get; private set; }

            /// <summary>
            /// Current paragraph number for the current element
            /// </summary>
            public int ParagraphNumber { get; private set; }

            /// <summary>
            /// Visual line height (MSFT typography metric)
            /// </summary>
            public double VisualLineHeight { get; private set; }

            /// <summary>
            /// Desired total width of the output in UI coordinates
            /// </summary>
            public double DesiredWidth { get; private set; }

            /// <summary>
            /// Desired total height of the output in UI coordinates
            /// </summary>
            public double DesiredHeight { get; private set; }

            /// <summary>
            /// Last line break found by the formatter. This should be related to text wrapping. But, there could
            /// be other implied meanings from MSFT.
            /// </summary>
            public TextLineBreak? LastLineBreak { get; private set; }

            /// <summary>
            /// Primary output of a measurement pass
            /// </summary>
            public IEnumerable<SimpleTextElement> VisualElements { get { return _visualElements; } }

            public MeasurementData()
            {
                _visualElements = new List<SimpleTextElement>();
                this.LastLineBreak = null;
            }

            /// <summary>
            /// Measures element based on current input location. Does NOT advance parameters.
            /// </summary>
            public Rect MeasureElement(TextLine textElement)
            {
                // The only way to know if there's an X offset is to get the SimpleTextStore to return it to you with the
                // TextRun instance. There should be data in this TextLine; but I haven't seen any circumstance where this
                // isn't a "line" of text by itself.
                //
                return new Rect(0, (this.VisualLineNumber - 1) * textElement.TextHeight, textElement.Width, textElement.Height);
            }

            /// <summary>
            /// Commits new element to the data and updates properties
            /// </summary>
            public void CommitElement(SimpleTextElement nextElement)
            {
                this.DesiredHeight += nextElement.Element.TextHeight;
                this.VisualLineNumber++;

                // Text Height
                this.VisualLineHeight = nextElement.Element.TextHeight;

                // TextWidth:  (Line Break (?) (Not likely related to text-wrapping))
                if (nextElement.Element.WidthIncludingTrailingWhitespace > this.DesiredWidth)
                    this.DesiredWidth = nextElement.Element.WidthIncludingTrailingWhitespace;

                // Character Offset
                this.CharacterOffset += nextElement.Length;                                      // Advance Character Offset (text source)

                _visualElements.Add(nextElement);
            }

            /// <summary>
            /// Builds element and calculates from current variables (does not update measurement data)
            /// </summary>
            public SimpleTextElement BuildElement(Rect visualBounds, TextLine textElement, string cachedText, SimpleTextRunProperties properties)
            {
                var desiredHeight = this.DesiredHeight;
                var desiredWidth = textElement.WidthIncludingTrailingWhitespace;
                var visualLineNumber = this.VisualLineNumber;
                var characterOffset = this.CharacterOffset + textElement.Length;

                // Line Breaks are detected by the text store
                //if (this.LastLineBreak != null)
                {
                    // Text Height
                    desiredHeight += textElement.TextHeight;       // Increments Desired Height

                    // Visual Position
                    visualLineNumber++;                            // Sets current visual line (visual line collection)
                }

                // Text Position
                var textPosition = new TextElementPosition(visualBounds, characterOffset, 0, 0, visualLineNumber, 0);

                // Next Element
                return new SimpleTextElement(textElement, textPosition, properties, cachedText);
            }
        }

        public SimpleTextEditorFormatter(SimpleTextStore textStore, SimpleTextVisualInputData visualInputData)
        {
            _formatter = TextFormatter.Create(TextFormattingMode.Display);
            _visualInputData = visualInputData;
            _emptyTextStore = new SimpleTextStore(visualInputData);
            _textStore = textStore;
            _textRunCache = new TextRunCache();
            _mouseData = new MouseData();
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
            _textRunCache.Change(offset, additionLength, removalLength);
        }

        /// <summary>
        /// Invalidates entire contents of cache, which will "re-draw" the formatted text.
        /// </summary>
        public void InvalidateCache()
        {
            _textRunCache.Invalidate();
        }

        public void SetMouseInfo(MouseData mouseData)
        {
            _mouseData = mouseData;
        }

        public SimpleTextVisualOutputData MeasureText(Size constraint)
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

            var measurementData = new MeasurementData();

            var propertySet = _textStore.GetCurrentTextProperties();
            var properties = _visualInputData.GetParagraphProperties(propertySet);

            // Format each line of text from the text store and draw it. EOL character requires the extra <= pass
            // (I think) which accounts for the extra "character"
            //
            while (measurementData.CharacterOffset <= _textStore.GetLength())
            {
                TextLineBreak? lastLineBreak = null;

                // First Pass Measurement
                //
                var textElement = _formatter.FormatLine(_textStore,                             // TextStore sub-class
                                                        measurementData.CharacterOffset,        // Character offset to ITextSource
                                                        constraint.Width,                       // UI Width
                                                        properties,                             // Visual Properties (Basic)
                                                        lastLineBreak,                          // Last Line Break
                                                        _textRunCache);                         // TextRunCache (MSFT) stores output of formatter

                // Process First Pass
                var nextElement = ProcessLineElement(_textStore, textElement, measurementData);

                // Check mouse overlap
                //if (_mouseData.IsSet())
                //{
                //    // UI Overlap Detected
                //    if (nextElement.Position.VisualBounds.IntersectsWith(_mouseData.SelectionBounds))
                //    {
                //        _textStore.SelectTextProperties(TextPropertySet.Highlighted);
                //        nextElement = ProcessLineElement(_textStore, textElement, measurementData);
                //        _textStore.SelectTextProperties(TextPropertySet.Normal);
                //    }
                //}

                measurementData.CommitElement(nextElement);
            }

            return new SimpleTextVisualOutputData(measurementData.VisualElements,
                                                  constraint,
                                                  new Size(measurementData.DesiredWidth, measurementData.DesiredHeight),
                                                  _textStore.GetLength());
        }

        // Completes the measurement process by calculating the caret bounds
        public Rect CalculateCaretBounds(SimpleTextVisualOutputData lastOutput, Size constraint)
        {
            var lineHeight = lastOutput.VisualElements.Max(x => x.Element.TextHeight);

            // Measure Caret while we're here (check for empty text)
            if (lineHeight == 0)
                return Rect.Empty;

            // This is AHEAD BY ONE!
            var caretPosition = _textStore.GetCaretPosition();
            var result = new Rect();

            // Need to locate the glyph run boxes where the caret position lies
            foreach (var textElement in lastOutput.VisualElements)
            {
                var glyphRuns = textElement.Element
                                           .GetIndexedGlyphRuns()
                                           .Where(x => x.TextSourceCharacterIndex < caretPosition &&
                                                       x.TextSourceCharacterIndex + x.TextSourceLength >= caretPosition);

                // These glyphs trap the caret
                foreach (var glyphRun in glyphRuns)
                {
                    result.X = Math.Max(result.X, textElement.Position.VisualBounds.X + textElement.Element.WidthIncludingTrailingWhitespace);
                    result.Y = (textElement.Position.VisualLineNumber - 1) * textElement.Element.TextHeight;
                    result.Width = 2;
                    result.Height = textElement.Element.TextHeight;
                }
            }

            if (result == Rect.Empty)
            {
                result.X = 0;
                result.Y = lineHeight * (lastOutput.VisualElements.Count() - 1);
                result.Width = 2;
                result.Height = lineHeight;
            }

            return result;
        }

        // Processes current formatted text; and sets MeasurementData accordingly. Returns false if there is another pass needed (this indicates
        // UI overlap).
        //
        private SimpleTextElement ProcessLineElement(SimpleTextStore textStore, TextLine textElement, MeasurementData measurementData)
        {
            // Procedure:  How did the TextFormatter know where to put the element? Where is the "current" visual UI position?
            //
            // 1) Calculate this from the text element's bounding boxes for the text
            // 2) Interpret the "line break" output of the formatter
            // 3) Update the desired control height
            //

            // Set MeasurementData (updates all parameters, and returns the element's UI location)
            var visualBounds = measurementData.MeasureElement(textElement);

            // Add Next Element
            var nextElement = measurementData.BuildElement(visualBounds,
                                                           textElement,
                                                           textStore.Get().GetSubString(measurementData.CharacterOffset, textElement.Length - 1),
                                                           GetCurrentTextProperties());

            return nextElement;
        }

        private SimpleTextRunProperties GetCurrentTextProperties()
        {
            var propertySet = _textStore.GetCurrentTextProperties();
            var properties = _visualInputData.GetProperties(propertySet);

            return properties;
        }
        private SimpleTextParagraphProperties GetCurrentParagraphProperties()
        {
            var propertySet = _textStore.GetCurrentTextProperties();
            var properties = _visualInputData.GetParagraphProperties(propertySet);

            return properties;
        }
    }
}
