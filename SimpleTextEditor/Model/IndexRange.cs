using SimpleWpf.RecursiveSerializer.Shared;

namespace SimpleTextEditor.Model
{
    public class IndexRange
    {
        public int StartIndex { get; private set; }
        public int EndIndex { get; private set; }
        public int Length { get { return this.EndIndex - this.StartIndex + 1; } }

        private IndexRange(int startIndex, int endIndex)
        {
            if (endIndex < startIndex)
                throw new ArgumentException("CharacterRange parameters must be in integer order");

            this.StartIndex = startIndex;
            this.EndIndex = endIndex;
        }

        public static IndexRange FromIndices(int startIndex, int endIndex)
        {
            return new IndexRange(startIndex, endIndex);
        }
        public static IndexRange FromStartCount(int startIndex, int count)
        {
            return new IndexRange(startIndex, startIndex + count - 1);
        }

        public IndexRange? GetOverlap(IndexRange otherRange)
        {
            return GetOverlap(otherRange.StartIndex, otherRange.EndIndex);
        }

        public bool Contains(IndexRange otherRange)
        {
            return this.StartIndex <= otherRange.StartIndex && this.EndIndex >= otherRange.EndIndex;
        }

        public bool Contains(int offset)
        {
            return this.StartIndex <= offset && this.EndIndex >= offset;
        }

        public void Shift(int offset)
        {
            this.StartIndex += offset;
            this.EndIndex += offset;
        }

        public IndexRange[] Split(int offset)
        {
            if (offset < this.StartIndex ||
                offset > this.EndIndex)
                throw new IndexOutOfRangeException("Must split IndexRange after the first index, and before or equal to the last");

            var range1 = IndexRange.FromIndices(this.StartIndex, offset - 1);
            var range2 = IndexRange.FromIndices(offset, this.EndIndex);

            return new IndexRange[] { range1, range2 };
        }

        public IndexRange[] Split(params int[] offsets)
        {
            if (offsets.Length == 0)
                throw new ArgumentException("Split must contain at least one offset");

            var result = new List<IndexRange>();
            var index = 0;

            while (index < offsets.Length)
            {
                if (offsets[index] < this.StartIndex ||
                    offsets[index] > this.EndIndex)
                    throw new IndexOutOfRangeException("Must split IndexRange after the first index, and before or equal to the last");

                var range = IndexRange.FromIndices(index == 0 ? this.StartIndex : offsets[index - 1], offsets[index] - 1);

                result.Add(range);

                index++;
            }

            // Final Split
            result.Add(IndexRange.FromIndices(offsets[offsets.Length - 1], this.EndIndex));

            return result.ToArray();
        }

        public IndexRange Add(int offset)
        {
            return new IndexRange(this.StartIndex + offset, this.EndIndex + offset);
        }

        public IndexRange Subtract(int offset)
        {
            return new IndexRange(this.StartIndex - offset, this.EndIndex - offset);
        }

        public IndexRange? GetOverlap(int start, int end)
        {
            if (end < start)
                throw new ArgumentException("CharacterRange parameters must be in integer order");

            if (start < this.StartIndex)
            {
                if (end >= this.StartIndex)
                    return new IndexRange(this.StartIndex, end);
            }
            else if (end > this.EndIndex)
            {
                if (start <= this.EndIndex)
                    return new IndexRange(start, end);
            }
            else if (start >= this.StartIndex && end <= this.EndIndex)
            {
                return new IndexRange(start, end);
            }

            return null;
        }

        protected static bool ValueCompare(IndexRange range1, IndexRange range2)
        {
            if (ReferenceEquals(range1, null))
                return ReferenceEquals(range2, null);

            if (ReferenceEquals(range2, null))
                return ReferenceEquals(range1, null);

            return range1.StartIndex == range2.StartIndex && range1.EndIndex == range2.EndIndex;
        }

        public override bool Equals(object? obj)
        {
            return ValueCompare(this, obj as IndexRange);
        }

        public override int GetHashCode()
        {
            return RecursiveSerializerHashGenerator.CreateSimpleHash(this.StartIndex, this.EndIndex);
        }
    }
}
