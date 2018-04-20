using System;
using System.Diagnostics.CodeAnalysis;

namespace ModernUI.Windows.Controls.BBCode
{
    /// <summary>
    ///     Represents a character buffer.
    /// </summary>
    internal class CharBuffer
    {
        int mark;
        int position;
        readonly string value;

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:CharBuffer" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public CharBuffer(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.value = value;
        }

        /// <summary>
        ///     Performs a look-ahead.
        /// </summary>
        /// <param name="count">The number of character to look ahead.</param>
        /// <returns></returns>
        public char LA(int count)
        {
            int index = position + count - 1;
            if (index < value.Length)
            {
                return value[index];
            }

            return char.MaxValue;
        }

        /// <summary>
        ///     Marks the current position.
        /// </summary>
        public void Mark()
        {
            mark = position;
        }

        /// <summary>
        ///     Gets the mark.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public string GetMark()
        {
            if (mark < position)
            {
                return value.Substring(mark, position - mark);
            }
            return string.Empty;
        }

        /// <summary>
        ///     Consumes the next character.
        /// </summary>
        public void Consume()
        {
            position++;
        }
    }
}