using System;
using System.Collections.Generic;
using System.Text;
using static CapitalCrawler.Utils.SyntaxState;

namespace CapitalCrawler.Utils
{
    public static class XmlSyntaxChecker
    {
        /// <summary>
        /// Return well-formed xml string.
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string ToWellFormed(this string src)
        {
            src += ' ';
            var reader = src.ToCharArray();
            var writer = new StringBuilder();

            var openTag = new StringBuilder();
            var closeTag = new StringBuilder();

            var isEmptyTag = false;
            var isOpenTag = false;
            var isCloseTag = false;

            var attrName = new StringBuilder();
            var attrValue = new StringBuilder();
            var attributes = new Dictionary<string, string>();

            var content = new StringBuilder();
            var stack = new Stack<string>();

            char? quote = null;

            var state = Content;

            foreach (var c in reader)
            {
                switch (state)
                {
                    case Content:
                        if (c == Lt)
                        {
                            state = OpenBracket;
                            writer.Append(content.ToString().Trim().Replace("&", "&amp;"));
                        }
                        else
                        {
                            content.Append(c);
                        }

                        break;
                    case OpenBracket:
                        if (c.IsStartTagChar())
                        {
                            state = OpenTagName;

                            isOpenTag = true;
                            isCloseTag = false;
                            isEmptyTag = false;

                            openTag.Clear();
                            openTag.Append(c);
                        }
                        else if (c == Slash)
                        {
                            state = CloseTagSlash;

                            isOpenTag = false;
                            isCloseTag = true;
                            isEmptyTag = false;
                        }

                        break;
                    case OpenTagName:
                        if (c.IsTagChar())
                        {
                            openTag.Append(c);
                        }
                        else if (c.IsSpace())
                        {
                            state = TagInner;
                            attributes.Clear();
                        }
                        else if (c == Gt)
                        {
                            state = CloseBracket;
                        }
                        else if (c == Slash)
                        {
                            state = EmptySlash;
                        }

                        break;
                    case TagInner:
                        if (c.IsSpace())
                        {

                        }
                        else if (c.IsStartAttrChar())
                        {
                            state = AttrName;
                            attrName.Clear();
                            attrName.Append(c);
                        }
                        else if (c == Gt)
                        {
                            state = CloseBracket;
                        }
                        else if (c == Slash)
                        {
                            state = EmptySlash;
                        }

                        break;
                    case AttrName:
                        if (c.IsAttrChar())
                        {
                            attrName.Append(c);
                        }
                        else if (c == Eq)
                        {
                            state = Equal;
                        }
                        else if (c.IsSpace())
                        {
                            state = EqualWait;
                        }
                        else
                        {
                            if (c == Slash)
                            {
                                attributes.Add(attrName.ToString(), "true");
                                state = EmptySlash;
                            }
                            else if (c == Gt)
                            {
                                attributes.Add(attrName.ToString(), "true");
                                state = CloseBracket;
                            }
                        }

                        break;
                    case EqualWait:
                        if (c.IsSpace())
                        {

                        }
                        else if (c == Eq)
                        {
                            state = Equal;
                        }
                        else
                        {
                            if (c.IsStartAttrChar())
                            {
                                attributes.Add(attrName.ToString(), "true");
                                state = AttrName;

                                attrName.Clear();
                                attrName.Append(c);
                            }
                        }

                        break;
                    case Equal:
                        if (c.IsSpace())
                        {

                        }
                        else if (c == DQuote || c == SQuote)
                        {
                            state = AttrValueQ;
                            quote = c;

                            attrValue.Clear();
                        }
                        else if (!c.IsSpace() && c != Gt)
                        {
                            state = AttrValueNQ;
                            attrValue.Clear();
                            attrValue.Append(c);
                        }

                        break;
                    case AttrValueQ:
                        if (c != quote)
                        {
                            attrValue.Append(c);
                        } else if (c == quote)
                        {
                            state = TagInner;
                            attributes.Add(attrName.ToString(), attrValue.ToString());
                        }

                        break;
                    case AttrValueNQ:
                        if (!c.IsSpace() && c != Gt && c != Slash)
                        {
                            attrValue.Append(c);
                        } else if (c.IsSpace())
                        {
                            state = TagInner;
                            attributes.Add(attrName.ToString(), attrValue.ToString());
                        } else if (c == Gt)
                        {
                            state = CloseBracket;
                            attributes.Add(attrName.ToString(), attrValue.ToString());
                        } else if (c == Slash)
                        {
                            state = EmptySlash;
                            attributes.Add(attrName.ToString(), attrValue.ToString());
                        }

                        break;
                    case EmptySlash:
                        if (c == Gt)
                        {
                            state = CloseBracket;
                            isEmptyTag = true;
                        }

                        break;
                    case CloseTagSlash:
                        if (c.IsStartTagChar())
                        {
                            state = CloseTagName;
                            closeTag.Clear();
                            closeTag.Append(c);
                        }

                        break;
                    case CloseTagName:
                        if (c.IsTagChar())
                        {
                            closeTag.Append(c);
                        } else if (c.IsSpace())
                        {
                            state = WaitEndTagClose;
                        } else if (c == Gt)
                        {
                            state = CloseBracket;
                        }

                        break;
                    case WaitEndTagClose:
                        if (c.IsSpace())
                        {

                        }
                        else if (c == Gt)
                        {
                            state = CloseBracket;
                        }

                        break;
                    case CloseBracket:
                        if (isOpenTag)
                        {
                            var openTagName = openTag.ToString().ToLower();

                            if (InlineTags.Contains(openTagName))
                            {
                                isEmptyTag = true;
                            }

                            writer.Append(Lt)
                                .Append(openTagName)
                                .Append(Convert(attributes))
                                .Append(isEmptyTag ? "/" : "")
                                .Append(Gt);

                            attributes.Clear();

                            if (!isEmptyTag)
                            {
                                stack.Push(openTagName);
                            }
                        } else if (isCloseTag)
                        {
                            var closeTagName = closeTag.ToString().ToLower();
                            if (stack.Count != 0 && stack.Contains(closeTagName))
                            {
                                while (stack.Count != 0 && !stack.Peek().Equals(closeTagName))
                                {
                                    writer.Append(Lt)
                                        .Append(Slash)
                                        .Append(stack.Pop())
                                        .Append(Gt);
                                }

                                if (stack.Count != 0 && stack.Peek().Equals(closeTagName))
                                {
                                    writer.Append(Lt)
                                        .Append(Slash)
                                        .Append(stack.Pop())
                                        .Append(Gt);
                                }
                            }
                        }

                        if (c == Lt)
                        {
                            state = OpenBracket;
                        }
                        else
                        {
                            state = Content;

                            content.Clear();
                            content.Append(c); 
                        }

                        break;
                }
            }

            if (state.Equals(Content))
            {
                writer.Append(content.ToString().Trim().Replace("&", "&amp;"));
            }

            while (stack.Count != 0)
            {
                writer.Append(Lt)
                    .Append(Slash)
                    .Append(stack.Pop())
                    .Append(Gt);
            }

            return writer.ToString();
        }

        private static string Convert(Dictionary<string, string> attributes)
        {
            if (attributes.Count == 0)
            {
                return "";
            }

            var builder = new StringBuilder();
            foreach (var (attrName, attrValue) in attributes)
            {
                var value = attrValue
                    .Replace("&", "&amp;")
                    .Replace("\"", "&quot;")
                    .Replace("'", "&apos;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;");

                builder.Append(attrName)
                    .Append("=")
                    .Append("\"").Append(value).Append("\"")
                    .Append(" ");
            }

            var result = builder.ToString().Trim();
            if (!string.IsNullOrEmpty(result))
            {
                result = " " + result;
            }

            return result;
        }
    }
}
