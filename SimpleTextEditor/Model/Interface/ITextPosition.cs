namespace SimpleTextEditor.Model.Interface
{
    public interface ITextPosition
    {
        /// <summary>
        /// Offset into the text source for this position (character)
        /// </summary>
        int Offset { get; }

        /// <summary>
        /// Offset into the text source for this position's line index
        /// </summary>
        int LineOffset { get; }

        /// <summary>
        /// Offset into the text source for this position's line's first character
        /// </summary>
        int LineFirstOffset { get; }

        /// <summary>
        /// Index for the element in the visual elements collection
        /// </summary>
        int VisualElementIndex { get; }

        /// <summary>
        /// Is there a special case for this text position? Is this instance being used 
        /// for appending the caret?
        /// </summary>
        AppendPosition AppendPosition { get; }

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
        /// Returns a new text position with a new append position parameter. Setting modifyOffset 
        /// to true will force the ITextPosition to supply a new Offset parameter (to the return
        /// value) if the append position is changed from its previous value; and only for the 
        /// change implied by the AppendPosition parameter (+1 for Append, or -1 for not Append, 
        /// if it was previously set to Append)
        /// </summary>
        ITextPosition AsAppend(AppendPosition newAppendPosition, bool modifyOffset);
    }
}
