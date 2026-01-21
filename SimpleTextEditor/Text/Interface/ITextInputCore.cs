using System.Windows;

using SimpleTextEditor.Text.Visualization;

namespace SimpleTextEditor.Text.Interface
{
    /// <summary>
    /// Component that acts as a front-end to the visual core. All UI calls should be routed through here to
    /// handle inputs, and caret indexing, which can be an extra complication to the text processing.
    /// </summary>
    public interface ITextInputCore : ITextVisualComponent
    {
        /// <summary>
        /// Initializes the core with the constraint (control) size. This must be called prior to loading,
        /// modifying text, or retrieveing formatted text output.
        /// </summary>
        void Initialize(VisualInputData inputData);

        /// <summary>
        /// Loads text into the backend for processing
        /// </summary>
        void Load(string text);

        /// <summary>
        /// Returns the current caret render bounds - calculated by the visual core's formatter
        /// </summary>
        Rect GetCaretBounds();

        /// <summary>
        /// Returns output from visual core, which is produced immediately after text or control size updates.
        /// </summary>
        VisualOutputData GetOutput();
    }
}
