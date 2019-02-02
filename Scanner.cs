using System;
using System.IO;
using System.Text;

namespace SqlCompiler.StringScanner
{
    public class Scanner : System.IO.StreamReader
    {
        public Scanner(string content) :
        base(new MemoryStream(Encoding.UTF8.GetBytes(content))) { }

        // This method is stateful, resetting every other time it is called.
        // The first time, it reads until a delimiter is matched.
        // The second time, it reads until the end of the delimiter, and 
        // resets it's state (the this.buffer).
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
            } while ((readTo = substringTo(buffer.ToString())) == -1);

            Seek(oldPosition + readTo);
            return buffer.ToString(0, readTo);
        }

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

        public string Peek(IDelimiter delimiter)
        {
            long oldPosition = BaseStream.Position;
            string ret = Read(delimiter);
            Seek(oldPosition);
            return ret;
        }

        private void Seek(long position)
        {
            BaseStream.Seek(position, SeekOrigin.Begin);
            DiscardBufferedData();
        }
    }    
}
