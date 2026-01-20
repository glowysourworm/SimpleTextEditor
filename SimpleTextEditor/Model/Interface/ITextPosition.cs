namespace SimpleTextEditor.Model.Interface
{
    public interface ITextPosition
    {
        /// <summary>
        /// Offset into the text source for this position (character)
        /// </summary>
        int SourceOffset { get; }

        /// <summary>
        /// Index for the element in the visual elements collection
        /// </summary>
        int ElementIndex { get; }

        /// <summary>
        /// Offset into the text source for this position's line number
        /// </summary>
        int SourceLineNumber { get; }

        /// <summary>
        /// This situation occurs when there is a line break. The character
        /// was followed by either a line break, or a special return character \r
        /// </summary>
        bool IsAtEndOfLine { get; }

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
