using SimpleTextEditor.Model;

namespace SimpleTextEditor.Extension
{
    public static class StringExtension
    {
        public static IEnumerable<StringLocator> GetWhiteSpaces(this string text, bool includeSingleWhiteSpace = false)
        {
            var result = new List<StringLocator>();
            var firstIndex = -1;
            var firstSpace = false;
            var nextSpace = false;

            for (int index = 0; index < text.Length; index++)
            {
                if (text[index] == ' ')
                {
                    if (!firstSpace)
                    {
                        firstSpace = true;
                        firstIndex = index;
                    }
                    else
                        nextSpace = true;
                }
                else
                {
                    // (More) White Space
                    if (nextSpace)
                    {
                        result.Add(new StringLocator(text, firstIndex, index - firstIndex + 1));
                    }

                    // Single White Space
                    else if (firstSpace && includeSingleWhiteSpace)
                    {
                        result.Add(new StringLocator(text, firstIndex, 1));
                    }
                }
            }

            return result;
        }
    }
}
