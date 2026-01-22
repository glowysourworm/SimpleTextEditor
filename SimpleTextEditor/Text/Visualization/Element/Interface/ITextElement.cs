using System.Windows;

using SimpleTextEditor.Model.Interface;

namespace SimpleTextEditor.Text.Visualization.Element.Interface
{
    public interface ITextElement : ITextVisual, ITextSpan
    {
        Rect[] CharacterBounds { get; }
        ITextPosition Position { get; }
        int Length { get; }
    }
}
