using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ModernUI.Windows.Navigation;

namespace ModernUI.Windows.Controls.BBCode
{
    /// <summary>
    ///     Represents the BBCode parser.
    /// </summary>
    internal class BBCodeParser
        : Parser<Span>
    {
        // supporting a basic set of BBCode tags
        const string TagBold = "b";

        const string TagColor = "color";
        const string TagItalic = "i";
        const string TagSize = "size";
        const string TagUnderline = "u";
        const string TagUrl = "url";
        const string TagStrikethrough = "s";
        const string TagQuote = "quote";
        const string TagList = "list";
        const string TagOrderedList = "ol";
        const string TagListItem = "li";
        const string TagNewLine = "br";
        readonly Brush quoteBrush;

        readonly FrameworkElement source;

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:BBCodeParser" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="source">The framework source element this parser operates in.</param>
        /// <param name="quoteBrush">The Brush used for quoting</param>
        public BBCodeParser(string value, FrameworkElement source, Brush quoteBrush = null)
            : base(new BBCodeLexer(value))
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            this.source = source;
            this.quoteBrush = quoteBrush;
        }

        /// <summary>
        ///     Gets or sets the available navigable commands.
        /// </summary>
        public CommandDictionary Commands { get; set; }

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        void ParseTag(string tag, bool start, ParseContext context)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
        {
            if (tag == TagBold)
            {
                context.FontWeight = null;
                if (start)
                {
                    context.FontWeight = FontWeights.Bold;
                }
            }
            else if (tag == TagColor)
            {
                if (start)
                {
                    Token token = LA(1);
                    if (token.TokenType == BBCodeLexer.TokenAttribute)
                    {
                        Color color = (Color)ColorConverter.ConvertFromString(token.Value);
                        context.Foreground = new SolidColorBrush(color);

                        Consume();
                    }
                }
                else
                {
                    context.Foreground = null;
                }
            }
            else if (tag == TagItalic)
            {
                if (start)
                {
                    context.FontStyle = FontStyles.Italic;
                }
                else
                {
                    context.FontStyle = null;
                }
            }
            else if (tag == TagSize)
            {
                if (start)
                {
                    Token token = LA(1);
                    if (token.TokenType == BBCodeLexer.TokenAttribute)
                    {
                        context.FontSize = Convert.ToDouble(token.Value);

                        Consume();
                    }
                }
                else
                {
                    context.FontSize = null;
                }
            }
            else if (tag == TagUnderline)
            {
                context.TextDecorations = start ? TextDecorations.Underline : null;
            }
            else if (tag == TagStrikethrough)
            {
                context.TextDecorations = start ? TextDecorations.Strikethrough : null;
            }
            else if (tag == TagUrl)
            {
                if (start)
                {
                    Token token = LA(1);
                    if (token.TokenType == BBCodeLexer.TokenAttribute)
                    {
                        context.NavigateUri = token.Value;
                        Consume();
                    }
                }
                else
                {
                    context.NavigateUri = null;
                }
            }
            else if (tag == TagQuote)
            {
                if (start)
                {
                    context.Background = quoteBrush;
                }
                else
                {
                    context.Background = null;
                }
            }
            else if (tag == TagList)
            {
                context.IsList = start;
                context.IsFirstListItem = true;
            }
            else if (tag == TagOrderedList)
            {
                context.IsOrderedList = start;
                context.ListCounter = 0;
                context.IsFirstListItem = true;
            }
            else if (tag == TagListItem)
            {
                context.IsListItem = start;
            }
            else if (tag == TagNewLine && start)
            {
                context.Parent.Inlines.Add(Environment.NewLine);
            }
        }

#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        void Parse(Span span)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
        {
            ParseContext context = new ParseContext(span);

            while (true)
            {
                Token token = LA(1);
                Consume();

                if (token.TokenType == BBCodeLexer.TokenStartTag)
                {
                    Span parent = span;
                    if (token.Value == TagListItem)
                    {
                        if (context.IsFirstListItem)
                        {
                            context.IsFirstListItem = false;
                            parent.Inlines.Add(new LineBreak());
                        }

                        parent.Inlines.Add(
                            new Run(string.Format("\u0020\u0020{0}\u0020\u0020",
                                context.IsOrderedList ? string.Format("{0}.", ++context.ListCounter) : "\u2022")));
                    }

                    ParseTag(token.Value, true, context);
                }
                else if (token.TokenType == BBCodeLexer.TokenEndTag)
                {
                    Span parent = span;
                    if (context.IsListItem && token.Value == TagListItem)
                    {
                        parent.Inlines.Add(new LineBreak());
                    }
                    ParseTag(token.Value, false, context);
                }
                else if (token.TokenType == BBCodeLexer.TokenText)
                {
                    Span parent = span;
                    Uri uri;
                    string parameter = null;
                    string targetName = null;

                    // parse uri value for optional parameter and/or target, eg [url=cmd://foo|parameter|target]
                    if (NavigationHelper.TryParseUriWithParameters(context.NavigateUri, out uri, out parameter,
                        out targetName))
                    {
                        Hyperlink link = new Hyperlink();

                        // assign ICommand instance if available, otherwise set NavigateUri
                        ICommand command;
                        if (Commands != null && Commands.TryGetValue(uri, out command))
                        {
                            link.Command = command;
                            link.CommandParameter = parameter;
                            if (targetName != null)
                            {
                                link.CommandTarget = source.FindName(targetName) as IInputElement;
                            }
                        }
                        else
                        {
                            link.NavigateUri = uri;
                            link.TargetName = parameter;
                        }
                        parent = link;
                        if (context.Foreground != null)
                        {
                            link.Foreground = context.Foreground;
                        }
                        span.Inlines.Add(parent);
                    }

                    if (context.IsListItem)
                    {
                        parent.Inlines.Add(context.CreateRun(token.Value));
                    }
                    else
                    {
                        Run run = context.CreateRun(token.Value);
                        parent.Inlines.Add(run);
                    }
                }
                else if (token.TokenType == BBCodeLexer.TokenLineBreak)
                {
                    span.Inlines.Add(new LineBreak());
                }
                else if (token.TokenType == BBCodeLexer.TokenAttribute)
                {
                    throw new ParseException(Resources.UnexpectedToken);
                }
                else if (token.TokenType == Lexer.TokenEnd)
                {
                    break;
                }
                else
                {
                    throw new ParseException(Resources.UnknownTokenType);
                }
            }
        }

