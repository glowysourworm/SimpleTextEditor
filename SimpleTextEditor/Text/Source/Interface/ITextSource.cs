using SimpleTextEditor.Model;
using SimpleTextEditor.Text.Visualization;
using SimpleTextEditor.Text.Visualization.Interface;

namespace SimpleTextEditor.Text.Source.Interface
{
    public interface ITextSource
    {
        const char EndOfLineCharacter = '\r';

        TextEditorString Get();
        char GetChar(int index);
        int GetLength();
        void AppendText(string text);
        void InsertText(int offset, string text);
        void RemoveText(int offset, int count);
        int Search(char character, int startIndex);

        /// <summary>
        /// Returns text lines split by using the end of line character. The EOL character may be used by the TextFormatter.
        /// </summary>
        IList<char[]> GetTextLines(bool keepEOLCharacter);

        /// <summary>
        /// Returns an IDictionary of the text properties in order of their occurrence
        /// </summary>
        /// <param name="textLine">char[] of the text line</param>
        /// <param name="textLineOffset">index of the text line</param>
        /// <param name="textLineCharacterOffset">index of the first character of the line of text, in the text source</param>
        /// <returns>A prepared IDictionary of text properties, in order of their occurrence</returns>
        IDictionary<IndexRange, ITextProperties> GetTextLineProperties(char[] textLine, int textLineOffset, int textLineCharacterOffset);

        /// <summary>
        /// Returns slices of the text where there are alternate properties set
        /// </summary>
        IndexRange[] GetPropertySlices();

        /// <summary>
        /// Returns the properties for the specified offset, along with the length of that specific text run
        /// </summary>
        ITextProperties GetProperties(int offset, out int length);

        /// <summary>
        /// Returns next index range for alternate properties after or equal to the supplied offset
        /// </summary>
        IndexRange GetNextPropertyRange(int offset, bool includeCurrentOffset);

        /// <summary>
        /// Sets alternate text properties for a range of text
        /// </summary>
        void SetProperties(IndexRange range, TextPropertySet propertySet);

        /// <summary>
        /// Clears alternate text properties for the entire text
        /// </summary>
        void ClearProperties();

        /// <summary>
        /// Clears all text from the text source
        /// </summary>
        void ClearText();
    }
}
