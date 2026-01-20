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
        }

        #region (public) IDocument Methods
        public void Initialize(Size constraintSize)
        {
            _visualCore.Initialize(constraintSize);
        }
        public void Load(string text)
        {
            _visualCore.AppendText(text);
        }
        public Size Measure(Size constraint)
        {
            // Update size on the backend
            _visualCore.UpdateSize(constraint);

            // This will force a measure pass if it hasn't run already
            return _visualCore.GetOutput().DesiredSize;
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
        public bool ProcessMouseInput(MouseData mouseData)
        {
            return _visualCore.SetMouseInfo(mouseData);
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
            // Will force a measurement pass if it hasn't run already.
            return _visualCore.GetOutput().VisualElements;
        }
        public void Clear()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region (private) UI <--> ITextVisualCore Methods
        private int GetTextOffsetFromUI(Point pointUI)
        {
            //if (_visualOutputData != null)
            //{
            //    foreach (var element in _visualOutputData.VisualElements)
            //    {
            //        // Get Text Bounds: For this element (NOT SURE ABOUT POSITION!)
            //        //var textBounds = element.Element.GetTextBounds(element.Position.SourceOffset, element.Length);

            //        var textBounds = element.Position.VisualBounds;

            //        //var index = textBounds.SelectMany(x => x.TextRunBounds)
            //        //                      .FirstOrDefault(x => x.Rectangle.Contains(pointUI))
            //        //                      ?.TextSourceCharacterIndex ?? -1;

            //        if (textBounds.Contains(pointUI))
            //            return element.Position.SourceOffset;
            //    }
            //}

            return -1;
        }
        #endregion
    }
}
