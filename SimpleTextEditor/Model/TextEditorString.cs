using System.Buffers;

using SimpleTextEditor.Extension;

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

        public IList<char[]> Split(int index)
        {
            if (index < 0 || index >= _content.Length)
                throw new ArgumentOutOfRangeException();

            var array1 = new char[index + 1];
            var array2 = new char[_content.Length - index - 1];

            Array.Copy(_content, 0, array1, 0, array1.Length);
            Array.Copy(_content, index + 1, array2, 0, array2.Length);

            return new char[][] { array1, array2 };
        }

        public IList<char[]> Split(char character)
        {
            var result = new List<char[]>();
            var nextString = string.Empty;

            for (int index = 0; index < _content.Length; index++)
            {
                // Next Character
                if (_content[index] == character && nextString.Length > 0)
                {
                    result.Add(nextString.ToArray());

                    // Reset
                    nextString = string.Empty;
                }
                else
                    nextString = string.Concat(nextString, _content[index]);
            }

            if (nextString.Length > 0)
                result.Add(nextString.ToArray());

            return result;
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
