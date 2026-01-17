using System.Windows;
using System.Windows.Media;

using SimpleTextEditor.Model;
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

        // Visual Output Data (last)
        SimpleTextVisualOutputData? _lastVisualOutputData;

        // Primary text store
        SimpleTextStore _textStore;

        // MSFT Advanced text formatting
        SimpleTextEditorFormatter _formatter;

        public SimpleTextVisualCore(FontFamily fontFamily,
                                    double fontSize,
                                    Brush foreground,
                                    Brush background,
                                    Brush highlightForeground,
                                    Brush highlightBackground,
                                    TextWrapping textWrapping)
        {
            _visualInputData = new SimpleTextVisualInputData(fontFamily, fontSize, foreground, background, highlightForeground, highlightBackground, textWrapping);
            _textStore = new SimpleTextStore(_visualInputData);
            _formatter = new SimpleTextEditorFormatter(_textStore, _visualInputData);

            _lastVisualOutputData = null;
        }

        public void AppendText(string text)
        {
            // Insert
            var insertIndex = _textStore.GetLength();

            // Append to source
            _textStore.AppendText(text);

            // Update TextRun Cache
            _formatter.UpdateCache(insertIndex, text.Length, -1);
        }

        public Rect GetCaretBounds()
        {
            if (_lastVisualOutputData == null)
                return new Rect();

            return _formatter.CalculateCaretBounds(_lastVisualOutputData, _lastVisualOutputData.ConstraintSize);
        }

        public string GetTextCopy()
        {
            return _textStore.ToString() ?? string.Empty;
        }

        public int GetTextLength()
        {
            return _textStore.GetLength();
        }

        public void InsertText(int offset, string text)
        {
            // Insert
            _textStore.InsertText(offset, text);

            // Update TextRun Cache
            _formatter.UpdateCache(offset, text.Length, -1);
        }

        public SimpleTextVisualOutputData Measure(Size constraint)
        {
            _lastVisualOutputData = _formatter.MeasureText(constraint);

            return _lastVisualOutputData;
        }

        public void RemoveText(int offset, int count)
        {
            // Remove
            _textStore.RemoveText(offset, count);

            // Update TextRun Cache
            _formatter.UpdateCache(offset, -1, offset);
        }

        public int SearchText(char character, int startIndex)
        {
            return _textStore.Search(character, startIndex);
        }

        public void SetMouseInfo(MouseData mouseData)
        {
            _formatter.SetMouseInfo(mouseData);
            _textStore.SetMouseInfo(mouseData);
        }
    }
}
