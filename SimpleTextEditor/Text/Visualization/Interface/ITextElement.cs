using System.Windows;
using System.Windows.Media.TextFormatting;

using SimpleTextEditor.Model.Interface;

namespace SimpleTextEditor.Text.Visualization.Interface
{
    public interface ITextElement
    {
        ITextPosition Start { get; }
        ITextPosition End { get; }
        int Length { get; }
        bool Contains(ITextPosition position);
        bool Contains(int offset);
        Rect VisualBounds { get; }
        TextLine Element { get; }
    }
}
