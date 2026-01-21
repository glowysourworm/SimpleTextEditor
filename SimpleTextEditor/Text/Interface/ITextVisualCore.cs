using SimpleTextEditor.Model.Interface;
using SimpleTextEditor.Text.Source.Interface;
using SimpleTextEditor.Text.Visualization;

namespace SimpleTextEditor.Text.Interface
{
    /// <summary>
    /// Produces text rendering using MSFT advanced text formatting. Consumes a TextSource (SimpleTextStore), and shares
    /// a reference to this primary text store. Produces visual lines, paragraphs, spans, and anything else visually required
    /// for a WPF rendering of the text.
    /// </summary>
    public interface ITextVisualCore : ITextVisualComponent
    {
        /// <summary>
        /// Initializes the core with the constraint (control) size. This must be called prior to loading,
        /// modifying text, or retrieveing formatted text output.
        /// </summary>
        void Initialize(ITextFormatter formatter, ITextSource textSource, VisualInputData inputData);

        /// <summary>
        /// (Mutator) Appends text to the ITextSource, Invalidates cached GlyphRuns, Updates caret position Re-runs the
        /// TextFormatter to produce new visual text elements, updates Caret special "glyph". Returns the new caret position.
        /// </summary>
        ITextPosition AppendText(string text);

        /// <summary>
        /// (Mutator) Inserts text to the ITextSource, Invalidates cached GlyphRuns, Updates caret position Re-runs the
        /// TextFormatter to produce new visual text elements, updates Caret special "glyph". Returns the new caret position.
        /// </summary>
        ITextPosition InsertText(int offset, string text);

        /// <summary>
        /// (Mutator) Appends text from the ITextSource, Invalidates cached GlyphRuns, Updates caret position Re-runs the
        /// TextFormatter to produce new visual text elements, updates Caret special "glyph". Returns the new caret position.
        /// </summary>
        ITextPosition RemoveText(int offset, int count);

        /// <summary>
        /// (Mutator) Removes all text from the text source. Returns the new caret position.
        /// </summary>
        ITextPosition ClearText();
    }
}
