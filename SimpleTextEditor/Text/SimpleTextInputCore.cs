using System.Windows;
using System.Windows.Input;

using SimpleTextEditor.Model;
using SimpleTextEditor.Model.Interface;
using SimpleTextEditor.Text.Interface;
using SimpleTextEditor.Text.Source;
using SimpleTextEditor.Text.Source.Interface;
using SimpleTextEditor.Text.Visualization;

namespace SimpleTextEditor.Text
{
    public class SimpleTextInputCore : ITextInputCore
    {
        // Primary Components
        ITextSource _textSource;
        ITextVisualCore _visualCore;

        // Primary caret source
        Caret _caret;

        // Mouse Data
        Point? _mouseDownPoint;

        bool _initialized;

        public SimpleTextInputCore()
        {
            _caret = new Caret(0, Rect.Empty, AppendPosition.None);
            _mouseDownPoint = null;

            _initialized = false;
        }

        public void Initialize(VisualInputData inputData)
        {
            _textSource = new LinearTextSource(inputData);
            _visualCore = new SimpleTextVisualCore();

            // MSFT TextFormatter 
            var textRunProvider = new SimpleTextRunProvider(_textSource);
            var formatter = new SimpleTextEditorFormatter(textRunProvider, _textSource, inputData);

            // Initialize Components
            _visualCore.Initialize(formatter, _textSource, inputData);

            _initialized = true;
        }
        public void UpdateSize(Size contorlSize)
        {
            if (!_initialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            _visualCore.UpdateSize(contorlSize);
        }
        public Rect GetCaretBounds()
        {
            if (!_initialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            return _caret.VisualBounds;
        }

        public VisualOutputData GetOutput()
        {
            if (!_initialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            return _visualCore.GetOutput();
        }

        public bool ProcessControlInput(ControlInput input)
        {
            if (!_initialized)
                throw new Exception("SimpleTextVisualCore not yet initialized");

            var caretInvalid = false;

            switch (input)
            {
                case ControlInput.None:
                    break;
                case ControlInput.Backspace:
                    if (_textSource.GetLength() == 0)
                        return false;

                    _visualCore.RemoveText(_caret.GetAdjustedOffset(), 1);
                    break;
                case ControlInput.DeleteCurrentCharacter:
                    if (_caret.GetAdjustedOffset() + 1 >= _textSource.GetLength())
                        return false;

                    _visualCore.RemoveText(_caret.GetAdjustedOffset() + 1, 1);
                    break;
                case ControlInput.LineUp:
                    break;
                case ControlInput.LineDown:
                    break;
                case ControlInput.CharacterLeft:
                    break;
                case ControlInput.CharacterRight:
                    break;
                case ControlInput.WordLeft:
                    break;
                case ControlInput.WordRight:
                    break;
                case ControlInput.EndOfLine:
                    break;
                case ControlInput.BeginningOfLine:
                    break;
                case ControlInput.PageUp:
                    break;
                case ControlInput.PageDown:
                    break;
                case ControlInput.EndOfDocument:
                    break;
                case ControlInput.BeginningOfDocument:
                    break;
                default:
                    throw new Exception("Unhandled Control Input:  SimpleTextVisualCore");
            }

            return _visualCore.IsInvalid;
        }

        public void Load(string text)
        {
            _visualCore.ClearText();
            _visualCore.AppendText(text);
        }

        public bool ProcessTextInputAtCaret(string text)
        {
            throw new NotImplementedException();
        }

        public bool ProcessRemoveSelectedText()
        {
            throw new NotImplementedException();
        }

        public bool ProcessPreviewMouseMove(Point location, MouseButtonState leftButtonState, MouseButtonState rightButtonState)
        {
            if (!_initialized)
                throw new Exception("SimpleTextInputCore not yet initialized");

            UpdateMouseData(location, leftButtonState, rightButtonState);

            return ProcessMouse(location);
        }

        public bool ProcessMouseDown(Point location, MouseButtonState leftButtonState, MouseButtonState rightButtonState)
        {
            if (!_initialized)
                throw new Exception("SimpleTextInputCore not yet initialized");

            UpdateMouseData(location, leftButtonState, rightButtonState);

            return ProcessMouse(location);
        }

        public bool ProcessMouseUp(Point location, MouseButtonState leftButtonState, MouseButtonState rightButtonState)
        {
            if (!_initialized)
                throw new Exception("SimpleTextInputCore not yet initialized");

            UpdateMouseData(location, leftButtonState, rightButtonState);

            return ProcessMouse(location);
        }

        #region (private) Update Methods
        private void UpdateMouseData(Point location, MouseButtonState leftButtonState, MouseButtonState rightButtonState)
        {
            if (leftButtonState == MouseButtonState.Pressed)
                _mouseDownPoint = location;
            else
                _mouseDownPoint = null;
        }
        private bool ProcessMouse(Point currentLocation)
        {
            if (_mouseDownPoint == null)
                return _visualCore.IsInvalid;

            // Click Event (not click-drag)
            if (_mouseDownPoint == null ||
                _mouseDownPoint.Value.Equals(currentLocation))
            {
                var textPosition = _visualCore.GetOutput().VisualPointToTextPosition(currentLocation);

                // Move Caret
                UpdateCaret(textPosition);

                // Clear Selection
                _textSource.ClearProperties();

                // TODO: Performance Optimization
                _visualCore.Invalidate();

                return _visualCore.IsInvalid;
            }

            // Click Drag
            else
            {
                var textPosition1 = _visualCore.GetOutput().VisualPointToTextPosition(_mouseDownPoint.Value);
                var textPosition2 = _visualCore.GetOutput().VisualPointToTextPosition(currentLocation);

                var index1 = textPosition1.AppendPosition == AppendPosition.Append ? textPosition1.Offset - 1 : textPosition1.Offset;
                var index2 = textPosition2.AppendPosition == AppendPosition.Append ? textPosition2.Offset - 1 : textPosition2.Offset;

                var startIndex = Math.Min(index1, index2);
                var endIndex = Math.Max(index1, index2);

                var selectRange = IndexRange.FromIndices(startIndex, endIndex);

                // Move Caret
                UpdateCaret(textPosition2);

                // Highlight Selection
                _textSource.ClearProperties();
                _textSource.SetProperties(selectRange, TextPropertySet.Highlighted);

                // TODO: Performance Optimization
                _visualCore.Invalidate();

                return _visualCore.IsInvalid;
            }
        }

        /// <summary>
        /// Caret position may be one greater than previous text, putting it in the visual line's "append position"
        /// </summary>
        private void UpdateCaret(ITextPosition textPosition)
        {
            // Update Caret Position
            var caretOrigin = _visualCore.GetOutput().CharacterOffsetToVisualOffset(textPosition.Offset, textPosition.AppendPosition);
            var textHeight = _visualCore.GetOutput().GetVisualLineHeight(textPosition.VisualLineNumber);

            _caret.Update(textPosition.Offset, new Rect(caretOrigin, new Size(2, textHeight)), textPosition.AppendPosition);
        }
        #endregion
    }
}
