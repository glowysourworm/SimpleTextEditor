using System.Windows;
using System.Windows.Media;

using SimpleTextEditor.Text.Interface;
using SimpleTextEditor.Text.Visualization;

namespace SimpleTextEditor.Text
{
    public class SimpleTextVisualCore : ITextVisualCore
    {
        // Recommended setting for pixelsPerDip
        protected const double PixelsPerDip = 1.25D;

        // Visual data
        SimpleTextVisualInputData _visualInputData;

        // Primary text store
        SimpleTextStore _textStore;

        // MSFT Advanced text formatting
        SimpleTextEditorFormatter _formatter;

        public SimpleTextVisualCore(FontFamily fontFamily, double fontSize, Brush foreground, Brush background, TextWrapping textWrapping)
        {
            _visualInputData = new SimpleTextVisualInputData(fontFamily, fontSize, foreground, background, textWrapping);
            _formatter = new SimpleTextEditorFormatter(_visualInputData);
            _textStore = new SimpleTextStore(SimpleTextVisualCore.PixelsPerDip, _visualInputData.TextProperties);
        }

        public ITextSource GetSource()
        {
            return _textStore;
        }

        public SimpleTextVisualOutputData Measure(Size constraint)
        {
            return _formatter.MeasureText(_textStore, constraint);
        }
    }
}
