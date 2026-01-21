using SimpleTextEditor.Collection;
using SimpleTextEditor.Model;

namespace SimpleTextEditor.Text.Visualization.Properties
{
    /// <summary>
    /// Dictionary to handle ranges of text properties
    /// </summary>
    public class TextPropertiesDictionary : RangeDictionary<IndexRange, SimpleTextRunProperties>
    {
        protected override IndexRange FromIndexRange(IndexRange range)
        {
            return range;
        }

        protected override IndexRange ToIndexRange(IndexRange key)
        {
            return key;
        }
    }
}
