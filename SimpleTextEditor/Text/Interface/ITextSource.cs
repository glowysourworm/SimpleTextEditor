using SimpleTextEditor.Model;

namespace SimpleTextEditor.Text.Interface
{
    public interface ITextSource
    {
        TextEditorString Get();
        int GetLength();
        void AppendText(string text, SimpleTextRunProperties properties);
        void InsertText(int offset, string text, SimpleTextRunProperties properties);
        void RemoveText(int offset, int count);
        int Search(char character, int startIndex);
    }
}
