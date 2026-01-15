namespace SimpleTextEditor.Component.Interface
{
    public interface IDocument
    {
        ITextPosition GetCaretPosition();
        void SetCaretPosition(ITextPosition position);

        int GetLineCount();
        int GetWordCount();
        int GetColumnCount();
        ITextLine GetTextLine(int index);

        void Load(string text);
        void Clear();
    }
}
