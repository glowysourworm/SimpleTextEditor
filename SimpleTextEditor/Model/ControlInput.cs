namespace SimpleTextEditor.Model
{
    /// <summary>
    /// Control definitions typically for moving the caret. These will decouple the UI from
    /// the IDocument
    /// </summary>
    public enum ControlInput
    {
        None,
        Backspace,
        DeleteCurrentCharacter,         // Using delete key in place
        LineUp,
        LineDown,
        CharacterLeft,
        CharacterRight,
        WordLeft,
        WordRight,
        EndOfLine,
        BeginningOfLine,
        PageUp,
        PageDown,
        EndOfDocument,
        BeginningOfDocument
    }
}
