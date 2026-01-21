using SimpleTextEditor.Model;

namespace SimpleTextEditor.Text.Source.Interface
{
    public interface ITextSource
    {
        char GetEOLCharacter();
        char GetChar(int index);
        char[] GetString();
        char[] GetString(int index, int count);
        int GetLength();
        IndexRange GetRange();
        void AppendText(string text);
        void InsertText(int offset, string text);
        void RemoveText(int offset, int count);
        int Search(char character, int startIndex);

        /// <summary>
        /// Returns text lines split by using the end of line character. The EOL character may be used by the TextFormatter.
        /// </summary>
        IList<char[]> GetTextLines(bool keepEOLCharacter);

        /// <summary>
        /// Clears all text from the text source
        /// </summary>
        void ClearText();
    }
}
