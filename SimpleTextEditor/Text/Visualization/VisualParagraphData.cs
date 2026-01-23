using SimpleTextEditor.Text.Visualization.Element.Interface;

using SimpleWpf.Extensions;

namespace SimpleTextEditor.Text.Visualization
{
    public class VisualParagraphData
    {
        public int ParagraphNumber { get; }
        public IEnumerable<VisualLineData> Lines
        {
            get
            {
                if (_closingSpan == null)
                    throw new Exception("Must add closing span before using VisualParagraphData!");

                return _lines;
            }
        }

        List<VisualLineData> _lines;
        ITextSpan _closingSpan;

        public VisualParagraphData(int paragraphNumber)
        {
            this.ParagraphNumber = paragraphNumber;
            _lines = new List<VisualLineData>();
        }

        public void AddLine(VisualLineData lineData)
        {
            _lines.Add(lineData);
        }

        public VisualLineData GetLine(int visualLineNumber)
        {
            return _lines[visualLineNumber - 1];
        }

        /// <summary>
        /// Adds final end span to paragraph. This is required for proper function!
        /// </summary>
        public void SetClosingSpan(ITextSpan textSpan)
        {
            if (textSpan is ITextElement)
                throw new ArgumentException("Trying to add ITextSpan as ITextElement! Please use AddElement for this operation!");

            _closingSpan = textSpan;

            // Visual Element not used for spans
        }

        public void ClearElements()
        {
            _lines.Clear();
            _closingSpan = null;
        }

        public override string ToString()
        {
            return this.FormatToString();
        }
    }
}
