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

        // Primary text source
        ITextSource _textSource;

        // Primary text store
        SimpleTextRunProvider _textRunProvider;

        // Primary text caret source
        SimpleCaretTracker _caretTracker;

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
            _textSource = new LinearTextSource(_visualInputData.GetProperties(TextPropertySet.Normal));
            _textRunProvider = new SimpleTextRunProvider(_textSource);
            _caretTracker = new SimpleCaretTracker();
            _formatter = new SimpleTextEditorFormatter(_textRunProvider, _textSource, _caretTracker, _visualInputData);

            _lastVisualOutputData = null;
        }

        public void AppendText(string text)
        {
            // Insert
            var insertIndex = _textSource.GetLength();

            // Append to source
            _textSource.AppendText(text);

            // Update TextRun Cache
            _formatter.UpdateCache(insertIndex, text.Length, -1);

            // Update Caret Position
            _caretTracker.SetCaretPosition(text.Length - 1);
        }

        public Rect GetCaretBounds()
        {
            if (_lastVisualOutputData == null)
                return new Rect();

            return _formatter.CalculateCaretBounds(_lastVisualOutputData, _lastVisualOutputData.ConstraintSize);
        }

        public string GetTextCopy()
        {
            return _textSource.ToString() ?? string.Empty;
        }

        public int GetTextLength()
        {
            return _textSource.GetLength();
        }

        public void InsertText(int offset, string text)
        {
            // Insert
            _textSource.InsertText(offset, text);

            // Update TextRun Cache
            _formatter.UpdateCache(offset, text.Length, -1);

            // Update Caret
            _caretTracker.SetCaretPosition(offset);
        }

        public SimpleTextVisualOutputData Measure(Size constraint)
        {
            _lastVisualOutputData = _formatter.MeasureText(constraint);

            return _lastVisualOutputData;
        }

        public void RemoveText(int offset, int count)
        {
            // Remove
            _textSource.RemoveText(offset, count);

            // Update TextRun Cache
            _formatter.UpdateCache(offset, -1, offset);

            // Update Caret
            _caretTracker.SetCaretPosition(offset);
        }

        public int SearchText(char character, int startIndex)
        {
            return _textSource.Search(character, startIndex);
        }

        public bool SetMouseInfo(MouseData mouseData)
        {
            var result = false;

            if (_lastVisualOutputData != null)
            {
                foreach (var element in _lastVisualOutputData.VisualElements)
                {
                    // Intersection
                    if (element.Position.VisualBounds.IntersectsWith(mouseData.SelectionBounds))
                    {

                        _textSource.SetProperties(IndexRange.FromStartCount(element.Position.SourceOffset,
                                                 element.Length),
                                                 _visualInputData.GetProperties(TextPropertySet.Highlighted));

                        _formatter.InvalidateCache();

                        result = true;
                    }
                }
            }

            // If it's still the first pass, then the measure will recreate the text runs anyway.
            if (_lastVisualOutputData != null)
                _lastVisualOutputData = _formatter.MeasureText(_lastVisualOutputData.ConstraintSize);

            return result;
        }
    }
}
