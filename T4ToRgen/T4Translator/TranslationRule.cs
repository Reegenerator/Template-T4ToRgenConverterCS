using System;
using System.Collections.Generic;
using System.Globalization;
using Irony.Parsing;

namespace T4ToRgen.T4Translator
{
    public partial class T4Translator
    {
        private class TranslationRule
        {
            public static readonly TranslationRule AsIs_TitleCase = new TranslationRule(Types.AsIs_TitleCase);
            public static readonly TranslationRule AsIs = new TranslationRule(Types.AsIs_TitleCase);
            public enum Types
            {
                AsIs,
                AsIs_TitleCase,
                ConstantReplacement,
                CustomFunction,
                DictionaryReplacement,
                ExpandTag
            }
            public string ConstantReplacement { get; set; }
            public Dictionary<string, string> ReplacementDict { get; set; }
            public Func<ParseTreeNode, string> TranslateFunction { get; set; }
            public Types Type { get; set; }
            public TranslationRule(Types typ)
            {
                Type = typ;
            }
            public TranslationRule(string replacement)
            {
                Type = Types.ConstantReplacement;
                ConstantReplacement = replacement;
            }
            public TranslationRule(Func<ParseTreeNode, string> f)
            {
                Type = Types.CustomFunction;
                TranslateFunction = f;
            }
            public TranslationRule(Dictionary<string, string> dict)
            {
                Type = Types.DictionaryReplacement;
                ReplacementDict = dict;
            }
            private TextInfo ToTitleCase_textInfo = CultureInfo.InvariantCulture.TextInfo;
            public string ToTitleCase(string s)
            {
                return ToTitleCase_textInfo.ToTitleCase(s);
            }
            public string Translate(ParseTreeNode node)
            {
                string res = null;
                switch (Type)
                {
                    case Types.AsIs:
                        res = Convert.ToString(node.FindTokenAndGetText());
                        break;
                    case Types.AsIs_TitleCase:
                        res = ToTitleCase(Convert.ToString(node.FindTokenAndGetText()));
                        break;
                    case Types.ConstantReplacement:
                        res = ConstantReplacement;
                        break;
                    case Types.DictionaryReplacement:
                        var replacement = (string )null;
                        ReplacementDict.TryGetValue(node.FindTokenAndGetText(),out replacement);
                        res = replacement;
                        break;
                    case Types.CustomFunction:
                        res = Convert.ToString(TranslateFunction.Invoke(node));
                        break;
                    default:
                        break;
                }
                if (res != null)
                {
                    var terminal = node.Term as Terminal;
                    if (terminal != null)
                    {
                        if (terminal.SkipsWhitespaceAfter)
                        {
                            res += " ";
                        }
                    }
                }
                return res;
            }
        }
    }
}
