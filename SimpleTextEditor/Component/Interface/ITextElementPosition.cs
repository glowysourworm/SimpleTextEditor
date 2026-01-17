using System.Windows;

namespace SimpleTextEditor.Component.Interface
{
    public interface ITextElementPosition : ITextPosition
    {
        /// <summary>
        /// This is the visual text element UI bounding box
        /// </summary>
        Rect VisualBounds { get; }
    }
}
