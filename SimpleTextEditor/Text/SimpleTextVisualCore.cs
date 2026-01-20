using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using SimpleTextEditor.Model;
using SimpleTextEditor.Model.Interface;
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

        // Primary caret source
        Caret _caret;

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
            _caret = new Caret(0, Rect.Empty, true);
            _formatter = new SimpleTextEditorFormatter(_textRunProvider, _textSource, _visualInputData);

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
            UpdateCaret(_textSource.GetLength(), true);

            _invalid = false;
        }

        public Rect GetCaretBounds()
        {
            if (!_initialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            return _caret.VisualBounds;
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
            UpdateCaret(offset, false);

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

            var invalidate = !contorlSize.Equals(_constraintSize) || _invalid;

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
            UpdateCaret(offset, offset == _textSource.GetLength());

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

            // Procedure:  Set click / click-drag events, and let the formatter run on next
            //             render pass (might have some performance benefit). So, just set
            //             invalid flag.
            // 

            // Nothing to do (no hover events currently)
            if (mouseData.LeftButton == MouseButtonState.Released)
                return _invalid;

            else
            {
                // Click Event (not click-drag)
                if (mouseData.MouseDownLocation == null ||
                    mouseData.MouseDownLocation.Value.Equals(mouseData.MouseLocation))
                {
                    var isAppendPosition = false;
                    var textPosition = _formatter.VisualPointToTextPosition(mouseData.MouseLocation, out isAppendPosition);

                    // Move Caret
                    UpdateCaret(textPosition, isAppendPosition);

                    // Clear Selection
                    _textSource.ClearProperties();
                }

                // Click Drag
                else
                {
                    var isAppendPosition1 = false;
                    var isAppendPosition2 = false;
                    var textPosition1 = _formatter.VisualPointToTextPosition(mouseData.MouseDownLocation.Value, out isAppendPosition1);
                    var textPosition2 = _formatter.VisualPointToTextPosition(mouseData.MouseLocation, out isAppendPosition2);
                    var startIndex = Math.Min(textPosition1.SourceOffset, textPosition2.SourceOffset);
                    var endIndex = Math.Max(textPosition1.SourceOffset, textPosition2.SourceOffset);
                    var selectRange = IndexRange.FromIndices(startIndex, endIndex);

                    // Move Caret
                    UpdateCaret(textPosition2, isAppendPosition2);

                    // Highlight Selection
                    _textSource.ClearProperties();
                    _textSource.SetProperties(selectRange, _visualInputData.GetProperties(TextPropertySet.Highlighted));
                }

                _formatter.InvalidateCache();
                _invalid = true;
            }

            return _invalid;
        }

        /// <summary>
        /// Caret position may be one greater than previous text, putting it in the visual line's "append position"
        /// </summary>
        private void UpdateCaret(int caretOffset, bool isAppendPosition)
        {
            if (caretOffset > _textSource.GetLength() || caretOffset < 0)
                throw new ArgumentException("Caret offset is out of bounds");

            // Update Caret Position
            var caretOrigin = _formatter.CharacterOffsetToVisualOffset(caretOffset, isAppendPosition);
            var textPosition = _formatter.CharacterOffsetToTextPosition(caretOffset, isAppendPosition);
            var textHeight = _formatter.GetVisualLineHeight(textPosition.VisualLineNumber);

            _caret.Update(caretOffset, new Rect(caretOrigin, new Size(2, textHeight)), isAppendPosition);
        }

        /// <summary>
        /// Caret position may be one greater than previous text, putting it in the visual line's "append position"
        /// </summary>
        private void UpdateCaret(ITextPosition textPosition, bool isAppendPosition)
        {
            // Update Caret Position
            var caretOrigin = _formatter.CharacterOffsetToVisualOffset(textPosition.SourceOffset, isAppendPosition);
            var textHeight = _formatter.GetVisualLineHeight(textPosition.VisualLineNumber);

            _caret.Update(textPosition.SourceOffset, new Rect(caretOrigin, new Size(2, textHeight)), isAppendPosition);
        }
    }
}
