using System.Windows;

using SimpleTextEditor.Model;
using SimpleTextEditor.Text.Visualization;

namespace SimpleTextEditor.Text.Interface
{
    /// <summary>
    /// Produces text rendering using MSFT advanced text formatting. Consumes a TextSource (SimpleTextStore), and shares
    /// a reference to this primary text store. Produces visual lines, paragraphs, spans, and anything else visually required
    /// for a WPF rendering of the text.
    /// </summary>
    public interface ITextVisualCore
    {
        string GetTextCopy();
        int GetTextLength();
        void AppendText(string text);
        void InsertText(int offset, string text);
        void RemoveText(int offset, int count);
        int SearchText(char character, int startIndex);

        /// <summary>
        /// Returns the current caret render bounds - calculated by the visual core's formatter
        /// </summary>
        Rect GetCaretBounds();

        /// <summary>
        /// Measure override for WPF controls. Returns output data for rendering, using the provided control constraint.
        /// </summary>
        SimpleTextVisualOutputData Measure(Size constraint);

        /// <summary>
        /// Sets mouse information for text selection processing. The text run properties must be pre-set with mouse
        /// selection properties (for the UI) during formatting. The Point objects may be null.
        /// </summary>
        /// <param name="topLeft">Top Left with respect to the TextEditor UI</param>
        /// <param name="bottomRight">Bottom Right with respect to the TextEditor UI</param>
        /// <param name="leftMouseButton">Left mouse button state</param>
        void SetMouseInfo(MouseData mouseData);
    }
}
