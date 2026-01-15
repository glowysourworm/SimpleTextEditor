using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace SimpleTextEditor.Text.Visualization
{
    /// <summary>
    /// Visual input data - which may link directly to source indices. For now, this is just the
    /// MSFT input classes for the TextFormatter, and a few extra things for our renderer.
    /// </summary>
    public class SimpleTextVisualInputData
    {
        // Recommended setting for pixelsPerDip
        public const double PixelsPerDip = 1.25D;

        public double LineHeight { get; }
        public double Indent { get; }
        public Rect CaretRenderBounds { get; }
        public SimpleTextRunProperties TextProperties { get; }
        public SimpleTextParagraphProperties ParagraphProperties { get; }

        public SimpleTextVisualInputData(FontFamily fontFamily,
                                         double fontSize,
                                         Brush foreground,
                                         Brush background,
                                         TextWrapping textWrapping)
        {
            this.LineHeight = 1.0D;
            this.Indent = 0.0D;
            this.CaretRenderBounds = new Rect();

            // Typeface
            var typeface = new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

            // TextRun Properties
            this.TextProperties = new SimpleTextRunProperties(typeface, PixelsPerDip, fontSize, fontSize, null,
                                                             foreground, background,
                                                             BaselineAlignment.Baseline, CultureInfo.CurrentUICulture);

            // Paragraph Properties
            this.ParagraphProperties = new SimpleTextParagraphProperties(FlowDirection.LeftToRight, TextAlignment.Left, false, false,
                                                                        this.TextProperties, textWrapping, this.LineHeight, this.Indent);
        }
    }
}
