using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

using SimpleTextEditor.Component;
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

        // Input data to the formatter
        readonly SimpleTextVisualInputData _visualInputData;

        // Empty text store used for formatting
        readonly SimpleTextStore _emptyTextStore;

        public SimpleTextEditorFormatter(SimpleTextVisualInputData visualInputData)
        {
            _formatter = TextFormatter.Create(TextFormattingMode.Ideal);
            _visualInputData = visualInputData;
            _emptyTextStore = new SimpleTextStore(SimpleTextVisualInputData.PixelsPerDip, visualInputData.TextProperties);
        }

        public SimpleTextVisualOutputData MeasureText(SimpleTextStore textStore, Size constraint)
        {
            var characterPosition = 0;
            var characterOffset = new Point(0, 0);
            var lineHeight = 0.0D;
            var controlWidth = constraint.Width;
            var desiredHeight = 0.0D;
            var desiredWidth = 0.0D;
            var linePosition = new Point();
            var textLines = new List<SimpleTextLine>();
            var caretBounds = new Rect();
            var lineNumber = 0;
            var paragraphNumber = 1;

            TextLineBreak lastLineBreak = null;

            // Format each line of text from the text store and draw it. EOL character requires the extra <= pass
            // (I think) which accounts for the extra "character"
            //
            while (characterPosition <= textStore.GetLength())
            {
                // Line Number
                lineNumber++;

                // Create a textline from the text store using the TextFormatter object. (CUSTOMIZE!!!)
                var textLine = _formatter.FormatLine(textStore, characterPosition, controlWidth, _visualInputData.ParagraphProperties, lastLineBreak);

                lastLineBreak = textLine.GetTextLineBreak();

                // NOT SURE ABOUT THIS ONE!
                if (lastLineBreak != null)
                    paragraphNumber++;

                // Measure the text line
                desiredHeight += textLine.TextHeight;

                if (textLine.WidthIncludingTrailingWhitespace > desiredWidth)
                    desiredWidth = textLine.WidthIncludingTrailingWhitespace;

                // Use the text lines to resize the rendered image
                // Update the index position in the text store.
                characterPosition += textLine.Length;
                characterOffset.X = textLine.WidthIncludingTrailingWhitespace;                 // Used to track the caret
                characterOffset.Y += textLine.TextHeight;

                // Update the line position coordinate for the displayed line.
                linePosition.Y += textLine.TextHeight;

                // (This is essentially for the caret)
                lineHeight = textLine.TextHeight;

                // Create text position for the line
                var position = new TextPosition(characterPosition - textLine.Length, lineNumber, 0, lineNumber, paragraphNumber);

                // Use these to render w/o re-formatting (NOTE*** THE TEXTLINE IS RECEIVING AN EXTRA CHARACTER?!)
                textLines.Add(new SimpleTextLine(textLine,
                                                 position,
                                                 _visualInputData.ParagraphProperties,
                                                 textStore.Get().GetSubString(characterPosition - textLine.Length, textLine.Length - 1)));
            }

            // Measure Caret while we're here (check for empty text)
            if (lineHeight == 0)
                lineHeight = _formatter.FormatLine(_emptyTextStore, 0, constraint.Width, _visualInputData.ParagraphProperties, null).TextHeight;

            caretBounds.X = characterOffset.X;
            caretBounds.Y = Math.Max(characterOffset.Y - lineHeight, 0);
            caretBounds.Width = 2;
            caretBounds.Height = lineHeight;

            return new SimpleTextVisualOutputData(textLines, constraint, new Size(desiredWidth, desiredHeight), caretBounds, textStore.GetLength());
        }

        /*
        /// <summary>
        /// Returns rendering of the text for the target constraint. Will re-render if there
        /// have been any changes.
        /// </summary>
        public RenderTargetBitmap GetRendering(Size constraint)
        {
            if (constraint.Width == 0 ||
                double.IsNaN(constraint.Width) ||
                constraint.Height == 0 ||
                double.IsNaN(constraint.Height))
                throw new ArgumentException("Invalid constraint size (NaN, or Zero):  SimpleTextEditorCore.cs");

            var visual = new DrawingVisual();

            // Text Area:  Minimum width of the control space; and height is at least the caret height
            var width = Math.Max(_textLines.Sum(line => line.WidthIncludingTrailingWhitespace), constraint.Width);
            var height = NumberExtension.Max(_textLines.Sum(line => line.TextHeight), _caretRenderBounds.Height, constraint.Height);

            // Draw the lines to a WPF Visual as they're calculated
            using (var context = visual.RenderOpen())
            {
                var textPosition = new Point();

                foreach (var textLine in _textLines)
                {
                    textPosition.X = textLine.Start;
                    textPosition.Y += textLine.TextHeight;
                    textLine.Draw(context, textPosition, InvertAxes.None);
                }
            }

            _renderingBitmap = new RenderTargetBitmap((int)width,
                                                      (int)height,
                                                      _renderingBitmapDPI,
                                                      _renderingBitmapDPI,
                                                      PixelFormats.Default);
            // Render the visual!
            _renderingBitmap.Render(visual);

            return _renderingBitmap;
        }
        */
    }
}
