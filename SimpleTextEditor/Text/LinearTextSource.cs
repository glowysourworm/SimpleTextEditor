using SimpleTextEditor.Model;
using SimpleTextEditor.Text.Interface;

namespace SimpleTextEditor.Text
{
    public class LinearTextSource : ITextSource
    {
        TextString _source;

        public LinearTextSource()
        {
            _source = new TextString();
        }

        public void AppendText(string text)
        {
            _source.Concat(text.ToArray());
        }

        public TextString Get()
        {
            return _source;
        }

        public int GetLength()
        {
            return _source.Length;
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

        public void SetMouseInfo(MouseData mouseData)
        {
            throw new NotImplementedException();
        }
    }
}
