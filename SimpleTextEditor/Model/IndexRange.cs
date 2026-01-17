using SimpleWpf.RecursiveSerializer.Shared;

namespace SimpleTextEditor.Model
{
    public class IndexRange
    {
        public int StartIndex { get; private set; }
        public int EndIndex { get; private set; }
        public int Count { get { return this.EndIndex - this.StartIndex + 1; } }

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

        public void Shift(int offset)
        {
            this.StartIndex += offset;
            this.EndIndex += offset;
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
            if (end <= start)
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

        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;

            var range = obj as IndexRange;

            if (range == null)
                return false;

            return range.StartIndex == this.StartIndex && range.EndIndex == this.EndIndex;
        }

        public override int GetHashCode()
        {
            return RecursiveSerializerHashGenerator.CreateSimpleHash(this.StartIndex, this.EndIndex);
        }
    }
}
