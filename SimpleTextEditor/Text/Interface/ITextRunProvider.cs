using SimpleTextEditor.Model;

namespace SimpleTextEditor.Text.Interface
{
    /// <summary>
    /// Component applied to MSFT's TextStore to add a couple of methods we need to 
    /// make our components cooperate.
    /// </summary>
    public interface ITextRunProvider
    {
        /// <summary>
        /// MSFT KLUDGE:  This is a kludge method to put together the TextFormatter. The problem is that there's no way
        ///               to pass information from the TextFormatter during line formatting. (Text effects may be how they
        ///               were doing that, but my guess is that the backend has other methods of processing that aren't as
        ///               direct as what we're going to build)
        ///               
        ///               The text lines must be broken down by \r or \n characters, to get visual formatting, but must honor
        ///               the UI constraint size. There's several unknowns (yet):  EOL characters (in their backend); text
        ///               line break input to the formatter; their caret tracking; character "buffering" (?); and how to 
        ///               apply text animations (which is probably not going to happen for our design).
        ///               
        ///               To prepare text we need:  the current text source line offset; the current line's character offset
        ///               (into the text source); and the current line's text properties (which may be set individually by 
        ///               character if you must).
        /// </summary>
        /// <param name="currentLine">current text line's index</param>
        /// <param name="textLineOffset">current text line's index</param>
        /// <param name="textLineCharacterOffset">current text line's text source offset</param>
        /// <param name="textLineProperties">current text line's text properties (in order), which should be prepared by the ITextSource</param>
        void SetLineRunProperties(string currentLine, int textLineOffset, int textLineCharacterOffset, IDictionary<IndexRange, ITextProperties> textLineProperties);
    }
}
