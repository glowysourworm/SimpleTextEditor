using System.Windows;

using SimpleTextEditor.Text.Source.Interface;
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
        bool IsInvalid { get; }
        bool IsInitialized { get; }
        int TextLength { get; }

        /// <summary>
        /// Initializes the core with the constraint (control) size. This must be called prior to loading,
        /// modifying text, or retrieveing formatted text output.
        /// </summary>
        void Initialize(SimpleTextEditorFormatter formatter, ITextSource textSource, VisualInputData inputData);

        /// <summary>
        /// Returns output from visual core, which is produced immediately after text or control size updates.
        /// </summary>
        VisualOutputData GetOutput();

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
        /// (Mutator) Removes all text from the text source
        /// </summary>
        void ClearText();

        /// <summary>
        /// Updates constraint size for the visual core
        /// </summary>
        void UpdateSize(Size contorlSize);

        /// <summary>
        /// Invalidates TextFormatter's TextRun cache
        /// </summary>
        void Invalidate();

        /// <summary>
        /// Invalidates TextFormatter's TextRun cache
        /// </summary>
        void Invalidate(int startIndex, int additionLength, int removalLength);
    }
}
