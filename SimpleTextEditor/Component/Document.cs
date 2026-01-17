using System.Windows;
using System.Windows.Media;

using SimpleTextEditor.Component.Interface;
using SimpleTextEditor.Model;
using SimpleTextEditor.Text;
using SimpleTextEditor.Text.Interface;
using SimpleTextEditor.Text.Visualization;

namespace SimpleTextEditor.Component
{
    public class Document : IDocument
    {
        // Last measurement data
        SimpleTextVisualOutputData? _visualOutputData;

        // Carries the primary text source
        readonly ITextVisualCore _visualCore;

        public int TextLength { get { return _visualCore.GetTextLength(); } }

        public Document(FontFamily fontFamily,
                        double fontSize,
                        Brush foreground,
                        Brush background,
                        Brush highlightForeground,
                        Brush highlightBackground,
                        TextWrapping textWrapping)
        {
            _visualCore = new SimpleTextVisualCore(fontFamily, fontSize, foreground, background, highlightForeground, highlightBackground, textWrapping);
            _visualOutputData = null;
        }

        #region (public) IDocument Methods
        public void Load(string text)
        {
            _visualCore.AppendText(text);
        }
        public Size Measure(Size constraint)
        {
            // Trying to reduce measurement cost
            if (_visualOutputData != null &&
                _visualOutputData.DesiredSize.Width <= constraint.Width &&
                _visualOutputData.DesiredSize.Height <= constraint.Height &&
                _visualOutputData.SourceLength == _visualCore.GetTextLength())
                return _visualOutputData.DesiredSize;

            _visualOutputData = _visualCore.Measure(constraint);

            return _visualOutputData.DesiredSize;
        }
        public Rect GetCaretBounds()
        {
            return _visualCore.GetCaretBounds();
        }
        public void ProcessUILeftClick(Point pointUI)
        {
            throw new NotImplementedException();
        }
        public void ProcessControlInput(ControlInput input)
        {
            switch (input)
            {
                case ControlInput.LineUp:
                    break;
                case ControlInput.LineDown:
                    break;
                case ControlInput.CharacterLeft:
                    break;
                case ControlInput.CharacterRight:
                    break;
                case ControlInput.WordLeft:
                    break;
                case ControlInput.WordRight:
                    break;
                case ControlInput.EndOfLine:
                    break;
                case ControlInput.BeginningOfLine:
                    break;
                case ControlInput.PageUp:
                    break;
                case ControlInput.PageDown:
                    break;
                default:
                    throw new Exception("Unhandled ControlInput Type");
            }
        }
        public void ProcessInputText(string inputText)
        {
            _visualCore.AppendText(inputText);
        }
        public void ProcessRemoveText(int offset, int count)
        {
            _visualCore.RemoveText(offset, count);
        }
        public IEnumerable<SimpleTextElement> GetVisualElements()
        {
            return _visualOutputData == null ? new List<SimpleTextElement>() : _visualOutputData.VisualElements;
        }
        public void Clear()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region (private) UI <--> ITextVisualCore Methods
        private int GetTextOffsetFromUI(Point pointUI)
        {
            if (_visualOutputData != null)
            {
                foreach (var element in _visualOutputData.VisualElements)
                {
                    // Get Text Bounds: For this element (NOT SURE ABOUT POSITION!)
                    var textBounds = element.Element.GetTextBounds(element.Position.SourceOffset, element.Length);

                    var index = textBounds.SelectMany(x => x.TextRunBounds)
                                          .FirstOrDefault(x => x.Rectangle.Contains(pointUI))
                                          ?.TextSourceCharacterIndex ?? -1;

                    if (index != -1)
                        return index;
                }
            }

            return -1;
        }
        #endregion
    }
}
