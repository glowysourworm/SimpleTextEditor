namespace SimpleTextEditor.Text.Interface
{
    public interface ITextSource
    {
        int GetLength();
        void AppendText(string text);
        void InsertText(int offset, string text);
        void RemoveText(int offset, int count);
    }
}
