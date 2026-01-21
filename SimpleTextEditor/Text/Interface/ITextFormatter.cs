using SimpleTextEditor.Text.Source.Interface;
using SimpleTextEditor.Text.Visualization;

namespace SimpleTextEditor.Text.Interface
{
    public interface ITextFormatter : ITextVisualComponent
    {
        /// <summary>
        /// Initializes the formatter with the ITextSource, and visual input data. This must be called prior
        /// to retrieving data.
        /// </summary>
        void Initialize(ITextRunProvider textRunProvider, ITextSource textSource, VisualInputData visualInputData);
    }
}
