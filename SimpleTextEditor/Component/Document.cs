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

        public Document(FontFamily fontFamily, double fontSize, Brush foreground, Brush background, TextWrapping textWrapping)
        {
            _visualCore = new SimpleTextVisualCore(fontFamily, fontSize, foreground, background, textWrapping);
            _visualOutputData = null;
        }

        public void AppendText(string text)
        {
            _visualCore.GetSource().AppendText(text);
        }

        public ITextSource GetSource()
        {
            return _visualCore.GetSource();
        }

        public void InsertText(int offset, string text)
        {
            _visualCore.GetSource().InsertText(offset, text);
        }

        public void Load(string text)
        {
            _visualCore.GetSource().AppendText(text);
        }

        public SimpleTextVisualOutputData Measure(Size constraint)
        {
            _visualOutputData = _visualCore.Measure(constraint);

            return _visualOutputData;
        }

        public void RemoveText(int offset, int count)
        {
            _visualCore.GetSource().RemoveText(offset, count);
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

        public TextString Get()
        {
            return _visualCore.GetSource().Get();
        }
    }
}
