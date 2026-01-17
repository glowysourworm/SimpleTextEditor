using SimpleTextEditor.Model;

namespace SimpleTextEditor.Text.Interface
{
    public interface ITextSource
    {
        TextEditorString Get();
        int GetLength();
        void AppendText(string text);
        void InsertText(int offset, string text);
        void RemoveText(int offset, int count);
        int Search(char character, int startIndex);

        /// <summary>
        /// Returns slices of the text where there are alternate properties set
        /// </summary>
        IndexRange[] GetPropertySlices();

        /// <summary>
        /// Returns the properties for the specified offset, along with the length of that specific text run
        /// </summary>
        SimpleTextRunProperties GetProperties(int offset, out int length);

        /// <summary>
        /// Sets alternate text properties for a range of text
        /// </summary>
        void SetProperties(IndexRange range, SimpleTextRunProperties properties);
    }
}
