using System.Windows;

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
        /// <summary>
        /// Sets reference to the simple text store
        /// </summary>
        ITextSource GetSource();

        /// <summary>
        /// Measure override for WPF controls. Returns output data for rendering, using the provided control constraint.
        /// </summary>
        SimpleTextVisualOutputData Measure(Size constraint);
    }
}
