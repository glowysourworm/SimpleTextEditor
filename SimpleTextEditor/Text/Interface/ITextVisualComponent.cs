using System.Windows;

using SimpleTextEditor.Text.Visualization;

namespace SimpleTextEditor.Text.Interface
{
    public interface ITextVisualComponent
    {
        bool IsInvalid { get; }
        bool IsInitialized { get; }

        /// <summary>
        /// Retrieves last output from visual component.
        /// </summary>
        VisualOutputData GetOutput();

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
