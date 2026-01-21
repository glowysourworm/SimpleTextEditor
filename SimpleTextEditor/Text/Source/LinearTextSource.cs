using SimpleTextEditor.Model;
using SimpleTextEditor.Text.Source.Interface;
using SimpleTextEditor.Text.Visualization;

namespace SimpleTextEditor.Text.Source
{
    public class LinearTextSource : ITextSource
    {
        const char _EOL = '\r';

        // Primary text source
        private TextEditorString _source;

        // Defaults set by the control
        private readonly VisualInputData _visualInputData;

        public LinearTextSource(VisualInputData visualInputData)
        {
            _source = new TextEditorString();
            _visualInputData = visualInputData;
        }

        public void AppendText(string text)
        {
            _source.Concat(text.ToArray());
        }
        public char GetEOLCharacter()
        {
            return _EOL;
        }
        public char GetChar(int index)
        {
            return _source.Get()[index];
        }
        public char[] GetString()
        {
            return _source.Get();
        }

        public char[] GetString(int index, int count)
        {
            throw new NotImplementedException();
        }
        public int GetLength()
        {
            return _source.Length;
        }

        public IndexRange GetRange()
        {
            return IndexRange.FromStartCount(0, _source.Length);
        }

        public IList<char[]> GetTextLines(bool keepEOLCharacter)
        {
            return _source.Split('\r', keepEOLCharacter);
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
            for (int index = startIndex; index < _source.Get().Length; index++)
            {
                if (_source.Get()[index] == character)
                    return index;
            }

            return -1;
        }

        public void ClearText()
        {
            _source = new TextEditorString();
        }

        public override string ToString()
        {
            return string.Format("Length={0} Text={1}", _source.Length, _source.ToString());
        }
    }
}
