using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SimpleTextEditor.Model.Configuration
{
    public class TextProperties
    {
        public string Font { get; set; }
        public double FontSize { get; set; }
        public FontStyle FontStyle { get; set; }
        public FontWeight FontWeight { get; set; }
        public Brush Foreground { get; set; }
        public Brush Background { get; set; }
        public TextDecorationLocation Decoration { get; set; }
        public bool HasDecoration { get; set; }
        public IndexRange Range { get; set; }

        public TextProperties()
        {
            this.Font = string.Empty;
            this.FontSize = 12.0D;
            this.FontStyle = FontStyles.Normal;
            this.FontWeight = FontWeights.Normal;
            this.Foreground = Brushes.Black;
            this.Background = Brushes.Transparent;
            this.Decoration = TextDecorationLocation.Underline;
            this.HasDecoration = false;
            this.Range = IndexRange.FromIndices(0, 0);
        }

        /// <summary>
        /// Initialize font settings using control's properties. Text decorations are not set or modified. Range is
        /// not set or modified.
        /// </summary>
        public void SetFromControl(Control control)
        {
            this.Font = control.FontFamily.ToString();
            this.FontSize = control.FontSize;
            this.FontStyle = control.FontStyle;
            this.FontWeight = control.FontWeight;
            this.Foreground = control.Foreground;
            this.Background = control.Background;
        }
    }
}
