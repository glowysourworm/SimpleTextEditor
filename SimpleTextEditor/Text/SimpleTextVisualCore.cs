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

        // Primary text source
        ITextSource _textSource;

        // Primary text store
        SimpleTextRunProvider _textRunProvider;

        // Primary text caret source
        SimpleCaretTracker _caretTracker;

        // MSFT Advanced text formatting
        SimpleTextEditorFormatter _formatter;

        // Keep track of control size. If it doesen't change, we don't need to re-call the formatter's measure method.
        Size _constraintSize;
        bool _initialized;
        bool _invalid;

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

            _initialized = false;
            _invalid = true;
        }

        public void Initialize(Size constraintSize)
        {
            if (_initialized)
                throw new Exception("SimpleTextVisualCore already initialized");

            _constraintSize = constraintSize;
            _formatter.MeasureText(constraintSize);
            _initialized = true;
            _invalid = false;
        }

        public void AppendText(string text)
        {
            if (!_initialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            // Insert
            var insertIndex = _textSource.GetLength();

            // Append to source
            _textSource.AppendText(text);

            // Invalidate Cache -> Measure
            _formatter.UpdateCache(insertIndex, text.Length, -1);
            _formatter.MeasureText(_constraintSize);

            // Set caret position to the trailing (OVERFLOW) position
            UpdateCaret(_textSource.GetLength());

            _invalid = false;
        }

        public Rect GetCaretBounds()
        {
            if (!_initialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            var caretPosition = _caretTracker.GetCaretPosition();

            if (caretPosition == null)
                return new Rect();

            return caretPosition.VisualBounds;
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
            if (!_initialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            // Insert
            _textSource.InsertText(offset, text);

            // Invalidate Cache -> Measure
            _formatter.UpdateCache(offset, text.Length, -1);
            _formatter.MeasureText(_constraintSize);

            // Update Caret to trail insert text
            UpdateCaret(offset + text.Length);

            _invalid = false;
        }

        public SimpleTextVisualOutputData GetOutput()
        {
            if (!_initialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            if (_invalid)
                _formatter.MeasureText(_constraintSize);

            _invalid = false;

            return _formatter.GetLastOutput();
        }

        public void UpdateSize(Size contorlSize)
        {
            if (!_initialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            var invalidate = !contorlSize.Equals(_constraintSize);

            _constraintSize = contorlSize;

            if (invalidate)
                _formatter.MeasureText(_constraintSize);

            _invalid = false;
        }

        public void RemoveText(int offset, int count)
        {
            if (!_initialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            // Remove
            _textSource.RemoveText(offset, count);

            // Update TextRun Cache
            _formatter.UpdateCache(offset, -1, offset);
            _formatter.MeasureText(_constraintSize);

            // Update Caret to trail insert text
            UpdateCaret(offset);

            _invalid = false;
        }

        public int SearchText(char character, int startIndex)
        {
            return _textSource.Search(character, startIndex);
        }

        public bool SetMouseInfo(MouseData mouseData)
        {
            if (!_initialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            var result = false;

            //if (_lastVisualOutputData != null)
            //{
            //    foreach (var element in _lastVisualOutputData.VisualElements)
            //    {
            //        // Intersection
            //        if (element.Position.VisualBounds.IntersectsWith(mouseData.SelectionBounds))
            //        {

            //            _textSource.SetProperties(IndexRange.FromStartCount(element.Position.SourceOffset,
            //                                     element.Length),
            //                                     _visualInputData.GetProperties(TextPropertySet.Highlighted));

            //            _formatter.InvalidateCache();

            //            result = true;
            //        }
            //    }
            //}

            //// If it's still the first pass, then the measure will recreate the text runs anyway.
            //if (_lastVisualOutputData != null)
            //    _lastVisualOutputData = _formatter.MeasureText(_lastVisualOutputData.ConstraintSize);

            return result;
        }

        /// <summary>
        /// Caret position is always one greater than previous text. So, it OVERFLOWS BY ONE.
        /// </summary>
        /// <param name="caretPosition">1 + offset to source character, or equal to the total text source length!</param>
        private void UpdateCaret(int caretPosition)
        {
            // Update Caret Position
            var caretOrigin = _formatter.CharacterOffsetToVisualOffset(caretPosition, true);
            var textPosition = _formatter.CharacterOffsetToTextPosition(caretPosition - 1);
            var textHeight = _formatter.GetVisualLineHeight(textPosition.VisualLineNumber);

            _caretTracker.Update(new Rect(caretOrigin, new Size(2, textHeight)),
                                 caretPosition,
                                 textPosition.SourceLineNumber,
                                 textPosition.VisualColumn,
                                 textPosition.VisualLineNumber,
                                 textPosition.ParagraphNumber);
        }
    }
}
