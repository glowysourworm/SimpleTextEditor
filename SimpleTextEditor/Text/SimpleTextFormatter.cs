using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

using SimpleTextEditor.Model;
using SimpleTextEditor.Text.Formatting;
using SimpleTextEditor.Text.Interface;
using SimpleTextEditor.Text.Source.Interface;
using SimpleTextEditor.Text.Visualization;
using SimpleTextEditor.Text.Visualization.Element;

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

            var builder = new VisualOutputBuilder(_textSource, _visualInputData.ConstraintSize);
            TextLineBreak? lastLineBreak = null;

            // MSFT Advance Text Formatting: This can be tracked after it's understood how to use the EOL!
            //
            // 1) EOL text elements are added for each EOL character ('\r')
            // 2) EOP text elements are added ONLY for paragraph breaks, (AND) (ONE EXTRA ITERATION) (see SimpleTextRunProvider.cs)
            //
            while (builder.BackendOffset <= _textSource.GetLength())
            {
                // MSFT Advanced Text Formatting (Makes multiple calls to ITextRunProvider)
                //
                var textElement = _formatter.FormatLine(_textRunProvider as TextSource,               // TextStore sub-class
                                                        builder.Offset,                               // Character offset to ITextSource
                                                        _visualInputData.ConstraintSize.Width,        // UI Width
                                                        _visualInputData.DefaultParagraphProperties,  // Visual Properties (Default Properties)
                                                        lastLineBreak /*,                             // Last Line Break
                                                        _textRunCache*/);                             // TextRunCache (MSFT) stores output of formatter

                // Visual Size / Offset
                builder.UpdateVisualSize(textElement);

                // MSFT "Advanced" Text Formatting:  I get to complain here because I've put in
                // time to figure this out! ^_^
                //
                // The Off-By-One issue is fairly "epic" because the return value from the formatter
                // does not follow the text source all the time (!!!) There are extra characters implied
                // where the source did not put them! 
                //
                // So, the EOP is one to watch for. It seems like this is the one causing issues. There
                // is no actual character. So, follow the ITextRunProvider implementation and try to be
                // sure you've accounted for every character.
                //
                // If you're stuck and need to verify, you must count your own EOL characters, each time!
                // So, any extra length must be MSFT's backend.
                // 
                // ====
                // Character Length (with EOP removed) (THIS MUST BE ACCURATE!)
                //
                // EOP:  Any element that is the TextEndOfParagraph (will also be TextEndOfLine)
                // EOL:  Any element that is TextEndOfLine (we will verify in our text source)
                //

                if (lastLineBreak != null)
                    throw new Exception("Unhandled Line break detected!");

                foreach (var span in textElement.GetTextRunSpans())
                {
                    // BackendOffset should follow the MSFT output
                    builder.BackendOffset += span.Length;

                    // VisualTextCollection -> BeginParagraph() -> BeginLine() -> Add Elements... -> EndLine() -> EndParagraph(...)
                    //
                    if (!builder.VisualText.AddingParagraph())
                    {
                        builder.VisualText.BeginParagraph();
                    }
                    if (!builder.VisualText.AddingLine(builder.ParagraphIndex + 1) &&
                        span.Value is not TextEndOfParagraph)
                    {
                        builder.VisualText.BeginLine();
                    }

                    // MSFT BUG!!!  span.Length (may equal) textElement.Length 
                    //
                    // Issue:  This seems to be true even for individual span segments when
                    //         they are the only segment! 
                    //
                    // Fix:    For proper character offset, just remove EOL characters when
                    //         adding it to the builder's offset.
                    //

                    // EOP
                    //
                    // NOTE*** EOL may also be EOP
                    //
                    if (span.Value is TextEndOfParagraph)
                    {
                        // PARAGRAPH INCREMENTERS
                        builder.ParagraphIndex++;

                        // VisualCollection -> EndParagraph( EOP )
                        builder.VisualText.EndParagraph(span, new TextEndOfParagraphElement());
                    }

                    // EOL
                    //
                    else if (span.Value is TextEndOfLine)
                    {
                        builder.VisualText.Add(span, new TextEndOfLineElement());

                        // VAILDATION: Does not include the '\r' character
                        var lineLength = builder.Offset - builder.LineOffset;

                        // OFFSET (this is the '\r' character)
                        builder.Offset++;

                        // LINE INCREMENTERS
                        builder.LineOffset = builder.Offset;
                        builder.LineIndex++;

                        // VisualCollection -> EndLine() (VALIDATE)
                        builder.VisualText.EndLine(lineLength);
                    }

                    // NOTE*** If there has already been an EOL / EOP, then the VisualTextCollection will
                    //         catch the error.
                    //
                    else if (span.Value is TextCharacters)
                    {
                        //var countEOL = span.Value.Count(x => x.Value is TextEndOfLine);
                        //var countEOP = textElement.GetTextRunSpans().Count(x => x.Value is TextEndOfParagraph);
                        //var ourEOLs = _textSource.GetString(builder.Offset, textElement.Length).Count(x => x == '\r');
                        //var characterLength = span.Length;

                        if (span.Value is TextEndOfLine ||
                            span.Value is TextEndOfParagraph)
                            throw new Exception("EOL / EOP formatting error! End of line characters are being treated as text!");

                        if ((builder.Offset + span.Length) >= _textSource.GetLength())
                            throw new Exception("Trying to add visual characters past the end of the text source!");

                        // Text Position 
                        var position = new TextPosition(builder.Offset,                                        // Offsets
                                                        builder.Offset - builder.LineOffset + 1,               // Column Number
                                                        builder.LineIndex + 1,
                                                        builder.ParagraphIndex + 1);

                        // Character Boundaries:  These were easier to do as we build them, rather than during user interaction.
                        //                        THESE MUST BE ONLY THE GLYPHS THAT RELATE TO THE ACTUAL TEXT SOURCE!
                        //
                        var characterBounds = textElement.GetIndexedGlyphRuns()
                                                         .SelectMany(x => x.GlyphRun.AdvanceWidths)
                                                         .Skip(builder.Offset - builder.LineOffset)
                                                         .Take(span.Length)
                                                         .Aggregate(new List<Rect>(), (list, nextWidth) =>
                                                         {
                                                             // Previous Offset-X
                                                             var currentWidth = list.Sum(x => x.Width);

                                                             // Current Character Bounds
                                                             list.Add(new Rect(currentWidth, builder.VisualOffset.Y, nextWidth, textElement.TextHeight));

                                                             // Re-iterate
                                                             return list;
                                                         })
                                                         .ToArray();

                        // TEXT LENGTH! The formatter's result has other characters that are EOL / EOP characters. We must
                        //              validate the proper character length and store it here! These character bounds will
                        //              be treated as the actual text length!
                        //
                        if (characterBounds.Length != span.Length)
                            throw new Exception("Character bounds improper calculation!");

                        // Visual Bounds
                        var textVisualBounds = characterBounds.Aggregate(new Rect(), (rect, nextRect) =>
                        {
                            rect.Union(nextRect);
                            return rect;
                        });

                        // Text Element:  The TextLine has a Draw() method for WPF. It may be that we end
                        //                up taking the glyphs apart and storing them to recall in the 
                        //                OnRender instead.
                        //
                        var element = new TextElement(characterBounds, textVisualBounds, position);

                        // VisualCollection -> Add (element)
                        builder.VisualText.Add(textElement, element);

                        // CHARACTERS (ARE) SPANS! (Spans can include EOP / EOL, but these do not index the ITextSource)
                        builder.Offset += span.Length;
                    }

                    if (span.Value is not TextEndOfLine &&
                        span.Value is not TextEndOfParagraph &&
                        span.Value is not TextCharacters)
                        throw new Exception("Unhandled TextRun Type:  SimpleTextFormatter");
                }
            }

            // Final Validation
            builder.Validate();

            // Build Output
            _lastOutputData = builder.BuildOutput();

            _invalid = false;
        }
    }
}
