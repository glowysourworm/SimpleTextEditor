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

        // Shared paragraph properties
        //SimpleTextParagraphProperties _properties;
        SimpleTextRunProperties _properties;

        // Extra cache of output for debugging
        string _cachedOutput;

        public TextLine Element { get { return _textElement; } }
        public ITextElementPosition Position { get { return _position; } }
        public int Length { get { return _textElement.Length; } }
        public SimpleTextRunProperties Properties { get { return _properties; } }

        public SimpleTextElement(TextLine textElement, ITextElementPosition position, SimpleTextRunProperties properties, string cachedOutput)
        {
            _textElement = textElement;
            _position = position;
            _cachedOutput = cachedOutput;
            _properties = properties;
        }

        public override string ToString()
        {
            return _cachedOutput;
        }
    }
}
