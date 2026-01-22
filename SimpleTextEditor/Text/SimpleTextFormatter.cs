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
    public class SimpleTextFormatter : ITextFormatter
    {
        public bool IsInvalid { get { return _invalid; } }
        public bool IsInitialized { get { return _initialized; } }

        // (see MSFT Advanced Text Formatting)
        TextFormatter _formatter;

        VisualOutputData _lastOutputData;

        // Input data to the formatter
        VisualInputData _visualInputData;

        // Primary text store
        ITextSource _textSource;

        // Primary TextRun source
        ITextRunProvider _textRunProvider;

        // Supposedly helps improve performance
        TextRunCache _textRunCache;

        // Keep track of control size. If it doesen't change, we don't need to re-call the formatter's measure method.
        Size _constraintSize;
        bool _initialized;
        bool _invalid;

        public SimpleTextFormatter()
        {
            _initialized = false;
            _invalid = true;
        }
        public void Initialize(ITextRunProvider textRunProvider, ITextSource textSource, VisualInputData visualInputData)
        {
            if (_initialized)
                throw new Exception("SimpleTextEditorFormatter already initialized.");

            _formatter = TextFormatter.Create(TextFormattingMode.Display);
            _visualInputData = visualInputData;
            _textSource = textSource;
            _textRunProvider = textRunProvider;
            _textRunCache = new TextRunCache();

            _initialized = true;
        }
        public VisualOutputData GetOutput()
        {
            if (!_initialized)
                throw new Exception("SimpleTextEditorFormatter must be initialized prior to calling other methods.");

            if (_lastOutputData == null)
                Run();

            if (_invalid)
                Run();

            return _lastOutputData;
        }
        public void UpdateSize(Size contorlSize)
        {
            if (!_initialized)
                throw new Exception("SimpleTextEditorFormatter must be initialized prior to calling other methods.");

            _constraintSize = contorlSize;
            _invalid = true;
        }
        public void Invalidate()
        {
            if (!_initialized)
                throw new Exception("SimpleTextEditorFormatter must be initialized prior to calling other methods.");

            _textRunCache.Invalidate();
            _invalid = true;
        }
        public void Invalidate(int startIndex, int additionLength, int removalLength)
        {
            if (!_initialized)
                throw new Exception("SimpleTextEditorFormatter must be initialized prior to calling other methods.");

            _textRunCache.Invalidate();
            _invalid = true;
        }

        private void Run()
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
            var lastLineCharacterOffset = 0;
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

                // First Pass Measurement (MAKES MULTIPLE CALLS TO TextSource!!!)
                //
                textElement = _formatter.FormatLine(_textRunProvider as TextSource,               // TextStore sub-class
                                                    characterOffset,                              // Character offset to ITextSource
                                                    _visualInputData.ConstraintSize.Width,        // UI Width
                                                    _visualInputData.DefaultParagraphProperties,  // Visual Properties (Default Properties)
                                                    lastLineBreak/*,                              // Last Line Break
                                                    _textRunCache*/);                             // TextRunCache (MSFT) stores output of formatter

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
                var endAppend = textEOP > 0 ? AppendPosition.Append : AppendPosition.None;

                // Text Position (AppendPosition is only used for the caret)
                var startPosition = new TextPosition(characterOffset,                                   // Offsets
                                                     textLineIndex,
                                                     lastLineCharacterOffset,
                                                     characterOffset - lastLineCharacterOffset + 1,     // Numbers
                                                     textLineIndex + 1,
                                                     textParagraphIndex + 1,
                                                     textElements.Count,                                // Element Index
                                                     AppendPosition.None);                              // Append Position

                var endPosition = TextPosition.FromLine(startPosition, characterOffset + textElement.Length - 1, characterOffset - lastLineCharacterOffset + 1);

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

                if (textEOL > 0)
                    lastLineCharacterOffset += characterOffset;
            }


            _lastOutputData = new VisualOutputData(textElements,
                                                   _visualInputData.ConstraintSize,
                                                   new Size(desiredWidth, desiredHeight),
                                                   _textSource.GetLength());

            _invalid = false;
        }
    }
}
