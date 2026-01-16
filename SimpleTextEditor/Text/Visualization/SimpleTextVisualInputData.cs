using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace SimpleTextEditor.Text.Visualization
{
    /// <summary>
    /// This is temporary for setting all of the MSFT readonly text properties. Use Cases:  Treating
    /// portions of text as bold / highlighted / etc...
    /// </summary>
    public enum TextPropertySet
    {
        Normal = 0,
        Highlighted = 1
    }

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

        SimpleTextRunProperties _properties;
        SimpleTextRunProperties _highlightProperties;
        SimpleTextParagraphProperties _paragraphProperties;
        SimpleTextParagraphProperties _paragraphHighlightedProperties;

        public SimpleTextVisualInputData(FontFamily fontFamily,
                                         double fontSize,
                                         Brush foreground,
                                         Brush background,
                                         Brush highlightBrush,
                                         Brush highlightBackground,
                                         TextWrapping textWrapping)
        {
            this.LineHeight = 1.0D;
            this.Indent = 0.0D;

            // Typeface
            var typeface = new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

            // TextRun Properties
            _properties = new SimpleTextRunProperties(typeface, PixelsPerDip, fontSize, fontSize, null,
                                                             foreground, background,
                                                             BaselineAlignment.Baseline, CultureInfo.CurrentUICulture);

            // TextRun Properties (for highlighted text)
            _highlightProperties = new SimpleTextRunProperties(typeface, PixelsPerDip, fontSize, fontSize, null,
                                                                      highlightBrush, highlightBackground,
                                                                      BaselineAlignment.Baseline, CultureInfo.CurrentUICulture);

            // Paragraph Properties
            _paragraphProperties = new SimpleTextParagraphProperties(FlowDirection.LeftToRight, TextAlignment.Left, false, false,
                                                                _properties, textWrapping, this.LineHeight, this.Indent);

            _paragraphHighlightedProperties = new SimpleTextParagraphProperties(FlowDirection.LeftToRight, TextAlignment.Left, false, false,
                                                                         _highlightProperties, textWrapping, this.LineHeight, this.Indent);
        }

        public SimpleTextRunProperties GetProperties(TextPropertySet propertySet)
        {
            if (propertySet == TextPropertySet.Normal)
                return _properties;

            return _highlightProperties;
        }

        public SimpleTextParagraphProperties GetParagraphProperties(TextPropertySet propertySet)
        {
            if (propertySet == TextPropertySet.Normal)
                return _paragraphProperties;

            return _paragraphHighlightedProperties;
        }
    }
}
