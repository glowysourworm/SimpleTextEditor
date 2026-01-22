using System.Windows.Media.TextFormatting;

using SimpleTextEditor.Model;
using SimpleTextEditor.Text.Source.Interface;
using SimpleTextEditor.Text.Visualization.Properties.Interface;

namespace SimpleTextEditor.Text.Visualization.Properties
{
    public class TextPropertiesSource : ITextPropertiesSource
    {
        // Text Run properties for index ranges
        private TextPropertiesDictionary _properties;
        private TextParagraphPropertiesDictionary _paragraphProperties;

        // Defaults set by the control
        private readonly ITextSource _textSource;
        private readonly VisualInputData _visualInputData;
        private readonly SimpleTextRunProperties _defaultProperties;
        private readonly SimpleTextParagraphProperties _defaultParagraphProperties;

        public TextPropertiesSource(ITextSource textSource, SimpleTextRunProperties properties, SimpleTextParagraphProperties paragraphProperties)
        {
            _textSource = textSource;
            _properties = new TextPropertiesDictionary();
            _paragraphProperties = new TextParagraphPropertiesDictionary();

            _defaultProperties = properties;
            _defaultParagraphProperties = paragraphProperties;
        }

        public void ClearProperties()
        {
            _properties.Clear();
            _paragraphProperties.Clear();
        }

        public void ClearAffectedProperties(IndexRange range)
        {
            var affectedKeys = _properties.FindKeys(x => x.GetOverlap(range) != null);

            foreach (var key in affectedKeys)
            {
                _properties.Remove(key);
            }
        }

        public IndexRange GetNextModifiedRange(int offset, bool includeCurrentOffset)
        {
            // NOT INCLUDING
            return _properties.FindKey(x => includeCurrentOffset ? x.StartIndex >= offset : x.StartIndex > offset);
        }

        public TextParagraphProperties GetParagraphProperties(int offset, out int length)
        {
            throw new NotImplementedException();
        }

        public TextRunProperties GetProperties(int offset, out int length)
        {
            var range = _properties.FindKey(x => x.Contains(offset));

            // Default length to full unless properties are set
            length = -1;

            if (range != null && range.Contains(offset))
            {
                length = range.Length - offset;
                return _properties.Get(range);
            }

            return _defaultProperties;
        }

        public void SetParagraphProperties(int paragraphNumber, Func<SimpleTextParagraphProperties, SimpleTextParagraphProperties> modifier)
        {
            _paragraphProperties.Modify(paragraphNumber, modifier);
        }

        public void SetProperties(IndexRange range, Func<SimpleTextRunProperties, SimpleTextRunProperties> modifier)
        {
            // Validate
            if (!_textSource.GetRange().Contains(range))
                throw new IndexOutOfRangeException("Cannot set properties outside of source text index space. Be sure to check for EOL, or trailing paragraph character offsets");

            _properties.ModifyRange(range, modifier(_defaultProperties), modifier);
        }
    }
}