        /// <summary>
        ///     Parses the text and returns a Span containing the parsed result.
        /// </summary>
        /// <returns></returns>
        public override Span Parse()
        {
            Span span = new Span();

            Parse(span);

            return span;
        }

        class ParseContext
        {
            public ParseContext(Span parent)
            {
                Parent = parent;
            }

            public Span Parent { get; }
            public double? FontSize { get; set; }
            public FontWeight? FontWeight { get; set; }
            public FontStyle? FontStyle { get; set; }
            public Brush Foreground { get; set; }
            public Brush Background { get; set; }
            public TextDecorationCollection TextDecorations { get; set; }
            public string NavigateUri { get; set; }
            public bool IsList { get; set; }
            public bool IsOrderedList { get; set; }
            public int ListCounter { get; set; }
            public bool IsListItem { get; set; }
            public bool IsFirstListItem { get; set; }

            /// <summary>
            ///     Creates a run reflecting the current context settings.
            /// </summary>
            /// <returns></returns>
            public Run CreateRun(string text)
            {
                Run run = new Run { Text = text };
                if (FontSize.HasValue)
                {
                    run.FontSize = FontSize.Value;
                }
                if (FontWeight.HasValue)
                {
                    run.FontWeight = FontWeight.Value;
                }
                if (FontStyle.HasValue)
                {
                    run.FontStyle = FontStyle.Value;
                }
                if (Foreground != null)
                {
                    run.Foreground = Foreground;
                }
                if (Background != null)
                {
                    run.Background = Background;
                }
                run.TextDecorations = TextDecorations;

                return run;
            }
        }
    }
}