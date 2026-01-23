using System.Windows;

using SimpleTextEditor.Model.Interface;
using SimpleTextEditor.Text.Visualization.Element.Interface;

namespace SimpleTextEditor.Text.Visualization.Element
{
    /// <summary>
    /// Small class to store and pre-compute some of MSFT's output for use
    /// </summary>
    public class TextElement : ITextElement
    {
        public Rect[] CharacterBounds { get; }
        public ITextPosition Position { get; }
        public Rect VisualBounds { get; }
        public int Length { get; }

        public TextElement(Rect[] characterBounds, Rect visualBounds, ITextPosition position)
        {
            this.Position = position;
            this.CharacterBounds = characterBounds;
            this.VisualBounds = visualBounds;
            this.Length = characterBounds.Length;
        }
    }
}
