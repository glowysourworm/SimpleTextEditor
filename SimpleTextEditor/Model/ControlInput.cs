namespace SimpleTextEditor.Model
{
    /// <summary>
    /// Control definitions typically for moving the caret. These will decouple the UI from
    /// the IDocument
    /// </summary>
    public enum ControlInput
    {
        ArrowUp,
        ArrowDown,
        ArrowLeft,
        ArrowRight,
        WordLeft,
        WordRight,
        EndOfLine,
        BeginningOfLine,
        PageUp,
        PageDown
    }
}
