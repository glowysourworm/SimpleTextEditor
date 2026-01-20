using System.Windows;

namespace SimpleTextEditor.Model
{
    public class Caret
    {
        public Rect VisualBounds { get; private set; }
        public int SourceOffset { get; private set; }
        public bool IsAppendPosition { get; private set; }

        public Caret(int sourceOffset, Rect visualBounds, bool isAppendPosition)
        {
            Update(sourceOffset, visualBounds, isAppendPosition);
        }

        public void Update(int sourceOffset, Rect visualBounds, bool isAppendPosition)
        {
            this.VisualBounds = visualBounds;
            this.SourceOffset = sourceOffset;
            this.IsAppendPosition = isAppendPosition;
        }
    }
}
