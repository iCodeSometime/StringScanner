using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;

namespace SqlCompiler.StringScanner
{
    public class Scanner : System.IO.StringReader
    {
        private readonly List<Regex> _delimiters = new List<Regex>();
        private StringBuilder _buffer = new StringBuilder();

        public Scanner(string content) : base(content) { }

        // This method is stateful, resetting every other time it is called.
        // The first time, it reads until a delimiter is matched.
        // The second time, it reads until the end of the delimiter, and 
        // resets it's state (the _buffer).
        public string ReadNextWordOrDelim()
        {
            string ret;
            // If we're in the middle of matching a closing delimiter
            if (_delimiters.Any(r => r.IsMatch(_buffer.ToString())))
            {
                ret =  ReadNextDelim();
            }
            else
            {
                ret = ReadNextWord();
            }
            return ret;
        }
        /// <summary>
        /// Reads the next word.
        /// </summary>
        /// <returns>The next word.</returns>
        private string ReadNextWord()
        {
            _buffer.Clear();
            int next;
            do
            {
                next = Read();
                if (next == -1)
                {
                    return _buffer.ToString();
                }
                _buffer.Append(next);
            } while (GetDelimStartEnd(_buffer.ToString()).HasValue);

            var (start, end) = GetDelimStartEnd(_buffer.ToString()).Value;
            return _buffer.ToString().Substring(0, start);
        }

        /// <summary>
        /// Reads the next delimiter. Assumes the buffer has previous content.
        /// </summary>
        /// <returns>The next delimiter.</returns>
        private string ReadNextDelim()
        {
            while (GetDelimStartEnd(_buffer.ToString() + Peek())?.end == _buffer.Length + 1)
            {
                _buffer.Append(Read());
            }
            return JustTheDelim(_buffer.ToString());
        }

        /// <summary>
        /// The start and end of the match for the longest _delimiter
        /// </summary>
        /// <returns>The beginning and end of the longest delimiter</returns>
        /// <param name="content">The string to check for a delimiter.</param>
        private (int start, int end)? GetDelimStartEnd(string content)
        {
            Match maxMatch = _delimiters
                .SelectMany(r => (IEnumerable<Match>) r.Matches(content))
                .OrderByDescending(m => m.Length)
                .FirstOrDefault();
           
            return (maxMatch.Index, maxMatch.Index + maxMatch.Length);
        }

        /// <summary>
        /// Longest match for any _delimiter within content. "" if none.
        /// </summary>
        /// <returns>The text of the longest delimiter</returns>
        /// <param name="content">The string to pull the delimiter from.</param>
        private string JustTheDelim(string content)
        {
            string ret = "";
            var startEnd = GetDelimStartEnd(content);
            if (startEnd.HasValue)
            {
                var (start, end) = startEnd.Value;
                ret = content.Substring(start, end);
            }
            return ret;
        }

        public void AddDelimiter(Regex delim) { _delimiters.Add(delim); }
        public void ClearDelimiters() { _delimiters.Clear(); }
        public void RemoveDelimiter(Regex delim) { _delimiters.Remove(delim); }
    }
}
