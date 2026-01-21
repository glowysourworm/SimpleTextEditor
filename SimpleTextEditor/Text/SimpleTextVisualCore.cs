using System.Windows;

using SimpleTextEditor.Model;
using SimpleTextEditor.Model.Interface;
using SimpleTextEditor.Text.Interface;
using SimpleTextEditor.Text.Source.Interface;
using SimpleTextEditor.Text.Visualization;

namespace SimpleTextEditor.Text
{
    public class SimpleTextVisualCore : ITextVisualCore
    {
        public bool IsInvalid { get { return _formatter.IsInvalid; } }
        public bool IsInitialized { get { return _formatter.IsInitialized; } }

        // Recommended setting for pixelsPerDip
        protected const double PixelsPerDip = 1.25D;

        // Primary text source
        ITextSource _textSource;

        // MSFT Advanced text formatting
        ITextFormatter _formatter;

        public SimpleTextVisualCore()
        {
        }

        public void Initialize(ITextFormatter formatter, ITextSource textSource, VisualInputData inputData)
        {
            _textSource = textSource;
            _formatter = formatter;
        }
        public void UpdateSize(Size contorlSize)
        {
            _formatter.UpdateSize(contorlSize);
        }
        public ITextPosition AppendText(string text)
        {
            if (!_formatter.IsInitialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            // Insert
            var insertIndex = _textSource.GetLength();

            // Append to source
            _textSource.AppendText(text);

            // Invalidate Cache -> Invalid
            Invalidate(insertIndex, text.Length, -1);

            return _formatter.GetOutput()
                             .CharacterOffsetToTextPosition(_textSource.GetLength(), AppendPosition.Append);
        }
        public ITextPosition InsertText(int offset, string text)
        {
            if (!_formatter.IsInitialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            // Insert
            _textSource.InsertText(offset, text);

            // Invalidate Cache -> Invalid
            Invalidate(offset, text.Length, -1);

            return _formatter.GetOutput()
                             .CharacterOffsetToTextPosition(offset + text.Length, AppendPosition.None);
        }
        public ITextPosition RemoveText(int offset, int count)
        {
            if (!_formatter.IsInitialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            // Remove
            _textSource.RemoveText(offset, count);

            // Update TextRun Cache
            Invalidate(offset, -1, offset);

            return _formatter.GetOutput()
                             .CharacterOffsetToTextPosition(offset, AppendPosition.None);
        }
        public ITextPosition ClearText()
        {
            if (!_formatter.IsInitialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            _textSource.ClearText();

            return _formatter.GetOutput()
                             .CharacterOffsetToTextPosition(0, AppendPosition.Append);
        }
        public VisualOutputData GetOutput()
        {
            if (!_formatter.IsInitialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            return _formatter.GetOutput();
        }

        public void Invalidate()
        {
            if (!_formatter.IsInitialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            _formatter.Invalidate();
        }

        public void Invalidate(int startIndex, int additionLength, int removalLength)
        {
            if (!_formatter.IsInitialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            _formatter.Invalidate(startIndex, additionLength, removalLength);
        }
    }
}
