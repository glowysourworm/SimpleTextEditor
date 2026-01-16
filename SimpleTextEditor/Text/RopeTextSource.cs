using SimpleTextEditor.Model;
using SimpleTextEditor.Text.Interface;

namespace SimpleTextEditor.Text
{
    public class RopeTextSource : ITextSource
    {
        Rope _rope;

        public RopeTextSource()
        {
            _rope = new Rope();
        }

        public void AppendText(string text)
        {
            _rope.Append(text);
        }

        public TextString Get()
        {
            return _rope.Get();
        }

        public int GetLength()
        {
            return _rope.GetLength();
        }

        public void InsertText(int offset, string text)
        {
            _rope.Insert(offset, text);
        }

        public void RemoveText(int offset, int count)
        {
            _rope.Remove(offset, count);
        }

        public int Search(char character, int startIndex)
        {
            throw new NotImplementedException();
        }

        public void SetMouseInfo(MouseData mouseData)
        {
            throw new NotImplementedException();
        }
    }
}
