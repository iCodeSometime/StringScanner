/* Copyright (C) 2019 Kenneth Cochran
 *
 * This file is part of StringScanner.
 *
 * StringScanner is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * StringScanner is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with StringScanner. If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace SqlCompiler.StringScanner
{
    public interface IDelimiter
    {
        Match Match(string toMatch);
        bool IsMatch(string toMatch);
    }

    /// <summary>
    ///     A Colection of delimiter objects. It also implements IDelimiter.
    ///     A match is defined as the longest match (if any) of any element.
    /// </summary>
    public abstract class DelimiterCollection<T> : ICollection<T>, IDelimiter
        where T : IDelimiter
    {
        public ICollection<T> Delimiters { get; set; }

        public DelimiterCollection() : this(new List<T>()) { }
        public DelimiterCollection(ICollection<T> delimiters)
        {
            Delimiters = delimiters;
        }

        /// <summary>
        ///     Does any element match the given string?
        /// </summary>
        /// <returns><c>true</c>, if there is any match, <c>false</c> otherwise.</returns>
        /// <param name="content">The string to match.</param>
        public bool IsMatch(string content)
        {
            return Delimiters.Any(d => d.IsMatch(content));
        }
        /// <summary>
        ///     Returns the longest match of a given string.
        /// </summary>
        /// <returns>The longest match.</returns>
        /// <param name="content">The string to match.</param>
        public Match Match(string content)
        {
            return Delimiters.Select(d => d.Match(content))
                             .OrderByDescending(m => m.Length)
                             .FirstOrDefault();
        }

        #region proxied interface methods
        public IEnumerator<T> GetEnumerator()
        {
            return Delimiters.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int Count => Delimiters.Count();

        public abstract bool IsReadOnly { get; }

        public void Add(T item)
        {
            Delimiters.Add(item);
        }

        public void Clear()
        {
            Delimiters.Clear();
        }

        public bool Contains(T item)
        {
            return Delimiters.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Delimiters.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return Delimiters.Remove(item);
        }
        #endregion
    }

}
