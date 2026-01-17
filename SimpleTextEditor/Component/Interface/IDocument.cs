using System.Windows;

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
        /// Returns the text offset (for the ITextSource) from the UI point provided. This
        /// is calculated by the cache of UI TextRun elements in the core.
        /// </summary>
        void ProcessUILeftClick(Point pointUI);
        void ProcessControlInput(ControlInput input);

        void ProcessInputText(string inputText);
        void ProcessRemoveText(int offset, int count);

        /// <summary>
        /// Returns visible list of visual elements that may be rendered
        /// </summary>
        IEnumerable<SimpleTextElement> GetVisualElements();
        Size Measure(Size availableSize);
        Rect GetCaretBounds();

        void Load(string text);
        void Clear();
    }
}
