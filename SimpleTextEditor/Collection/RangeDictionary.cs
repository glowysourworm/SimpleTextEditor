using SimpleTextEditor.Model;

using SimpleWpf.SimpleCollections.Collection;
using SimpleWpf.SimpleCollections.Extension;

namespace SimpleTextEditor.Collection
{
    public abstract class RangeDictionary<K, V>
    {
        SimpleDictionary<IndexRange, V> _dictionary;

        public RangeDictionary()
        {
            _dictionary = new SimpleDictionary<IndexRange, V>();
        }

        /// <summary>
        /// Creates an IndexRange from a given key. This class must be implemented to work with the
        /// modify methods for a ranged value.
        /// </summary>
        protected abstract IndexRange ToIndexRange(K key);
        protected abstract K FromIndexRange(IndexRange range);

        public V this[K key]
        {
            get { return _dictionary[ToIndexRange(key)]; }
        }

        public void Add(K key, V value)
        {
            var indexRange = ToIndexRange(key);

            _dictionary[indexRange] = value;
        }

        /// <summary>
        /// Returns key for index range if it exists
        /// </summary>
        public V Get(IndexRange indexRange)
        {
            return _dictionary[indexRange];
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        /// <summary>
        /// Finds key value pair from key-based predicate
        /// </summary>
        public K FindKey(Func<K, bool> keyPredicate)
        {
            var pair = _dictionary.FirstOrDefault(x => keyPredicate(FromIndexRange(x.Key)));

            if (pair.Key == null)
                return default(K);

            if (_dictionary.ContainsKey(pair.Key))
                return FromIndexRange(pair.Key);

            return FromIndexRange(pair.Key);
        }

        /// <summary>
        /// Modifies value for the selected element
        /// </summary>
        /// <param name="key">Key to the element</param>
        /// <param name="modifier">Modifier function to set the value</param>
        /// <exception cref="Exception">Key not found in dictionary</exception>
        public void Modify(K key, Func<V, V> modifier)
        {
            var indexRange = ToIndexRange(key);

            if (_dictionary.ContainsKey(indexRange))
                _dictionary[indexRange] = modifier(_dictionary[indexRange]);

            else
                throw new Exception("Key not found in RangeDictionary");
        }

        /// <summary>
        /// Modifies any values in the provided range using provided modifier. A default value may / must be provided
        /// for any portions of the range that have not yet been set into the dictionary. This method will favor the 
        /// new set value.
        /// </summary>
        /// <param name="range">Target range (not necessarily already part of the dictionary)</param>
        /// <param name="defaultValue">Default value for any portions of the range that have not yet been set.</param>
        /// <param name="modifier">Modifier function for current values in the specified range</param>
        public void ModifyRange(IndexRange range, V defaultValue, Func<V, V> modifier)
        {
            // Affected Ranges (overlapping)
            var affectedRanges = _dictionary.Filter(x => x.Key.GetOverlap(range) != null);

            foreach (var affected in affectedRanges)
            {
                // Remove
                if (range.Contains(affected.Key))
                    continue;

                // Portion before start index
                if (affected.Key.StartIndex < range.StartIndex)
                {
                    _dictionary.Add(IndexRange.FromIndices(affected.Key.StartIndex, range.StartIndex), affected.Value);
                }

                // Portion after end index
                if (affected.Key.EndIndex > range.EndIndex)
                {
                    _dictionary.Add(IndexRange.FromIndices(range.EndIndex, affected.Key.EndIndex), affected.Value);
                }
            }

            _dictionary.Add(range, defaultValue);
        }
    }
}
