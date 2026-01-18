using SimpleTextEditor.Extension;

using SimpleWpf.Extensions.Collection;

namespace SimpleTextEditor.Model
{
    /// <summary>
    /// Primary encapsulating model for the string primitives. This should help guard
    /// memory from string copies; and provide some convenience methods.
    /// </summary>
    public class TextEditorString
    {
        char[] _content;

        public int Length { get { return _content.Length; } }

        public TextEditorString()
        {
            _content = new char[0];
        }
        public TextEditorString(char[] content)
        {
            _content = new char[content.Length];

            Concat(content);
        }

        public void Concat(char[] characters)
        {
            var result = new char[_content.Length + characters.Length];

            Array.Copy(_content, 0, result, 0, _content.Length);
            Array.Copy(characters, 0, result, _content.Length, characters.Length);

            _content = result;
        }

        public void Insert(char[] characters, int insertIndex)
        {
            if (insertIndex < 0 || insertIndex >= _content.Length)
                throw new ArgumentOutOfRangeException();

            var result = new char[_content.Length + characters.Length];

            // Copy array in 3 pieces (THIS IS STILL OFF BY ONE!)
            Array.Copy(_content, 0, result, 0, insertIndex);
            Array.Copy(characters, 0, result, insertIndex, characters.Length);
            Array.Copy(_content, insertIndex, result, insertIndex + characters.Length, result.Length - characters.Length - insertIndex);

            _content = result;
        }

        public void Remove(int offset, int count)
        {
            if (offset < 0 || offset >= _content.Length)
                throw new ArgumentOutOfRangeException();

            if (_content.Length - count < 0)
                throw new ArgumentOutOfRangeException("Trying to remove more than is contained in the TextString");

            if (offset + count > _content.Length)
                throw new ArgumentOutOfRangeException("Trying to remove off the end of a TextString");

            var result = new char[_content.Length - count];

            Array.Copy(_content, 0, result, 0, offset);
            Array.Copy(_content, offset + count, result, offset, _content.Length - offset - count);

            _content = result;
        }

        public IEnumerable<StringLocator> GetWhiteSpaces(bool includeSingleWhiteSpace)
        {
            return GetString().GetWhiteSpaces(includeSingleWhiteSpace);
        }

        public IList<char[]> Split(int index, bool keepSplitCharacter)
        {
            if (index < 0 || index >= _content.Length)
                throw new ArgumentOutOfRangeException();

            return Split(keepSplitCharacter, new int[] { index });
        }

        public IList<char[]> Split(char character, bool keepSplitCharacter)
        {
            var splitIndices = new List<int>();

            for (int index = 0; index < _content.Length; index++)
            {
                // Next Character
                if (_content[index] == character)
                    splitIndices.Add(index);
            }

            return Split(keepSplitCharacter, splitIndices.ToArray());
        }

        public IList<char[]> Split(bool keepSplitCharacter, params int[] splitIndices)
        {
            // Creating copy intentionally for the user code
            if (splitIndices.Length == 0)
                return new List<char[]>() { _content.Copy().ToArray() };

            var result = new List<char[]>();
            var index = 0;

            char[] nextArray;

            while (index < splitIndices.Length)
            {
                if (splitIndices[index] < 1 ||
                    splitIndices[index] >= _content.Length)
                    throw new IndexOutOfRangeException("Must split text editor string after the first index, and before or equal to the last");

                // Calculate Next Sub-Array
                nextArray = GetSubArray(index == 0 ? 0 : splitIndices[index - 1], splitIndices[index], keepSplitCharacter);

                result.Add(nextArray);

                index++;
            }

            // Final Split (Copy) (careful with split character, here)
            nextArray = GetSubArray(keepSplitCharacter ? splitIndices[index - 1] + 1 : splitIndices[index - 1], _content.Length - 1, true);

            result.Add(nextArray);

            return result.ToArray();
        }

        private char[] GetSubArray(int startIndex, int splitIndex, bool keepSplitCharacter)
        {
            // Create Next Sub-Array
            var nextArray = new char[keepSplitCharacter ? splitIndex - startIndex + 1 : splitIndex - startIndex];

            // Copy Sub-Array
            Array.Copy(_content, startIndex, nextArray, 0, nextArray.Length);

            return nextArray;
        }

        public string GetString()
        {
            return new string(_content);
        }

        public string GetSubString(int index, int count)
        {
            var result = new char[count];

            Array.Copy(_content, result, count);

            return new string(result);
        }

        public char[] Get()
        {
            return _content;
        }

        public static TextEditorString From(params char[][] otherArrays)
        {
            var length = otherArrays.Sum(x => x.Length);
            var result = new char[length];
            var insertIndex = 0;

            for (int index = 0; index < otherArrays.Length; index++)
            {
                // Copy
                Array.Copy(otherArrays[index], 0, result, insertIndex, otherArrays[index].Length);

                // Next Insertion
                insertIndex += otherArrays[index].Length;
            }

            return new TextEditorString(result);
        }

        public static TextEditorString From(params TextEditorString[] otherInstances)
        {
            var result = new TextEditorString();

            foreach (var instance in otherInstances)
            {
                result.Concat(instance.Get());
            }

            return result;
        }


        public override string ToString()
        {
            return GetString();
        }
    }
}
