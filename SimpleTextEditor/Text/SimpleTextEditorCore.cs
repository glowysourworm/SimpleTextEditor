using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;

using SimpleWpf.Extensions;

namespace SimpleTextEditor.Text
{
    public class SimpleTextEditorCore
    {
        // Recommended setting for pixelsPerDip
        protected const double PixelsPerDip = 1.25D;

        // (see MSFT Advanced Text Formatting)
        TextFormatter _formatter;
        SimpleTextStore _textStore;
        SimpleTextStore _emptyTextStore;
        SimpleTextRunProperties _textProperties;
        SimpleTextParagraphProperties _paragraphProperties;

        RenderTargetBitmap _renderingBitmap;
        double _renderingBitmapDPI = 96.0D;

        double _lineHeight = 1.0D;
        double _indent = 0.0D;

        Rect _caretRenderBounds = new Rect();

        List<TextLine> _textLines;
        bool _isDirty = true;

        public SimpleTextEditorCore(FontFamily fontFamily,
                                    double fontSize,
                                    Brush foreground,
                                    Brush background,
                                    TextWrapping textWrapping)
        {
            _formatter = TextFormatter.Create(TextFormattingMode.Ideal);

            var typeface = new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

            _textProperties = new SimpleTextRunProperties(typeface, PixelsPerDip, fontSize, fontSize, null,
                                                          foreground, background,
                                                          BaselineAlignment.Baseline, CultureInfo.CurrentUICulture);

            _paragraphProperties = new SimpleTextParagraphProperties(FlowDirection.LeftToRight, TextAlignment.Left, false, false,
                                                                     _textProperties, textWrapping, _lineHeight, _indent);

            _textLines = new List<TextLine>();
            _textStore = new SimpleTextStore(_renderingBitmapDPI, _textProperties);
            _emptyTextStore = new SimpleTextStore(_renderingBitmapDPI, _textProperties);
            _textStore.Text = string.Empty;
            _emptyTextStore.Text = string.Empty;

            _renderingBitmap = new RenderTargetBitmap(100, 100, _renderingBitmapDPI, _renderingBitmapDPI, PixelFormats.Default);
        }

        public void SetText(string text)
        {
            _textStore.Text = text;
            _isDirty = true;
        }

        public void AppendText(string text)
        {
            _textStore.Text += text;
            _isDirty = true;
        }

        public bool Backspace()
        {
            if (_textStore.Text.Length > 0)
            {
                _textStore.Text = _textStore.Text.Substring(0, _textStore.Text.Length - 1);
                _isDirty = true;
                return true;
            }
            return false;
        }

        public void AppendChar(char character)
        {
            _textStore.Text += character;
            _isDirty = true;
        }

        /*
        public IEnumerable<TextLine> GetTextLines(ref Size constraint, out Rect caretRenderBounds)
        {
            _textLines.Clear();
            _isDirty = true;

            var characterPosition = 0;
            var characterOffset = new Point(0, 0);
            var lineHeight = 0.0D;
            var lineExtentHeight = 0.0D;
            var controlWidth = constraint.Width;
            var desiredHeight = 0.0D;
            var desiredWidth = 0.0D;
            var linePosition = new Point();

            TextLineBreak lastLineBreak = null;

            // Format each line of text from the text store and draw it.
            while (characterPosition < _textStore.Text.Length)
            {
                // Create a textline from the text store using the TextFormatter object. (CUSTOMIZE!!!)
                var textLine = _formatter.FormatLine(_textStore, characterPosition, controlWidth, _paragraphProperties, lastLineBreak);

                lastLineBreak = textLine.GetTextLineBreak();

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
                lineExtentHeight = textLine.Extent;

                // Use these to render w/o re-formatting
                _textLines.Add(textLine);
            }

            // Measure Caret while we're here (check for empty text)
            if (lineHeight == 0)
                lineHeight = _formatter.FormatLine(_emptyTextStore, 0, constraint.Width, _paragraphProperties, null).TextHeight;

            _caretRenderBounds.X = characterOffset.X;
            _caretRenderBounds.Y = Math.Max(characterOffset.Y - lineHeight, 0);
            _caretRenderBounds.Width = 2;
            _caretRenderBounds.Height = lineHeight;

            constraint.Width = desiredWidth;
            constraint.Height = desiredHeight;

            caretRenderBounds = _caretRenderBounds;

            return _textLines;
        }
        */

        public Size MeasureText(Size constraint)
        {
            _textLines.Clear();
            _isDirty = true;

            var characterPosition = 0;
            var characterOffset = new Point(0, 0);
            var lineHeight = 0.0D;
            var controlWidth = constraint.Width;
            var desiredHeight = 0.0D;
            var desiredWidth = 0.0D;
            var linePosition = new Point();

            TextLineBreak lastLineBreak = null;

            // Format each line of text from the text store and draw it.
            while (characterPosition < _textStore.Text.Length)
            {
                // Create a textline from the text store using the TextFormatter object. (CUSTOMIZE!!!)
                var textLine = _formatter.FormatLine(_textStore, characterPosition, controlWidth, _paragraphProperties, lastLineBreak);

                lastLineBreak = textLine.GetTextLineBreak();

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

                // Use these to render w/o re-formatting
                _textLines.Add(textLine);
            }

            // Measure Caret while we're here (check for empty text)
            if (lineHeight == 0)
                lineHeight = _formatter.FormatLine(_emptyTextStore, 0, constraint.Width, _paragraphProperties, null).TextHeight;

            _caretRenderBounds.X = characterOffset.X;
            _caretRenderBounds.Y = Math.Max(characterOffset.Y - lineHeight, 0);
            _caretRenderBounds.Width = 2;
            _caretRenderBounds.Height = lineHeight;

            return new Size(desiredWidth, desiredHeight);
        }

        public IEnumerable<TextLine> GetTextLines()
        {
            return _textLines;
        }

        public string GetText()
        {
            return _textStore.Text;
        }

        public Rect GetCaretRenderBounds()
        {
            return _caretRenderBounds;
        }

        /// <summary>
        /// Returns rendering of the text for the target constraint. Will re-render if there
        /// have been any changes.
        /// </summary>
        public RenderTargetBitmap GetRendering(Size constraint)
        {
            if (!_isDirty)
                return _renderingBitmap;

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

            _isDirty = false;

            return _renderingBitmap;
        }
    }
}
