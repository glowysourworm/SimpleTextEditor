using System.Windows.Media.TextFormatting;

namespace SimpleTextEditor.Text.Visualization.Interface
{
    public interface ITextProperties
    {
        TextRunProperties Properties { get; }
        TextParagraphProperties ParagraphProperties { get; }
    }
}
