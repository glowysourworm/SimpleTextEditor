
using SimpleTextEditor.Collection;

namespace SimpleTextEditor.Model
{
    public class Rope
    {
        RopeNode _root;

        public Rope()
        {
            _root = new RopeNode(new TextString());
        }

        public void Insert(int index, string text)
        {
            _root.Insert(new TextString(text.ToArray()), index);
        }

        public void Remove(int index, int count)
        {
            _root.Remove(index, count);
        }

        public string Get(int index, int count)
        {
            return _root.Content.GetSubString(index, count);
        }
    }
}