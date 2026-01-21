using System.Windows;
using System.Windows.Input;

using SimpleTextEditor.Model;

namespace SimpleTextEditor.Text.Interface
{
    /// <summary>
    /// Set of common UI methods for backend visual components
    /// </summary>
    public interface ITextInputComponent
    {
        /// <summary>
        /// Updates the constraint size for the text. This should be the entire size of the virtual document, or the
        /// WPF control size. This will cause a re-run of the TextFormatter.
        /// </summary>
        void UpdateSize(Size contorlSize);

        /// <summary>
        /// Appends text using the current caret position
        /// </summary>
        bool ProcessTextInputAtCaret(string text);

        /// <summary>
        /// Removes selected text from the text source
        /// </summary>
        bool ProcessRemoveSelectedText();

        /// <summary>
        /// Processes mouse move event. Returns true if text was re-formatted.
        /// </summary>
        bool ProcessPreviewMouseMove(Point location, MouseButtonState leftButtonState, MouseButtonState rightButtonState);

        /// <summary>
        /// Processes mouse down event. Returns true if text was re-formatted.
        /// </summary>
        bool ProcessMouseDown(Point location, MouseButtonState leftButtonState, MouseButtonState rightButtonState);

        /// <summary>
        /// Processes mouse up event. Returns true if text was re-formatted.
        /// </summary>
        bool ProcessMouseUp(Point location, MouseButtonState leftButtonState, MouseButtonState rightButtonState);

        /// <summary>
        /// Processes control input from the UI. Returns true if there has been reformatted text
        /// </summary>
        bool ProcessControlInput(ControlInput input);
    }
}
