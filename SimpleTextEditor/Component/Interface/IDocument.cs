using System.Windows;
using System.Windows.Input;

using SimpleTextEditor.Model;
using SimpleTextEditor.Text.Visualization;

namespace SimpleTextEditor.Component.Interface
{
    public interface IDocument
    {
        /// <summary>
        /// Gets the total text length of the document's data source
        /// </summary>
        int TextLength { get; }

        /// <summary>
        /// Processes requested control input: Move caret, Select text, Remove selected text, etc...
        /// </summary>        
        bool ProcessControlInput(ControlInput input);
        bool ProcessPreviewMouseMove(Point pointUI, MouseButtonState leftButtonState, MouseButtonState rightButtonState);
        bool ProcessMouseButtonDown(Point pointUI, MouseButtonState leftButtonState, MouseButtonState rightButtonState);
        bool ProcessMouseButtonUp(Point pointUI, MouseButtonState leftButtonState, MouseButtonState rightButtonState);

        /// <summary>
        /// Processes input text using the current cursor position
        /// </summary>
        void ProcessInputText(string inputText);

        /// <summary>
        /// Returns visual elements that may be rendered
        /// </summary>
        VisualTextCollection GetVisualText();
        Size Measure(Size availableSize);
        Rect GetCaretBounds();

        void Initialize(VisualInputData inputData);
        void Load(string text);
        void Clear();
    }
}
