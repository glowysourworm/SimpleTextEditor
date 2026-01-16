using System.Windows;

using SimpleTextEditor.Text.Interface;
using SimpleTextEditor.Text.Visualization;

namespace SimpleTextEditor.Component.Interface
{
    public interface IDocument : ITextVisualCore
    {
        /// <summary>
        /// Returns visual output data from last measure pass
        /// </summary>
        SimpleTextVisualOutputData? LastVisualData { get; }

        /// <summary>
        /// Returns the text offset (for the ITextSource) from the UI point provided. This
        /// is calculated by the cache of UI TextRun elements in the core.
        /// </summary>
        int GetTextOffsetFromUI(Point pointUI);

        ITextPosition GetCaretPosition();
        void SetCaretPosition(ITextPosition position);

        void Load(string text);
        void Clear();
    }
}
