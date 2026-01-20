using System.Data;

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
        private SimpleDictionary<IndexRange, ITextProperties> _propertyDict;

        // Defaults set by the control
        private readonly ITextProperties _defaultProperties;

        public LinearTextSource(ITextProperties defaultProperties)
        {
            _source = new TextEditorString();
            _propertyDict = new SimpleDictionary<IndexRange, ITextProperties>();
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

        public IList<char[]> GetTextLines(bool keepEOLCharacter)
        {
            return _source.Split('\r', keepEOLCharacter);
        }

        public IDictionary<IndexRange, ITextProperties> GetTextLineProperties(char[] textLine, int textLineOffset, int textLineCharacterOffset)
        {
            // Ranges with alternate properties
            var propertyRanges = GetPropertySlices();

            var currentLine = new string(textLine);
            var propertiesDict = new SimpleDictionary<IndexRange, ITextProperties>();
            var alternateOverlaps = new List<IndexRange>();
            var currentRange = IndexRange.FromStartCount(textLineCharacterOffset, textLine.Length);

            // Alternate Properties
            foreach (var range in propertyRanges)
            {
                var overlap = range.GetOverlap(textLineCharacterOffset, textLineCharacterOffset + textLine.Length - 1);

                // Found Sub-Section w/ Alternate Properties
                //
                if (overlap != null)
                    alternateOverlaps.Add(overlap);
            }

            // Alternate Text
            if (alternateOverlaps.Count > 0)
            {
                var startIndexes = alternateOverlaps.Select(x => x.StartIndex).ToList();
                var endIndexes = alternateOverlaps.Select(x => x.EndIndex).ToList();
                var splitIndices = startIndexes.Concat(endIndexes)
                                               .Where(x => x > currentRange.StartIndex)         // Must split before the character
                                               .Order()                                         // Must order split indices
                                               .ToArray();

                // Alternate Property Indices
                var splits = currentRange.Split(splitIndices);

                foreach (var range in splits)
                {
                    var length = 0;
                    var properties = GetProperties(range.StartIndex, out length);

                    // Create Formatted Text
                    propertiesDict.Add(range, properties);
                }
            }

            // Default Text
            else
            {
                propertiesDict.Add(currentRange, _defaultProperties);
            }

            return propertiesDict;
        }

        public IndexRange GetNextPropertyRange(int offset, bool includeCurrentOffset)
        {
            // NOT INCLUDING
            var pair = _propertyDict.FirstOrDefault(x => includeCurrentOffset ? x.Key.StartIndex >= offset : x.Key.StartIndex > offset);

            if (pair.Key != null)
                return pair.Key;

            return IndexRange.Empty;
        }

        public ITextProperties GetProperties(int offset, out int length)
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

        public void SetProperties(IndexRange range, ITextProperties properties)
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

        public void ClearProperties()
        {
            _propertyDict.Clear();
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
            for (int index = startIndex; index < _source.Get().Length; index++)
            {
                if (_source.Get()[index] == character)
                    return index;
            }

            return -1;
        }
    }
}
