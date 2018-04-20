namespace ModernUI.Windows.Controls.BBCode
{
    /// <summary>
    ///     The BBCode lexer.
    /// </summary>
    internal class BBCodeLexer
        : Lexer
    {
        /// <summary>
        ///     Start tag
        /// </summary>
        public const int TokenStartTag = 0;

        /// <summary>
        ///     End tag
        /// </summary>
        public const int TokenEndTag = 1;

        /// <summary>
        ///     Attribute
        /// </summary>
        public const int TokenAttribute = 2;

        /// <summary>
        ///     Text
        /// </summary>
        public const int TokenText = 3;

        /// <summary>
        ///     Line break
        /// </summary>
        public const int TokenLineBreak = 4;

        /// <summary>
        ///     Normal state
        /// </summary>
        public const int StateNormal = 0;

        /// <summary>
        ///     Tag state
        /// </summary>
        public const int StateTag = 1;

        static readonly char[] QuoteChars = { '\'', '"' };
        static readonly char[] WhitespaceChars = { ' ', '\t' };
        static readonly char[] NewlineChars = { '\r', '\n' };

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:BBCodeLexer" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public BBCodeLexer(string value)
            : base(value)
        {
        }

        /// <summary>
        ///     Gets the default state of the lexer.
        /// </summary>
        /// <value>The state of the default.</value>
        protected override int DefaultState => StateNormal;

        bool IsTagNameChar()
        {
            return IsInRange('A', 'Z') || IsInRange('a', 'z') || IsInRange(new[] { '*' });
        }

        Token OpenTag()
        {
            Match('[');
            Mark();
            while (IsTagNameChar())
                Consume();

            return new Token(GetMark(), TokenStartTag);
        }

        Token CloseTag()
        {
            Match('[');
            Match('/');

            Mark();
            while (IsTagNameChar())
                Consume();
            Token token = new Token(GetMark(), TokenEndTag);
            Match(']');

            return token;
        }

        Token Newline()
        {
            Match('\r', 0, 1);
            Match('\n');

            return new Token(string.Empty, TokenLineBreak);
        }

        Token Text()
        {
            Mark();
            while (LA(1) != '[' && LA(1) != char.MaxValue && !IsInRange(NewlineChars))
                Consume();
            return new Token(GetMark(), TokenText);
        }

        Token EscapedTag()
        {
            Mark();
            Consume();
            while (LA(1) != '[' && LA(1) != char.MaxValue && !IsInRange(NewlineChars))
                Consume();
            string result = GetMark();
            return new Token(result.Substring(0, 1) + result.Substring(2), TokenText);
        }

        Token Attribute()
        {
            Match('=');
            while (IsInRange(WhitespaceChars))
                Consume();

            Token token;

            if (IsInRange(QuoteChars))
            {
                Consume();
                Mark();
                while (!IsInRange(QuoteChars))
                    Consume();
                token = new Token(GetMark(), TokenAttribute);
                Consume();
            }
            else
            {
                Mark();
                while (!IsInRange(WhitespaceChars) && LA(1) != ']' && LA(1) != char.MaxValue)
                    Consume();

                token = new Token(GetMark(), TokenAttribute);
            }

            while (IsInRange(WhitespaceChars))
                Consume();
            return token;
        }

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
                             /// <summary>
                             ///     Gets the next token.
                             /// </summary>
                             /// <returns></returns>
        public override Token NextToken()
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
        {
            if (LA(1) == char.MaxValue)
            {
                return Token.End;
            }

            if (State == StateNormal)
            {
                if (LA(1) == '[')
                {
                    if (LA(2) == '\\')
                    {
                        return EscapedTag();
                    }
                    if (LA(2) == '/')
                    {
                        return CloseTag();
                    }
                    Token token = OpenTag();
                    PushState(StateTag);
                    return token;
                }
                if (IsInRange(NewlineChars))
                {
                    return Newline();
                }
                return Text();
            }
            if (State == StateTag)
            {
                if (LA(1) == ']')
                {
                    Consume();
                    PopState();
                    return NextToken();
                }

                return Attribute();
            }
            throw new ParseException("Invalid state");
        }
    }
}