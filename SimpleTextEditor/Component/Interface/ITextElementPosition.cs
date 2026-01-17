using System.Windows;

namespace SimpleTextEditor.Component.Interface
{
    public interface ITextElementPosition : ITextPosition
    {
        /// <summary>
        /// This is the X,Y location of this visual text element (top left corner)
        /// </summary>
        Point VisualPosition { get; }
    }
}
