namespace SimpleTextEditor.Component.Interface
{
    public interface ITextPosition
    {
        /// <summary>
        /// Offset into the text source for this position (character)
        /// </summary>
        int SourceOffset { get; }

        /// <summary>
        /// Offset into the text source for this position's line number
        /// </summary>
        int SourceLineNumber { get; }

        /// <summary>
        /// Visual column for this text position
        /// </summary>
        int VisualColumn { get; }

        /// <summary>
        /// Visual line number for this text position
        /// </summary>
        int VisualLineNumber { get; }

        /// <summary>
        /// Paragraph number for this text position
        /// </summary>
        int ParagraphNumber { get; }
    }
}
