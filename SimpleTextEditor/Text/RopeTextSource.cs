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

        public void AppendText(string text, SimpleTextRunProperties properties)
        {
            _rope.Append(text, properties);
        }

        public TextEditorString Get()
        {
            return _rope.Get();
        }

        public int GetLength()
        {
            return _rope.GetLength();
        }

        public void InsertText(int offset, string text, SimpleTextRunProperties properties)
        {
            _rope.Insert(offset, text, properties);
        }

        public void SetProperties(int offset, int count, SimpleTextRunProperties properties)
        {
            throw new NotImplementedException();
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
