using SimpleTextEditor.Model.Interface;

using SimpleWpf.Extensions;
using SimpleWpf.RecursiveSerializer.Shared;

namespace SimpleTextEditor.Model
{
    /// <summary>
    /// Represents a position of text for both source and visual representations
    /// </summary>
    public class TextPosition : ITextPosition
    {
        public int Offset { get; }
        public int ParagraphNumber { get; }
        public int VisualColumnNumber { get; }
        public int VisualLineNumber { get; }

        public TextPosition()
        {

        }
        public TextPosition(int sourceOffset,
                            int visualColumnNumber,
                            int visualLineNumber,
                            int paragraphNumber)
        {
            if (sourceOffset < 0)
                throw new ArgumentException("ITextPosition Offset should not be less than 0");

            this.Offset = sourceOffset;
            this.VisualColumnNumber = visualColumnNumber;
            this.VisualLineNumber = visualLineNumber;
            this.ParagraphNumber = paragraphNumber;
        }

        public bool IsEmpty()
        {
            return this.Offset == 0 &&
                   this.VisualColumnNumber == 0 &&
                   this.VisualLineNumber == 0 &&
                   this.ParagraphNumber == 0;
        }

        public ITextPosition WithOffset(int offset)
        {
            return new TextPosition(offset, this.VisualColumnNumber, this.VisualLineNumber, this.ParagraphNumber);
        }

        public static TextPosition CreateEmpty()
        {
            return new TextPosition();
        }

        public override int GetHashCode()
        {
            // Just use offsets for unique ITextSource position hashing
            return RecursiveSerializerHashGenerator.CreateSimpleHash(this.Offset);
        }
        public override string ToString()
        {
            return this.FormatToString();
        }
    }
}
