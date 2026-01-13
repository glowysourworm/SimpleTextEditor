using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace SimpleTextEditor.Controls
{
    public class SimpleTextRunProperties : TextRunProperties
    {
        private Typeface _typeface;
        private double _emSize;
        private double _emHintingSize;
        private TextDecorationCollection _textDecorations;
        private Brush _foregroundBrush;
        private Brush _backgroundBrush;
        private BaselineAlignment _baselineAlignment;
        private CultureInfo _culture;

        #region (public) Properties
        public override Typeface Typeface
        {
            get { return _typeface; }
        }
        public override double FontRenderingEmSize
        {
            get { return _emSize; }
        }
        public override double FontHintingEmSize
        {
            get { return _emHintingSize; }
        }
        public override TextDecorationCollection TextDecorations
        {
            get { return _textDecorations; }
        }
        public override Brush ForegroundBrush
        {
            get { return _foregroundBrush; }
        }
        public override Brush BackgroundBrush
        {
            get { return _backgroundBrush; }
        }
        public override BaselineAlignment BaselineAlignment
        {
            get { return _baselineAlignment; }
        }
        public override CultureInfo CultureInfo
        {
            get { return _culture; }
        }
        public override TextRunTypographyProperties TypographyProperties
        {
            get { return null; }
        }
        public override TextEffectCollection TextEffects
        {
            get { return null; }
        }
        public override NumberSubstitution NumberSubstitution
        {
            get { return null; }
        }
        #endregion

        public SimpleTextRunProperties(
                    Typeface typeface,
                    double pixelsPerDip,
                    double emSize,
                    double emHintingSize,
                    TextDecorationCollection textDecorations,
                    Brush forgroundBrush,
                    Brush backgroundBrush,
                    BaselineAlignment baselineAlignment,
                    CultureInfo culture)
        {
            if (typeface == null)
                throw new ArgumentNullException("typeface");

            ValidateCulture(culture);

            PixelsPerDip = pixelsPerDip;
            _typeface = typeface;
            _emSize = emSize;
            _emHintingSize = emHintingSize;
            _textDecorations = textDecorations;
            _foregroundBrush = forgroundBrush;
            _backgroundBrush = backgroundBrush;
            _baselineAlignment = baselineAlignment;
            _culture = culture;
        }

        private static void ValidateCulture(CultureInfo culture)
        {
            if (culture == null)
                throw new ArgumentNullException("culture");

            if (culture.IsNeutralCulture || culture.Equals(CultureInfo.InvariantCulture))
                throw new ArgumentException("Specific Culture Required", "culture");
        }

        private static void ValidateFontSize(double emSize)
        {
            if (emSize <= 0)
                throw new ArgumentOutOfRangeException("emSize", "Parameter Must Be Greater Than Zero.");

            //if (emSize > MaxFontEmSize)
            //   throw new ArgumentOutOfRangeException("emSize", "Parameter Is Too Large.");

            if (double.IsNaN(emSize))
                throw new ArgumentOutOfRangeException("emSize", "Parameter Cannot Be NaN.");
        }
    }

    /*
    public class SimpleTextRunProperties : TextRunProperties
    {
        private Typeface typeface;
        private double fontSize1;
        private double fontSize2;
        private TextDecorationCollection textDecorations;
        private Brush textColor;
        private SolidColorBrush background;
        private BaselineAlignment baseline;
        private CultureInfo currentUICulture;

        public override Brush BackgroundBrush { get { return this.background; } }
        public override CultureInfo CultureInfo { get { return this.currentUICulture; } }
        public override double FontHintingEmSize { get { return this.fontSize2; } }
        public override double FontRenderingEmSize { get { return this.fontSize1; } }
        public override Brush ForegroundBrush { get { return this.textColor; } }
        public override TextDecorationCollection TextDecorations { get { return this.textDecorations; } }
        public override TextEffectCollection TextEffects { get { return null; } }
        public override Typeface Typeface { get { return this.typeface; } }
        public override BaselineAlignment BaselineAlignment { get { return this.baseline; } }

        public SimpleTextRunProperties(Typeface typeface, 
                                        double pixelsPerDip, 
                                        double fontSize1, 
                                        double fontSize2, 
                                        TextDecorationCollection textDecorations, 
                                        Brush textColor, 
                                        SolidColorBrush background, 
                                        BaselineAlignment baseline, 
                                        CultureInfo currentUICulture)
        {
            this.typeface = typeface;
            this.PixelsPerDip = pixelsPerDip;
            this.fontSize1 = fontSize1;
            this.fontSize2 = fontSize2;
            this.textDecorations = textDecorations;
            this.textColor = textColor;
            this.background = background;
            this.baseline = baseline;
            this.currentUICulture = currentUICulture;
        }


    }
    */
}