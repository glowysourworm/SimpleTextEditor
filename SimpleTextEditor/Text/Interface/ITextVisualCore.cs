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
        int SearchText(char character, int startIndex);

        /// <summary>
        /// Initializes the core with the constraint (control) size. This must be called prior to loading,
        /// modifying text, or retrieveing formatted text output.
        /// </summary>
        void Initialize(Size constraintSize);

        /// <summary>
        /// (Mutator) Appends text to the ITextSource, Invalidates cached GlyphRuns, Updates caret position Re-runs the
        /// TextFormatter to produce new visual text elements, updates Caret special "glyph".
        /// </summary>
        void AppendText(string text);

        /// <summary>
        /// (Mutator) Inserts text to the ITextSource, Invalidates cached GlyphRuns, Updates caret position Re-runs the
        /// TextFormatter to produce new visual text elements, updates Caret special "glyph".
        /// </summary>
        void InsertText(int offset, string text);

        /// <summary>
        /// (Mutator) Appends text from the ITextSource, Invalidates cached GlyphRuns, Updates caret position Re-runs the
        /// TextFormatter to produce new visual text elements, updates Caret special "glyph".
        /// </summary>
        void RemoveText(int offset, int count);

        /// <summary>
        /// Returns the current caret render bounds - calculated by the visual core's formatter
        /// </summary>
        Rect GetCaretBounds();

        /// <summary>
        /// Updates the constraint size for the text. This should be the entire size of the virtual document, or the
        /// WPF control size. This will cause a re-run of the TextFormatter.
        /// </summary>
        void UpdateSize(Size contorlSize);

        /// <summary>
        /// Sets mouse information for text selection processing. Returns true if text needs to be invalidated.
        /// </summary>
        /// <param name="topLeft">Top Left with respect to the TextEditor UI</param>
        /// <param name="bottomRight">Bottom Right with respect to the TextEditor UI</param>
        /// <param name="leftMouseButton">Left mouse button state</param>
        bool SetMouseInfo(MouseData mouseData);

        /// <summary>
        /// Returns output from visual core, which is produced immediately after text or control size updates.
        /// </summary>
        SimpleTextVisualOutputData GetOutput();
    }
}
