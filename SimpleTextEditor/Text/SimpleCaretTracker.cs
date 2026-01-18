namespace SimpleTextEditor.Text
{
    public class SimpleCaretTracker
    {
        // Keeps track of caret position (this will be the character AFTER to caret rendering). The last
        // position is off the end of the text source by one.
        private int _caretPosition;

        /// <summary>
        /// Gets the position of the caret. The final caret position is off the end 
        /// of the text by one.
        /// </summary>
        public int GetCaretPosition()
        {
            return _caretPosition;
        }

        /// <summary>
        /// Sets the caret position based on text offset. The final caret position is off the
        /// end of the text by one
        /// </summary>
        public void SetCaretPosition(int textOffset)
        {
            _caretPosition = textOffset + 1;
        }
    }
}
