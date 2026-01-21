using System.Globalization;
using System.Windows;
using System.Windows.Media;

using SimpleTextEditor.Text.Visualization.Interface;

using SimpleWpf.SimpleCollections.Collection;

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
    public class VisualInputData
    {
        // Recommended setting for pixelsPerDip
        public const double PixelsPerDip = 1.25D;

        public Size ConstraintSize { get; }
        public double LineHeight { get; }
        public double Indent { get; }

        SimpleDictionary<TextPropertySet, ITextProperties> _propertyDict;

        public VisualInputData(Size constraintSize,
                                         FontFamily fontFamily,
                                         double fontSize,
                                         Brush foreground,
                                         Brush background,
                                         Brush highlightBrush,
                                         Brush highlightBackground,
                                         TextWrapping textWrapping)
        {
            this.ConstraintSize = constraintSize;
            this.LineHeight = 1.0D;
            this.Indent = 0.0D;

            _propertyDict = new SimpleDictionary<TextPropertySet, ITextProperties>();

            // Typeface
            var typeface = new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

            // TextRun Properties
            var properties = new SimpleTextRunProperties(typeface, PixelsPerDip, fontSize, fontSize, null,
                                                             foreground, background,
                                                             BaselineAlignment.Baseline, CultureInfo.CurrentUICulture);

            // TextRun Properties (for highlighted text)
            var highlightProperties = new SimpleTextRunProperties(typeface, PixelsPerDip, fontSize, fontSize, null,
                                                                      highlightBrush, highlightBackground,
                                                                      BaselineAlignment.Baseline, CultureInfo.CurrentUICulture);

            // Paragraph Properties
            var paragraphProperties = new SimpleTextParagraphProperties(FlowDirection.LeftToRight, TextAlignment.Left, false, false,
                                                                properties, textWrapping, this.LineHeight, this.Indent);

            var paragraphHighlightedProperties = new SimpleTextParagraphProperties(FlowDirection.LeftToRight, TextAlignment.Left, false, false,
                                                                         highlightProperties, textWrapping, this.LineHeight, this.Indent);

            _propertyDict.Add(TextPropertySet.Normal, new SimpleTextProperties(properties, paragraphProperties));
            _propertyDict.Add(TextPropertySet.Highlighted, new SimpleTextProperties(highlightProperties, paragraphHighlightedProperties));
        }

        public ITextProperties GetProperties(TextPropertySet propertySet)
        {
            return _propertyDict[propertySet];
        }
    }
}
