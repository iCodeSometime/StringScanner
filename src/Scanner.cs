using System;
using System.IO;
using System.Text;

namespace SqlCompiler.StringScanner
{
    /// <summary>
    ///     Scans a string for words based on given delimiters.
    /// </summary>
    public class Scanner : System.IO.StreamReader
    {
        public Scanner(string content) :
        base(new MemoryStream(Encoding.UTF8.GetBytes(content))) { }

        /// <summary>
        ///     Reads characters, passing the accumulated word to the provided 
        ///     function until it returns a positive value.Then returns the 
        ///     substring up to that value and seeks the base stream to that 
        ///     location.
        /// </summary>
        /// <param name="substringTo">
        ///     The function to call with the accumulated word. Should return a
        ///     negative number to continue being called with the next character.
        ///     Should return a non-negative number to stop reading. The string
        ///     up to the given index will be the return value.
        /// </param>
        /// <returns>
        ///     The substring up to the index given by <paramref name="substringTo"/>
        /// </returns>
        private string Read(Func<string, int> substringTo)
        {
            StringBuilder buffer = new StringBuilder();
            long oldPosition = BaseStream.Position;
            int readTo;

            do
            {
                int character = Read();
                if (character == -1)
                {
                    return buffer.Length > 0 ? buffer.ToString() : null;
                }
                buffer.Append((char)character);
            } while ((readTo = substringTo(buffer.ToString())) < 0);

            Seek(oldPosition + readTo);
            return buffer.ToString(0, readTo);
        }

        /// <summary>
        ///     Reads the next word using the given IDelimiters. If a delimiter
        ///     can be matched to the beginning of the string, it will be 
        ///     preferred. Otherwise, the text up to the first delimiter will be
        ///     returned.
        /// </summary>
        /// <returns>The next word from the stream.</returns>
        /// <param name="delimiter">
        ///     The delimiter that defines word boundaries. In practice, DelimiterCollection is 
        ///     likely the best type to use here.
        /// </param>
        public string Read(IDelimiter delimiter)
        {
            // Read until we have a match.
            string word = Read(s =>
            {
                var m = delimiter.Match(s);
                if (!m.Success) return -1;
                return m.Index;
            });

            // Read until we find a delimiter match.
            if (String.IsNullOrEmpty(word))
            {
                word = Read(s =>
                {
                    int matchLength = delimiter.Match(s).Length;
                    return matchLength == 0 || matchLength == s.Length ? -1 : matchLength;
                });
            }
            return word;
        }

        /// <summary>
        ///     Reads the next word without advancing the stream
        /// </summary>
        /// <returns>The next word from the stream.</returns>
        /// <param name="delimiter">
        ///     The delimiter that defines word boundaries. 
        ///     <see cref="Read(IDelimiter)"/>
        /// </param>
        public string Peek(IDelimiter delimiter)
        {
            long oldPosition = BaseStream.Position;
            string ret = Read(delimiter);
            Seek(oldPosition);
            return ret;
        }

        // Seeks the stream to the given position.
        private void Seek(long position)
        {
            BaseStream.Seek(position, SeekOrigin.Begin);
            DiscardBufferedData();
        }
    }    
}
