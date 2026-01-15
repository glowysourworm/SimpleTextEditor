namespace SimpleTextEditor.Component.Interface
{
    public interface ITextLine
    {
        /// <summary>
        /// Line number for the text source
        /// </summary>
        int SourceLineNumber { get; }

        /// <summary>
        /// Line number for the visualization of the text source
        /// </summary>
        int VisualLineNumber { get; }
    }
}
