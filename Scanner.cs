using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;

namespace SqlCompiler.StringScanner
{
    public class Scanner : System.IO.StreamReader
    {
        public Scanner(string content) :
        base(new MemoryStream(Encoding.UTF8.GetBytes(content)))
        { }

        // This method is stateful, resetting every other time it is called.
        // The first time, it reads until a delimiter is matched.
        // The second time, it reads until the end of the delimiter, and 
        // resets it's state (the this.buffer).
        public string ReadNextWordOrDelim(DelimiterCollection delimiters)
        {
            StringBuilder buffer = new StringBuilder();
            long oldPosition = BaseStream.Position;
            string ret;

            // Read until we find a delimiter match.
            do
            {
                buffer.Append(Read());
            } while (delimiters.IsMatch(buffer.ToString()));

            Match match = delimiters.LongestMatch(buffer.ToString());

            // We are at EOL.
            if (match == null) return buffer.ToString();


            Seek(oldPosition + match.Index);

            // If we're reading a delim;
            if (match.Index == 0)
            {
                ret = ReadDelim(delimiters);
            }
            else
            {
                ret = buffer.ToString(0, match.Index);
            }

            return ret;

        }

        private void Seek(long position)
        {
            BaseStream.Seek(position, SeekOrigin.Begin);
            DiscardBufferedData();
        }

        private string ReadDelim(DelimiterCollection delimiters)
        {
            StringBuilder buffer = new StringBuilder();
            int matchLength;
            do
            {
                buffer.Append
                matchLength = delimiters.LongestMatch(buffer.ToString()).Length;
            } while (matchLength == buffer.Length);
            return buffer.ToString(0, matchLength);
        }
    }

    abstract class DelimiterCollection : IEnumerable<Regex>
    {
        private List<Regex> delimiters = new List<Regex>();
        public List<Regex> Delimiters { get; set; }

        public bool IsMatch(string content)
        {
            return delimiters.Any(d => d.IsMatch(content));
        }
        public Match LongestMatch(string content)
        {
            return delimiters.Select(d => d.Match(content))
                             .FirstOrDefault(m => m.Success);
        }

        #region interface methods
        public IEnumerator<Regex> GetEnumerator()
        {
            return delimiters.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion
    }
    
}
