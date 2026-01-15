
using SimpleTextEditor.Collection;

namespace SimpleTextEditor.Model
{
    public class Rope
    {
        // Must start with non-zero length
        RopeNode? _root;

        public Rope()
        {
            _root = null;
        }

        /// <summary>
        /// Returns O(1) length of the entire string represented by the rope
        /// </summary>
        public int GetLength()
        {
            if (_root == null)
                return 0;

            return _root.Content.Length;
        }

        public void Insert(int index, string text)
        {
            if (_root == null)
                _root = new RopeNode(new TextString(text.ToArray()));
            else
                _root.Insert(new TextString(text.ToArray()), index);
        }

        public void Append(string text)
        {
            if (_root == null)
                _root = new RopeNode(new TextString(text.ToArray()));
            else
                _root.Append(new TextString(text.ToArray()));
        }

        public void Remove(int index, int count)
        {
            if (_root == null)
                throw new Exception("Trying to remove text from an empty rope!");

            _root.Remove(index, count);
        }

        /// <summary>
        /// Returns copy of a substring portion of the text
        /// </summary>
        public string GetCopy(int index, int count)
        {
            if (_root != null)
                return _root.Content.GetSubString(index, count);
            else
                return string.Empty;
        }

        /// <summary>
        /// Returns a pointer to the entire text store. (DO NOT ALTER THE TEXT STORE!)
        /// </summary>
        public TextString Get()
        {
            if (_root == null)
                return new TextString();

            return _root.Content;
        }
    }
}