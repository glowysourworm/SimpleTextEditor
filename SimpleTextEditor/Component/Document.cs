using System.Windows;
using System.Windows.Input;

using SimpleTextEditor.Component.Interface;
using SimpleTextEditor.Model;
using SimpleTextEditor.Text;
using SimpleTextEditor.Text.Interface;
using SimpleTextEditor.Text.Visualization;

namespace SimpleTextEditor.Component
{
    public class Document : IDocument
    {
        // Carries the primary text source
        readonly ITextInputCore _core;

        public int TextLength { get; }

        public Document()
        {
            _core = new SimpleTextInputCore();
        }

        #region (public) IDocument Methods
        public void Initialize(VisualInputData visualInputData)
        {
            _core.Initialize(visualInputData);
        }
        public void Load(string text)
        {
            _core.Load(text);
        }
        public Size Measure(Size constraint)
        {
            // Update size on the backend
            _core.UpdateSize(constraint);

            // This will force a measure pass if it hasn't run already
            return _core.GetOutput().DesiredSize;
        }
        public Rect GetCaretBounds()
        {
            return _core.GetCaretBounds();
        }
        public bool ProcessControlInput(ControlInput input)
        {
            return _core.ProcessControlInput(input);
        }
        public void Clear()
        {
            _core.Load(string.Empty);
        }

        public bool ProcessPreviewMouseMove(Point pointUI, MouseButtonState leftButtonState, MouseButtonState rightButtonState)
        {
            return _core.ProcessPreviewMouseMove(pointUI, leftButtonState, rightButtonState);
        }

        public bool ProcessMouseButtonDown(Point pointUI, MouseButtonState leftButtonState, MouseButtonState rightButtonState)
        {
            return _core.ProcessMouseDown(pointUI, leftButtonState, rightButtonState);
        }

        public bool ProcessMouseButtonUp(Point pointUI, MouseButtonState leftButtonState, MouseButtonState rightButtonState)
        {
            return _core.ProcessMouseUp(pointUI, leftButtonState, rightButtonState);
        }

        public void ProcessInputText(string inputText)
        {
            _core.ProcessTextInputAtCaret(inputText);
        }

        public VisualTextCollection GetVisualText()
        {
            return _core.GetOutput().VisualCollection;
        }
        #endregion
    }
}
