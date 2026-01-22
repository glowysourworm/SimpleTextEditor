using System.Windows.Media.TextFormatting;

using SimpleTextEditor.Model;
using SimpleTextEditor.Text.Visualization.Properties;

namespace SimpleTextEditor.Text.Visualization.Interface
{
    public interface ITextPropertiesSource
    {
        /// <summary>
        /// Sets alternate text properties for a range of text
        /// </summary>
        void SetProperties(IndexRange range, Func<SimpleTextRunProperties, SimpleTextRunProperties> modifier);

        /// <summary>
        /// Sets alternate text paragraph properties for the provided paragraph
        /// </summary>
        void SetParagraphProperties(int paragraphNumber, Func<SimpleTextParagraphProperties, SimpleTextParagraphProperties> modifier);

        /// <summary>
        /// Sets default text settings for any characters in the provided range
        /// </summary>
        void ClearAffectedProperties(IndexRange range);

        /// <summary>
        /// Clears alternate text properties for the entire text
        /// </summary>
        void ClearProperties();

        /// <summary>
        /// Returns the properties for the specified offset, along with the length of that specific text run. If
        /// none are set, then the default properties are returned, along with the length of the text run up until
        /// the next alternate properties range, or the end of the paragraph.
        /// </summary>
        TextRunProperties GetProperties(int offset, out int length);

        /// <summary>
        /// Returns the properties for the specified offset, for the paragraph, along with the length of that
        /// specific text run.
        /// </summary>
        TextParagraphProperties GetParagraphProperties(int offset, out int length);

        /// <summary>
        /// Returns next index range for alternate properties after or equal to the supplied offset
        /// </summary>
        IndexRange GetNextModifiedRange(int offset, bool includeCurrentOffset);
    }
}
