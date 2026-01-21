using System.Windows;

namespace SimpleTextEditor.Model
{
    public class Caret
    {
        public Rect VisualBounds { get; private set; }
        public int SourceOffset { get; private set; }
        public AppendPosition AppendPosition { get; private set; }

        public Caret(int sourceOffset, Rect visualBounds, AppendPosition appendPosition)
        {
            Update(sourceOffset, visualBounds, appendPosition);
        }

        public void Update(int sourceOffset, Rect visualBounds, AppendPosition appendPosition)
        {
            this.VisualBounds = visualBounds;
            this.SourceOffset = sourceOffset;
            this.AppendPosition = appendPosition;
        }
    }
}
