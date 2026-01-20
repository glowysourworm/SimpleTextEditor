using System.Windows;

using SimpleTextEditor.Component;
using SimpleTextEditor.Component.Interface;

namespace SimpleTextEditor.Text
{
    public class SimpleCaretTracker
    {
        // Keeps track of caret position (this will be the character AFTER to caret rendering). The last
        // position is off the end of the text source by one.
        private ITextElementPosition _caretPosition;

        /// <summary>
        /// Gets the position of the caret. The final caret position is off the end 
        /// of the text by one.
        /// </summary>
        public ITextElementPosition GetCaretPosition()
        {
            return _caretPosition;
        }

        /// <summary>
        /// Sets the caret position based on text offset. The final caret position is off the
        /// end of the text by one
        /// </summary>
        public void Update(Rect visualBounds, int sourceOffset, int sourceLineNumber, int visualColumn, int visualLineNumber, int paragraphNumber)
        {
            if (_caretPosition == null)
                _caretPosition = new TextElementPosition(visualBounds, sourceOffset, sourceLineNumber, visualColumn, visualLineNumber, paragraphNumber);

            else
                _caretPosition.Update(visualBounds, sourceOffset, sourceLineNumber, visualColumn, visualLineNumber, paragraphNumber);
        }
    }
}
