using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ModernUI.Windows.Controls.BBCode
{
    /// <summary>
    ///     Provides basic lexer functionality.
    /// </summary>
    internal abstract class Lexer
    {
        /// <summary>
        ///     Defines the end-of-file token type.
        /// </summary>
        public const int TokenEnd = int.MaxValue;

        readonly CharBuffer buffer;
        readonly Stack<int> states;

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Lexer" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        protected Lexer(string value)
        {
            buffer = new CharBuffer(value);
            states = new Stack<int>();
        }

        /// <summary>
        ///     Gets the default state of the lexer.
        /// </summary>
        /// <value>The state of the default.</value>
        protected abstract int DefaultState { get; }

        /// <summary>
        ///     Gets the current state of the lexer.
        /// </summary>
        /// <value>The state.</value>
        protected int State
        {
            get
            {
                if (states.Count > 0)
                {
                    return states.Peek();
                }
                return DefaultState;
            }
        }

        static void ValidateOccurence(int count, int minOccurs, int maxOccurs)
        {
            if (count < minOccurs || count > maxOccurs)
            {
                throw new ParseException("Invalid number of characters");
            }
        }

        /// <summary>
        ///     Pushes a new state on the stac.
        /// </summary>
        /// <param name="state">The state.</param>
        protected void PushState(int state)
        {
            states.Push(state);
        }

        /// <summary>
        ///     Pops the state.
        /// </summary>
        /// <returns></returns>
        protected int PopState()
        {
            return states.Pop();
        }

        /// <summary>
        ///     Performs a look-ahead.
        /// </summary>
        /// <param name="count">The number of characters to look ahead.</param>
        /// <returns></returns>
        protected char LA(int count)
        {
            return buffer.LA(count);
        }

        /// <summary>
        ///     Marks the current position.
        /// </summary>
        protected void Mark()
        {
            buffer.Mark();
        }

        /// <summary>
        ///     Gets the mark.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected string GetMark()
        {
            return buffer.GetMark();
        }

        /// <summary>
        ///     Consumes the next character.
        /// </summary>
        protected void Consume()
        {
            buffer.Consume();
        }

        /// <summary>
        ///     Determines whether the current character is in given range.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="last">The last.</param>
        /// <returns>
        ///     <c>true</c> if the current character is in given range; otherwise, <c>false</c>.
        /// </returns>
        protected bool IsInRange(char first, char last)
        {
            char la = LA(1);
            return la >= first && la <= last;
        }

        /// <summary>
        ///     Determines whether the current character is in given range.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///     <c>true</c> if the current character is in given range; otherwise, <c>false</c>.
        /// </returns>
        protected bool IsInRange(char[] value)
        {
            if (value == null)
            {
                return false;
            }
            char la = LA(1);
            for (int i = 0; i < value.Length; i++)
            {
                if (la == value[i])
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Matches the specified character.
        /// </summary>
        /// <param name="value">The value.</param>
        protected void Match(char value)
        {
            if (LA(1) == value)
            {
                Consume();
            }
            else
            {
                throw new ParseException("Character mismatch");
            }
        }

        /// <summary>
        ///     Matches the specified character.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="minOccurs">The min occurs.</param>
        /// <param name="maxOccurs">The max occurs.</param>
        protected void Match(char value, int minOccurs, int maxOccurs)
        {
            int i = 0;
            while (LA(1) == value)
            {
                Consume();
                i++;
            }
            ValidateOccurence(i, minOccurs, maxOccurs);
        }

        /// <summary>
        ///     Matches the specified string.
        /// </summary>
        /// <param name="value">The value.</param>
        protected void Match(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            for (int i = 0; i < value.Length; i++)
            {
                if (LA(1) == value[i])
                {
                    Consume();
                }
                else
                {
                    throw new ParseException("String mismatch");
                }
            }
        }

        /// <summary>
        ///     Matches the range.
        /// </summary>
        /// <param name="value">The value.</param>
        protected void MatchRange(char[] value)
        {
            if (IsInRange(value))
            {
                Consume();
            }
            else
            {
                throw new ParseException("Character mismatch");
            }
        }

        /// <summary>
        ///     Matches the range.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="minOccurs">The min occurs.</param>
        /// <param name="maxOccurs">The max occurs.</param>
        protected void MatchRange(char[] value, int minOccurs, int maxOccurs)
        {
            int i = 0;
            while (IsInRange(value))
            {
                Consume();
                i++;
            }
            ValidateOccurence(i, minOccurs, maxOccurs);
        }

        /// <summary>
        ///     Matches the range.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="last">The last.</param>
        protected void MatchRange(char first, char last)
        {
            if (IsInRange(first, last))
            {
                Consume();
            }
            else
            {
                throw new ParseException("Character mismatch");
            }
        }

        /// <summary>
        ///     Matches the range.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="last">The last.</param>
        /// <param name="minOccurs">The min occurs.</param>
        /// <param name="maxOccurs">The max occurs.</param>
        protected void MatchRange(char first, char last, int minOccurs, int maxOccurs)
        {
            int i = 0;
            while (IsInRange(first, last))
            {
                Consume();
                i++;
            }
            ValidateOccurence(i, minOccurs, maxOccurs);
        }

        /// <summary>
        ///     Gets the next token.
        /// </summary>
        /// <returns></returns>
        public abstract Token NextToken();
    }
}