using System.Collections.Generic;
using System.Linq;
using static System.Char;

namespace CapitalCrawler.Utils
{
    public static class SyntaxState
    {
        public const string Content = "content";
        public const string OpenBracket = "openBracket";
        public const string OpenTagName = "openTagName";
        public const string TagInner = "tagInner";
        public const string AttrName = "attributeName";
        public const string EqualWait = "equalWait";
        public const string Equal = "equal";
        public const string ValueWait = "valueWait";
        public const string AttrValueNQ = "nonQuotedAttributeValue";
        public const string AttrValueQ = "quotedAttributeValue";
        public const string EmptySlash = "emptySlash";
        public const string CloseBracket = "closeBracket";
        public const string CloseTagSlash = "closeTagSlash";
        public const string CloseTagName = "closeTagName";
        public const string WaitEndTagClose = "waitEndTagClose";

        public const char Lt = '<';
        public const char Slash = '/';
        public const char Gt = '>';
        public const char Eq = '=';
        public const char DQuote = '"';
        public const char SQuote = '\'';

        public const char Underscore = '_';
        public const char Colon = ':';
        public const char Hyphen = '-';
        public const char Period = '.';

        public static readonly List<string> InlineTags = new []{
            "area", "base", "br", "col", "command",
            "embed", "hr", "img", "input", "keygen",
            "link", "meta", "param", "source", "track", "wbr"
        }.ToList();

        private static bool IsStartChar(char c)
        {
            return IsLetter(c) || c == Underscore || c == Colon;
        }

        private static bool IsNamedChar(char c)
        {
            return IsLetterOrDigit(c) || c == Underscore || c == Hyphen || c == Period;
        }

        public static bool IsStartTagChar(this char c)
        {
            return IsStartChar(c);
        }

        public static bool IsStartAttrChar(this char c)
        {
            return IsStartChar(c);
        }

        public static bool IsTagChar(this char c)
        {
            return IsNamedChar(c);
        }

        public static bool IsAttrChar(this char c)
        {
            return IsNamedChar(c);
        }

        public static bool IsSpace(this char c)
        {
            return IsWhiteSpace(c);
        }
    }
}
