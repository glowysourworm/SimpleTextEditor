using System.Windows;

namespace SimpleTextEditor.Model
{
    public class Caret
    {
        public Rect VisualBounds { get; private set; }
        public int Offset { get; private set; }
        public AppendPosition AppendPosition { get; private set; }

        public Caret(int sourceOffset, Rect visualBounds, AppendPosition appendPosition)
        {
            Update(sourceOffset, visualBounds, appendPosition);
        }

        public void Update(int sourceOffset, Rect visualBounds, AppendPosition appendPosition)
        {
            this.VisualBounds = visualBounds;
            this.Offset = sourceOffset;
            this.AppendPosition = appendPosition;
        }

        /// <summary>
        /// Gets offset adjusted to the character in question. This will remove the append position offset.
        /// </summary>
        public int GetInputOffset()
        {
            switch (this.AppendPosition)
            {
                case AppendPosition.None:
                case AppendPosition.EndOfLine:
                    return this.Offset;
                case AppendPosition.Append:
                    return this.Offset - 1;
                default:
                    throw new Exception("Unhandled AppendPosition:  Caret.cs");
            }
        }
    }
}
