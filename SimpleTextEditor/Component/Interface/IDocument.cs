using SimpleTextEditor.Text.Interface;
using SimpleTextEditor.Text.Visualization;

namespace SimpleTextEditor.Component.Interface
{
    public interface IDocument : ITextVisualCore, ITextSource
    {
        /// <summary>
        /// Returns visual output data from last measure pass
        /// </summary>
        public SimpleTextVisualOutputData? LastVisualData { get; }

        ITextPosition GetCaretPosition();
        void SetCaretPosition(ITextPosition position);

        void Load(string text);
        void Clear();
    }
}
