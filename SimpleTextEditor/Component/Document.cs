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

        public SimpleTextVisualOutputData? LastVisualData { get { return _visualOutputData; } }

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

        public void AppendText(string text)
        {
            _visualCore.AppendText(text);
        }

        public void InsertText(int offset, string text)
        {
            _visualCore.InsertText(offset, text);
        }

        public void Load(string text)
        {
            _visualCore.AppendText(text);
        }

        public SimpleTextVisualOutputData Measure(Size constraint)
        {
            // Trying to reduce measurement cost
            if (_visualOutputData != null &&
                _visualOutputData.DesiredSize.Width <= constraint.Width &&
                _visualOutputData.DesiredSize.Height <= constraint.Height &&
                _visualOutputData.SourceLength == _visualCore.GetTextLength())
                return _visualOutputData;

            _visualOutputData = _visualCore.Measure(constraint);

            return _visualOutputData;
        }

        public void RemoveText(int offset, int count)
        {
            _visualCore.RemoveText(offset, count);
        }

        public ITextPosition GetCaretPosition()
        {
            throw new NotImplementedException();
        }

        public void SetCaretPosition(ITextPosition position)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public int GetLength()
        {
            throw new NotImplementedException();
        }

        public string GetTextCopy()
        {
            return _visualCore.GetTextCopy();
        }

        public int GetTextLength()
        {
            return _visualCore.GetTextLength();
        }

        public int SearchText(char character, int startIndex)
        {
            return _visualCore.SearchText(character, startIndex);
        }

        public void SetMouseInfo(MouseData mouseData)
        {
            throw new NotImplementedException();
        }

        public int GetTextOffsetFromUI(Point pointUI)
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
    }
}
