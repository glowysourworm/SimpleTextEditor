using System.Windows;

using SimpleTextEditor.Model;
using SimpleTextEditor.Model.Interface;
using SimpleTextEditor.Text.Interface;
using SimpleTextEditor.Text.Source.Interface;
using SimpleTextEditor.Text.Visualization;
using SimpleTextEditor.Text.Visualization.Properties;
using SimpleTextEditor.Text.Visualization.Properties.Interface;

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

        // Primary text properties source for both text selection and alternate text settings
        ITextPropertiesSource _propertiesSource;

        // Primary input data for the backend (except the text source)
        VisualInputData _visualInputData;

        public SimpleTextVisualCore()
        {
        }

        public void Initialize(ITextFormatter formatter, ITextPropertiesSource propertiesSource, ITextSource textSource, VisualInputData inputData)
        {
            _textSource = textSource;
            _formatter = formatter;
            _visualInputData = inputData;
            _propertiesSource = propertiesSource;
        }
        public void UpdateSize(Size contorlSize)
        {
            _formatter.UpdateSize(contorlSize);
        }
        public int GetTextLength()
        {
            return _textSource.GetLength();
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

            var result = _formatter.GetOutput()
                                   .VisualCollection
                                   .GetAppendPosition();

            if (result == null)
                throw new Exception("Mishandled text offset:  SimpleTextVisualCore.InsertText");

            return result;
        }
        public ITextPosition InsertText(int offset, string text)
        {
            if (!_formatter.IsInitialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            // Insert
            _textSource.InsertText(offset, text);

            // Invalidate Cache -> Invalid
            Invalidate(offset, text.Length, -1);

            //// Check to see if we're appending
            //var appendPosition = _formatter.GetOutput()
            //                               .VisualCollection
            //                               .GetAppendPosition();

            //if (appendPosition == null)
            //    throw new Exception("Cannot insert text to an empty ITextSource! Please use AppendText");

            var result = _formatter.GetOutput()
                                   .VisualCollection
                                   .GetVisualLineForOffset(offset)
                                   .GetPosition(offset);

            if (result == null)
                throw new Exception("Mishandled text offset:  SimpleTextVisualCore.InsertText");

            return result;
        }
        public ITextPosition? RemoveText(int offset, int count)
        {
            if (!_formatter.IsInitialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            // Remove
            _textSource.RemoveText(offset, count);

            // Update TextRun Cache
            Invalidate(offset, -1, offset);

            return _formatter.GetOutput()
                             .VisualCollection
                             .GetVisualLineForOffset(offset)
                             .GetPosition(offset);
        }
        public ITextPosition? ClearText()
        {
            if (!_formatter.IsInitialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            _textSource.ClearText();

            return _formatter.GetOutput()
                             .VisualCollection
                             .GetAppendPosition();
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

        public void ModifyTextRange(IndexRange range, Action<SimpleTextRunProperties> modifier)
        {
            if (!_formatter.IsInitialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            _propertiesSource.SetProperties(range, x =>
            {
                // Invoke copy method
                var properties = x.CreateCopy();

                // Apply modifier
                modifier(properties);

                return properties;
            });
            _formatter.Invalidate();
        }

        public void ClearAffectedTextProperties(IndexRange range)
        {
            if (!_formatter.IsInitialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            _propertiesSource.ClearAffectedProperties(range);
        }

        public void ClearTextProperties()
        {
            if (!_formatter.IsInitialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            _propertiesSource.ClearProperties();
            _formatter.Invalidate();
        }
    }
}
