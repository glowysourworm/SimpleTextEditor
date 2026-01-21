using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

using SimpleWpf.Extensions.Collection;

namespace SimpleTextEditor.Text.Visualization.Properties
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

            this.PixelsPerDip = pixelsPerDip;
            _typeface = typeface;
            _emSize = emSize;
            _emHintingSize = emHintingSize;
            _textDecorations = textDecorations ?? new TextDecorationCollection();
            _foregroundBrush = forgroundBrush;
            _backgroundBrush = backgroundBrush;
            _baselineAlignment = baselineAlignment;
            _culture = culture;
        }

        /// <summary>
        /// Modifies properties of the font, and updates decorations
        /// </summary>
        public void Modify(Typeface typeface,
                           double emSize,
                           Brush foregroundBrush,
                           Brush backgroundBrush)
        {
            _typeface = typeface;
            _emSize = emSize;
            _foregroundBrush = foregroundBrush;
            _backgroundBrush = backgroundBrush;

            // Cleaer / Re-Add Decorations (foreground)
            var decorations = _textDecorations.Select(x => x.Location).Actualize();

            _textDecorations.Clear();

            foreach (var decoration in decorations)
            {
                _textDecorations.Add(new TextDecoration(decoration, new Pen(_foregroundBrush, 1.0), 0,
                                     TextDecorationUnit.FontRecommended, TextDecorationUnit.FontRecommended));
            }
        }

        /// <summary>
        /// Sets text decoration (using current colors)
        /// </summary>
        public void SetDecoration(TextDecorationLocation decoration)
        {
            _textDecorations.Clear();
            _textDecorations.Add(new TextDecoration(decoration, new Pen(_foregroundBrush, 1), 0,
                                 TextDecorationUnit.FontRecommended, TextDecorationUnit.FontRecommended));
        }

        public SimpleTextRunProperties CreateCopy()
        {
            var typeFace = new Typeface(this.Typeface.FontFamily, this.Typeface.Style, this.Typeface.Weight, this.Typeface.Stretch, this.Typeface.FontFamily);

            return new SimpleTextRunProperties(typeFace,
                                               this.PixelsPerDip,
                                               _emSize,
                                               _emHintingSize,
                                               _textDecorations.Clone(),
                                               _foregroundBrush.Clone(),
                                               _backgroundBrush.Clone(),
                                               _baselineAlignment,
                                               CultureInfo.CurrentUICulture);
        }
    }
}