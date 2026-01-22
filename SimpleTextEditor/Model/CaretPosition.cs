namespace SimpleTextEditor.Model
{
    /// <summary>
    /// Declares caret positions for appending. Also, the position offset is NORMAL for end of line, and
    /// OVERFLOW OF ONE for EndOfParagraph. This means that the offset to the text source will overflow! So,
    /// this case must be handled. It is for the caret only; and to indicate where / how it is drawn and how
    /// the user's Point (UI) input is translated to the SimpleTextFormatter.
    /// </summary>
    public enum CaretPosition
    {
        BeforeCharacter = 0,
        AfterCharacter = 1
    }
}
