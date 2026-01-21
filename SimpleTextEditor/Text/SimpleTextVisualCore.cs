using System.Windows;

using SimpleTextEditor.Text.Interface;
using SimpleTextEditor.Text.Source.Interface;
using SimpleTextEditor.Text.Visualization;

namespace SimpleTextEditor.Text
{
    public class SimpleTextVisualCore : ITextVisualCore
    {
        public bool IsInvalid { get { return _invalid; } }
        public bool IsInitialized { get { return _initialized; } }

        public int TextLength { get; }

        // Recommended setting for pixelsPerDip
        protected const double PixelsPerDip = 1.25D;

        // Primary text source
        ITextSource _textSource;

        // MSFT Advanced text formatting
        SimpleTextEditorFormatter _formatter;

        // Keep track of control size. If it doesen't change, we don't need to re-call the formatter's measure method.
        Size _constraintSize;
        bool _initialized;
        bool _invalid;

        public SimpleTextVisualCore()
        {
            _initialized = false;
            _invalid = true;
        }

        public void Initialize(SimpleTextEditorFormatter formatter, ITextSource textSource, VisualInputData inputData)
        {
            if (_initialized)
                throw new Exception("SimpleTextVisualCore already initialized");

            _textSource = textSource;
            _formatter = formatter;

            _constraintSize = inputData.ConstraintSize;
            _formatter.MeasureText(inputData.ConstraintSize);
            _initialized = true;
            _invalid = false;
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
        public void AppendText(string text)
        {
            if (!_initialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            // Insert
            var insertIndex = _textSource.GetLength();

            // Append to source
            _textSource.AppendText(text);

            // Invalidate Cache -> Invalid
            Invalidate(insertIndex, text.Length, -1);
        }
        public void InsertText(int offset, string text)
        {
            if (!_initialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            // Insert
            _textSource.InsertText(offset, text);

            // Invalidate Cache -> Invalid
            Invalidate(offset, text.Length, -1);
        }
        public void RemoveText(int offset, int count)
        {
            if (!_initialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            // Remove
            _textSource.RemoveText(offset, count);

            // Update TextRun Cache
            Invalidate(offset, -1, offset);
        }
        public void ClearText()
        {
            if (!_initialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            _textSource.ClearText();
        }
        public VisualOutputData GetOutput()
        {
            if (!_initialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            if (_invalid)
                _formatter.MeasureText(_constraintSize);

            _invalid = false;

            return _formatter.GetLastOutput();
        }

        public void Invalidate()
        {
            if (!_initialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            _formatter.InvalidateCache();
            _invalid = true;
        }

        public void Invalidate(int startIndex, int additionLength, int removalLength)
        {
            if (!_initialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            _formatter.UpdateCache(startIndex, additionLength, removalLength);
            _invalid = true;
        }
    }
}
