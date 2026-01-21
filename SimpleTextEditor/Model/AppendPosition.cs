namespace SimpleTextEditor.Model
{
    /// <summary>
    /// Declares caret positions for appending. Also, the position offset is NORMAL for end of line, and
    /// OVERFLOW OF ONE for EndOfParagraph. This means that the offset to the text source will overflow! So,
    /// this case must be handled. It is for the caret only; and to indicate where / how it is drawn and how
    /// the user's Point (UI) input is translated to the SimpleTextFormatter.
    /// </summary>
    public enum AppendPosition
    {
        /// <summary>
        /// No append position
        /// </summary>
        None = 0,

        /// <summary>
        /// Append position will be only VISUALLY at the end of the line - which is calculated with the last
        /// character's actual offset - making the bounding rectangle's top-right corner available.
        /// </summary>
        EndOfLine = 1,

        /// <summary>
        /// Append position will be ONE EXTRA CHARACTER, which works with MSFT's backend TextFormatter. So,
        /// for this case, the extra offset index must be handled.
        /// </summary>
        Append = 2
    }
}
