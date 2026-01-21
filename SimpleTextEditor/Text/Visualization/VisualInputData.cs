using System.Globalization;
using System.Windows;
using System.Windows.Media;

using SimpleTextEditor.Model.Configuration;
using SimpleTextEditor.Text.Visualization.Properties;

namespace SimpleTextEditor.Text.Visualization
{
    /// <summary>
    /// Visual input data - which may link directly to source indices. For now, this is just the
    /// MSFT input classes for the TextFormatter, and a few extra things for our renderer.
    /// </summary>
    public class VisualInputData
    {
        // Recommended setting for pixelsPerDip
        public const double PixelsPerDip = 1.25D;

        public Size ConstraintSize { get; }
        public double LineHeight { get; }
        public double Indent { get; }

        public SimpleTextRunProperties DefaultProperties { get; }
        public SimpleTextRunProperties DefaultHighlightProperties { get; }

        public SimpleTextParagraphProperties DefaultParagraphProperties { get; }

        public VisualInputData(TextEditorConfiguration configuration, Size constraintSize)
        {
            this.ConstraintSize = constraintSize;
            this.LineHeight = 1.0D;
            this.Indent = 0.0D;

            // Typeface
            var typeface = CreateTypeface(configuration.DefaultProperties);
            var typefaceHighlight = CreateTypeface(configuration.DefaultHighlightProperties);

            // TextRun Properties
            this.DefaultProperties = CreateProperties(typeface, configuration.DefaultProperties);

            // TextRun Properties (for highlighted text)
            this.DefaultHighlightProperties = CreateProperties(typefaceHighlight, configuration.DefaultHighlightProperties);

            // Paragraph Properties
            this.DefaultParagraphProperties = CreateParagraphProperties(configuration.TextWrapping, this.DefaultProperties);
        }

        private Typeface CreateTypeface(TextProperties properties)
        {
            return new Typeface(new FontFamily(properties.Font),
                                               properties.FontStyle,
                                               properties.FontWeight,
                                               FontStretches.Normal,
                                               new FontFamily(properties.Font));
        }

        private SimpleTextRunProperties CreateProperties(Typeface typeface, TextProperties properties)
        {
            return new SimpleTextRunProperties(typeface,
                                               PixelsPerDip,
                                               properties.FontSize,
                                               properties.FontSize,
                                               null,
                                               properties.Foreground,
                                               properties.Background,
                                               BaselineAlignment.Baseline,
                                               CultureInfo.CurrentUICulture);
        }

        private SimpleTextParagraphProperties CreateParagraphProperties(TextWrapping textWrapping, SimpleTextRunProperties properties)
        {
            return new SimpleTextParagraphProperties(FlowDirection.LeftToRight,
                                                     TextAlignment.Left,
                                                     false,
                                                     false,
                                                     properties,
                                                     textWrapping,
                                                     this.LineHeight,
                                                     this.Indent);
        }
    }
}
