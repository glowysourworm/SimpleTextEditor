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
            /// <summary>
            /// Absolute text source offset for the character
            /// </summary>
            public int CharacterOffset;

            /// <summary>
            /// Current visual line offset for the current element. (This will index into the visual lines collection)
            /// </summary>
            public int VisualLineOffset;

            /// <summary>
            /// Current paragraph number for the current element
            /// </summary>
            public int ParagraphNumber;

            /// <summary>
            /// Visual line height (MSFT typography metric)
            /// </summary>
            public double VisualLineHeight;

            /// <summary>
            /// Desired total size of the output in UI coordinates
            /// </summary>
            public Size DesiredSize;

            /// <summary>
            /// Desired UI size of the caret - given its current offset in
            /// the text source.
            /// </summary>
            public Rect CaretBounds;

            /// <summary>
            /// Last line break found by the formatter. This should be related to text wrapping. But, there could
            /// be other implied meanings from MSFT.
            /// </summary>
            public TextLineBreak? LastLineBreak;

            /// <summary>
            /// Primary output of a measurement pass
            /// </summary>
            public List<SimpleTextElement> VisualElements;

            /// <summary>
            /// Builds a text position instance from the current data
            /// </summary>
            public TextPosition CreateTextPosition()
            {
                return new TextPosition(this.CharacterOffset, this.VisualLineOffset + 1, 0, this.VisualLineOffset + 1, this.ParagraphNumber);
            }

            public MeasurementData()
            {
                this.VisualElements = new List<SimpleTextElement>();
                this.LastLineBreak = null;
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
                var firstResult = ProcessLineElement(_textStore, textElement, measurementData);

                // UI Overlap Detected
                if (!firstResult)
                {
                    // TODO
                }
            }

            // Measure Caret
            ProcessCaretMeasurement(measurementData, constraint);

            return new SimpleTextVisualOutputData(measurementData.VisualElements,
                                                  constraint,
                                                  measurementData.DesiredSize,
                                                  measurementData.CaretBounds,
                                                  _textStore.GetLength());
        }

        // Processes current formatted text; and sets MeasurementData accordingly. Returns false if there is another pass needed (this indicates
        // UI overlap).
        //
        private bool ProcessLineElement(SimpleTextStore textStore, TextLine textElement, MeasurementData measurementData)
        {
            // Procedure:  How did the TextFormatter know where to put the element? Where is the "current" visual UI position?
            //
            // 1) Calculate this from the text element's bounding boxes for the text
            // 2) Interpret the "line break" output of the formatter
            // 3) Update the desired control height
            //

            //Rect? boundingBox = null;

            // The indexed GlyphRun elements appear to be the final result
            //
            //foreach (var indexedGlyphRun in textElement.GetIndexedGlyphRuns())
            //{
            //    foreach (var textBounds in textElement.GetTextBounds(indexedGlyphRun.TextSourceCharacterIndex, indexedGlyphRun.TextSourceLength))
            //    {
            //        if (boundingBox == null)
            //            boundingBox = textBounds.Rectangle;
            //        else
            //            boundingBox.Value.Union(textBounds.Rectangle);
            //    }
            //}

            //measurementData.LastLineBreak = textElement.GetTextLineBreak();

            // Line Breaks are detected by the text store
            if (measurementData.LastLineBreak != null)
            {
                // Update the line position coordinate for the displayed line.
            }

            // Text Height
            measurementData.DesiredSize.Height += textElement.TextHeight;
            measurementData.VisualLineHeight = textElement.TextHeight;

            // TextWidth:  (Line Break (?) (Not likely related to text-wrapping))
            if (textElement.WidthIncludingTrailingWhitespace > measurementData.DesiredSize.Width)
                measurementData.DesiredSize.Width = textElement.WidthIncludingTrailingWhitespace;

            // Character Offset
            measurementData.CharacterOffset += textElement.Length;                                      // Advance Character Offset (text source)

            // Visual Position
            measurementData.VisualLineOffset = 0;                                                       // Sets current visual line (visual line collection)

            // Create text position for the line
            var position = measurementData.CreateTextPosition();
            var propertySet = textStore.GetCurrentTextProperties();
            var textProperties = _visualInputData.GetProperties(propertySet);

            // Use these to render w/o re-formatting (NOTE*** THE TEXTLINE IS RECEIVING AN EXTRA CHARACTER?!)
            var nextElement = new SimpleTextElement(textElement,
                                                    position,
                                                    textProperties,
                                                    textStore.Get().GetSubString(measurementData.CharacterOffset - textElement.Length, textElement.Length - 1));

            // Add Next Element
            measurementData.VisualElements.Add(nextElement);

            return true;
        }

        // Completes the measurement process by calculating the caret bounds
        private void ProcessCaretMeasurement(MeasurementData measurementData, Size constraint)
        {
            // Measure Caret while we're here (check for empty text)
            if (measurementData.VisualLineHeight == 0)
                measurementData.VisualLineHeight = _formatter.FormatLine(_emptyTextStore, 0, constraint.Width, GetCurrentParagraphProperties(), null).TextHeight;

            measurementData.CaretBounds.X = 0;
            measurementData.CaretBounds.Y = 0;
            measurementData.CaretBounds.Width = 2;
            measurementData.CaretBounds.Height = measurementData.VisualLineHeight;
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
