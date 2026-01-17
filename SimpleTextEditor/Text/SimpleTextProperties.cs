using System.Windows.Media.TextFormatting;

using SimpleTextEditor.Text.Interface;

namespace SimpleTextEditor.Text
{
    public class SimpleTextProperties : ITextProperties
    {
        public TextRunProperties Properties { get; }
        public TextParagraphProperties ParagraphProperties { get; }

        public SimpleTextProperties(TextRunProperties properties, TextParagraphProperties paragraphProperties)
        {
            this.Properties = properties;
            this.ParagraphProperties = paragraphProperties;
        }
    }
}
