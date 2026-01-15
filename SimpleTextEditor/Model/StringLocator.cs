namespace SimpleTextEditor.Model
{
    /// <summary>
    /// Class to locate substring inside of a parent string
    /// </summary>
    public class StringLocator
    {
        public int Index { get; }
        public int Length { get; }

        private readonly string _parentString;

        public StringLocator(string parent, string substring)
        {
            _parentString = parent;
            this.Index = parent.IndexOf(substring);
            this.Length = substring.Length;
        }

        public StringLocator(string parent, char firstCharacter, bool inclusiveEndpoint = true)
        {
            var index = parent.IndexOf(firstCharacter);

            if (index == -1 || (index == 0 && !inclusiveEndpoint))
                throw new ArgumentException("Invalid substring locator");

            _parentString = parent;

            this.Index = inclusiveEndpoint ? index : index - 1;
            this.Length = this.Index + 1;
        }

        public StringLocator(string parent, int index, int length)
        {
            _parentString = parent;
            this.Index = index;
            this.Length = length;
        }

        public string GetParentString()
        {
            return _parentString;
        }
        public string GetSubString()
        {
            return _parentString.Substring(this.Index, this.Length);
        }
        public bool ContainsIndex(int index)
        {
            return (index >= this.Index) && (index < this.Index + this.Length);
        }

        public override string ToString()
        {
            return GetSubString();
        }
    }
}
