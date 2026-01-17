using System.Windows.Media.TextFormatting;

namespace SimpleTextEditor.Text.Interface
{
    public interface ITextProperties
    {
        TextRunProperties Properties { get; }
        TextParagraphProperties ParagraphProperties { get; }
    }
}
