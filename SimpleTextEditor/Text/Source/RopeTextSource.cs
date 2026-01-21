using SimpleTextEditor.Model;
using SimpleTextEditor.Text.Source.Interface;

namespace SimpleTextEditor.Text.Source
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

        public TextEditorString Get()
        {
            return _rope.Get();
        }
        public char GetChar(int index)
        {
            return _rope.Get().Get()[index];
        }
        public int GetLength()
        {
            return _rope.GetLength();
        }
        public IList<char[]> GetTextLines(bool keepEOLCharacter)
        {
            throw new NotImplementedException();
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

        public IndexRange[] GetPropertySlices()
        {
            throw new NotImplementedException();
        }

        public IndexRange GetNextModifiedRange(int offset, bool includeCurrentOffset)
        {
            throw new NotImplementedException();
        }

        public void ClearText()
        {
            throw new NotImplementedException();
        }

        public char GetEOLCharacter()
        {
            throw new NotImplementedException();
        }

        public char[] GetString()
        {
            throw new NotImplementedException();
        }

        public char[] GetString(int index, int count)
        {
            throw new NotImplementedException();
        }

        public IndexRange GetRange()
        {
            throw new NotImplementedException();
        }
    }
}
