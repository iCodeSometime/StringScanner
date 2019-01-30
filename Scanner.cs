using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;

namespace SqlCompiler.StringScanner
{
    public class Scanner : System.IO.StringReader
    {
        private StringBuilder buffer = new StringBuilder();

        public Scanner(string content) : base(content) { }

        // This method is stateful, resetting every other time it is called.
        // The first time, it reads until a delimiter is matched.
        // The second time, it reads until the end of the delimiter, and 
        // resets it's state (the this.buffer).
        public string ReadNextWordOrDelim(IEnumerable<Regex> delimiters)
        {
            string ret;

            do
            {
                // If we're in the middle of matching a closing delimiter
                if (delimiters.Any(r => r.IsMatch(this.buffer.ToString())))
                {
                    ret = ReadNextDelim(delimiters);
                    this.buffer.Clear();
                }
                else
                {
                    ret = ReadNextWord(delimiters);
                }
            } while (String.IsNullOrEmpty(ret) && Peek() != -1);
            return ret;
        }

        /// <summary>
        /// Reads the next word.
        /// </summary>
        /// <returns>The next word.</returns>
        private string ReadNextWord(IEnumerable<Regex> delimiters)
        {
            int next;
            do
            {
                next = Read();
                if (next == -1)
                {
                    return this.buffer.ToString();
                }
                this.buffer.Append((char)next);
            } while (!GetDelimLocation(this.buffer.ToString(), delimiters).HasValue);

            var (start, length) = GetDelimLocation(this.buffer.ToString(), delimiters).Value;
            return this.buffer.ToString().Substring(0, start);
        }

        /// <summary>
        /// Reads the next delimiter. Assumes the buffer has previous content.
        /// </summary>
        /// <returns>The next delimiter.</returns>
        private string ReadNextDelim(IEnumerable<Regex> delimiters)
        {
            while (KeepReadingDelim(delimiters))
            {
                this.buffer.Append((char)Read());
            }
            return JustTheDelim(this.buffer.ToString(), delimiters);
        }

        /// <summary>
        /// Should we keep adding to the delimiter?
        /// </summary>
        /// <remarks>
        /// Assumes that GetDelimLocation will return non-null data at the current state
        /// </remarks>
        /// <returns><c>true</c>, if the next character can be included in the delimiter, <c>false</c> otherwise.</returns>
        private bool KeepReadingDelim(IEnumerable<Regex> delimiters)
        {
            int next = Peek();
            if (next == -1)
            {
                return false;
            }
            var (_, oldLength) = GetDelimLocation(this.buffer.ToString(), delimiters).Value;
            var (_, newLength) = GetDelimLocation(this.buffer.ToString() + (char)next, delimiters).Value;

            return newLength > oldLength;
        }

        /// <summary>
        /// The start and end of the match for the longest _delimiter
        /// </summary>
        /// <returns>The beginning and end of the longest delimiter</returns>
        /// <param name="content">The string to check for a delimiter.</param>
        private (int start, int length)? GetDelimLocation(string content, IEnumerable<Regex> delimiters)
        {
            Match maxMatch = delimiters
                .SelectMany(r => (IEnumerable<Match>) r.Matches(content))
                .OrderByDescending(m => m.Length)
                .FirstOrDefault();
           
            if (maxMatch == null)
            {
                return null;
            }
            return (maxMatch.Index, maxMatch.Length);
        }

        /// <summary>
        /// Longest match for any _delimiter within content. "" if none.
        /// </summary>
        /// <returns>The text of the longest delimiter</returns>
        /// <param name="content">The string to pull the delimiter from.</param>
        private string JustTheDelim(string content, IEnumerable<Regex> delimiters)
        {
            string ret = "";
            var startEnd = GetDelimLocation(content, delimiters);
            if (startEnd.HasValue)
            {
                var (start, length) = startEnd.Value;
                ret = content.Substring(start, length);
            }
            return ret;
        }
    }
}
