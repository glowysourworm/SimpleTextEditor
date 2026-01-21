using SimpleTextEditor.Collection;
using SimpleTextEditor.Model;

namespace SimpleTextEditor.Text.Visualization.Properties
{
    /// <summary>
    /// Dictionary to handle ranges of text properties
    /// </summary>
    public class TextParagraphPropertiesDictionary : RangeDictionary<int, SimpleTextParagraphProperties>
    {
        protected override int FromIndexRange(IndexRange range)
        {
            return range.StartIndex;
        }

        protected override IndexRange ToIndexRange(int key)
        {
            return IndexRange.FromIndices(key, key);
        }
    }
}
