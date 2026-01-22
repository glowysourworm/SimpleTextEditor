using System.Windows;

using SimpleTextEditor.Model.Interface;

namespace SimpleTextEditor.Model
{
    public class Caret
    {
        public Rect VisualBounds { get; private set; }
        public ITextPosition Position { get; private set; }
        public CaretPosition CaretPosition { get; private set; }

        public Caret(ITextPosition position, Rect visualBounds, CaretPosition caretPosition)
        {
            Update(position, visualBounds, caretPosition);
        }

        public void Update(ITextPosition position, Rect visualBounds, CaretPosition caretPosition)
        {
            this.VisualBounds = visualBounds;
            this.Position = position;
            this.CaretPosition = caretPosition;
        }
    }
}
