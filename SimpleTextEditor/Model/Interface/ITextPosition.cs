namespace SimpleTextEditor.Model.Interface
{
    public interface ITextPosition
    {
        /// <summary>
        /// Offset into the raw text source for this position (character)
        /// </summary>
        int Offset { get; }

        /// <summary>
        /// Visual column for this text position
        /// </summary>
        int VisualColumnNumber { get; }

        /// <summary>
        /// Visual line number for this text position
        /// </summary>
        int VisualLineNumber { get; }

        /// <summary>
        /// Paragraph number for this text position
        /// </summary>
        int ParagraphNumber { get; }

        /// <summary>
        /// Returns true if the ITextPosition represents an empty ITextSource
        /// </summary>
        bool IsEmpty();

        /// <summary>
        /// Creates a new ITextPosition with the provided offset
        /// </summary>
        ITextPosition WithOffset(int offset);
    }
}
