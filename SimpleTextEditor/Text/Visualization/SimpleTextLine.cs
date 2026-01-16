using System.Windows.Media.TextFormatting;

using SimpleTextEditor.Component.Interface;

namespace SimpleTextEditor.Text.Visualization
{
    /// <summary>
    /// Container for MSFT (abstract) TextLine, which carries data linking the visualization of
    /// the text source, and the actual text source.
    /// </summary>
    public class SimpleTextLine
    {
        // Visual text line provided by the formatter
        TextLine _textLine;

        // Position of the line (source and visual) (also has line data)
        ITextPosition _position;

        // Shared paragraph properties
        SimpleTextParagraphProperties _properties;

        // Extra cache of output for debugging
        string _cachedOutput;

        public TextLine Line { get { return _textLine; } }
        public ITextPosition Position { get { return _position; } }
        public SimpleTextParagraphProperties Properties { get { return _properties; } }

        public SimpleTextLine(TextLine textLine, ITextPosition position, SimpleTextParagraphProperties properties, string cachedOutput)
        {
            _textLine = textLine;
            _position = position;
            _properties = properties;
            _cachedOutput = cachedOutput;
        }

        public override string ToString()
        {
            return _cachedOutput;
        }
    }
}
