using SimpleTextEditor.Model;
using SimpleTextEditor.Text.Interface;

namespace SimpleTextEditor.Text
{
    public class LinearTextSource : ITextSource
    {
        TextEditorString _source;

        public LinearTextSource()
        {
            _source = new TextEditorString();
        }

        public void AppendText(string text, SimpleTextRunProperties properties)
        {
            _source.Concat(text.ToArray(), properties);
        }

        public TextEditorString Get()
        {
            return _source;
        }

        public int GetLength()
        {
            return _source.Length;
        }

        public void InsertText(int offset, string text, SimpleTextRunProperties properties)
        {
            _source.Insert(text.ToArray(), offset, properties);
        }

        public void SetProperties(int offset, int count, SimpleTextRunProperties properties)
        {
            _source.SetProperties(offset, count, properties);
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
    }
}
