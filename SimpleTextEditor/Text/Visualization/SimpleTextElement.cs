using System.Windows.Media.TextFormatting;

using SimpleTextEditor.Component.Interface;

namespace SimpleTextEditor.Text.Visualization
{
    /// <summary>
    /// Container for MSFT (abstract) TextLine, which carries data linking the visualization of
    /// the text source, and the actual text source.
    /// </summary>
    public class SimpleTextElement
    {
        // Visual text line provided by the formatter
        TextLine _textElement;

        // Position of the line (source and visual) (also has line data)
        ITextElementPosition _position;

        public TextLine Element { get { return _textElement; } }
        public ITextElementPosition Position { get { return _position; } }
        public int Length { get { return _textElement.Length; } }

        public SimpleTextElement(TextLine textElement, ITextElementPosition position)
        {
            _textElement = textElement;
            _position = position;
        }
    }
}
