using SimpleTextEditor.Model;
using SimpleTextEditor.Text.Interface;

using SimpleWpf.SimpleCollections.Collection;
using SimpleWpf.SimpleCollections.Extension;

namespace SimpleTextEditor.Text
{
    public class LinearTextSource : ITextSource
    {
        // Primary text source
        private TextEditorString _source;

        // Text Run properties for index ranges
        private SimpleDictionary<IndexRange, SimpleTextRunProperties> _propertyDict;

        // Defaults set by the control
        private readonly SimpleTextRunProperties _defaultProperties;

        public LinearTextSource(SimpleTextRunProperties defaultProperties)
        {
            _source = new TextEditorString();
            _propertyDict = new SimpleDictionary<IndexRange, SimpleTextRunProperties>();
            _defaultProperties = defaultProperties;
        }

        public void AppendText(string text)
        {
            _source.Concat(text.ToArray());
        }

        public TextEditorString Get()
        {
            return _source;
        }

        public int GetLength()
        {
            return _source.Length;
        }

        public SimpleTextRunProperties GetProperties(int offset, out int length)
        {
            var pair = _propertyDict.FirstOrDefault(x => x.Key.Contains(offset));

            // Default length to full unless properties are set
            length = -1;

            if (pair.Key != null && pair.Key.Contains(offset))
            {
                length = pair.Key.Count;
                return pair.Value;
            }


            return _defaultProperties;
        }

        public void SetProperties(IndexRange range, SimpleTextRunProperties properties)
        {
            // Affected Ranges (overlapping)
            var affectedRanges = _propertyDict.Filter(x => x.Key.GetOverlap(range) != null);

            foreach (var affected in affectedRanges)
            {
                // Remove
                if (range.Contains(affected.Key))
                    continue;

                // Portion before start index
                if (affected.Key.StartIndex < range.StartIndex)
                {
                    _propertyDict.Add(IndexRange.FromIndices(affected.Key.StartIndex, range.StartIndex), affected.Value);
                }

                // Portion after end index
                if (affected.Key.EndIndex > range.EndIndex)
                {
                    _propertyDict.Add(IndexRange.FromIndices(range.EndIndex, affected.Key.EndIndex), affected.Value);
                }
            }

            // New Properties
            _propertyDict.Add(range, properties);
        }

        public IndexRange[] GetPropertySlices()
        {
            return _propertyDict.Keys.ToArray();
        }

        public void InsertText(int offset, string text)
        {
            _source.Insert(text.ToArray(), offset);
        }

        public void RemoveText(int offset, int count)
        {
            _source.Remove(offset, count);
        }

        public int Search(char character, int startIndex)
        {
            var result = _source.Get().IndexOf(character);

            if (result >= startIndex)
                return result;

            return -1;
        }
    }
}
