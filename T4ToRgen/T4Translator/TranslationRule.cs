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
            public static readonly TranslationRule AsIsTitleCase = new TranslationRule(Types.AsIsTitleCase);
            public static readonly TranslationRule AsIs = new TranslationRule(Types.AsIsTitleCase);

            private enum Types
            {
                AsIs,
                AsIsTitleCase,
                ConstantReplacement,
                CustomFunction,
                DictionaryReplacement
            }

            private string ConstantReplacement { get; set; }
            public Dictionary<string, string> ReplacementDict { get; set; }
            private Func<ParseTreeNode, string> TranslateFunction { get; set; }
            private Types Type { get; set; }

            private TranslationRule(Types typ)
            {
                Type = typ;
            }
            public TranslationRule(string replacement)
            {
                Type = Types.ConstantReplacement;
                ConstantReplacement = replacement;
            }
            // ReSharper disable once UnusedMember.Local
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
            private readonly TextInfo _toTitleCaseTextInfo = CultureInfo.InvariantCulture.TextInfo;

            private string ToTitleCase(string s)
            {
                return _toTitleCaseTextInfo.ToTitleCase(s);
            }
            public string Translate(ParseTreeNode node)
            {
                string res = null;
                switch (Type)
                {
                    case Types.AsIs:
                        res = Convert.ToString(node.FindTokenAndGetText());
                        break;
                    case Types.AsIsTitleCase:
                        res = ToTitleCase(Convert.ToString(node.FindTokenAndGetText()));
                        break;
                    case Types.ConstantReplacement:
                        res = ConstantReplacement;
                        break;
                    case Types.DictionaryReplacement:
                        string replacement;
                        ReplacementDict.TryGetValue(node.FindTokenAndGetText(),out replacement);
                        res = replacement;
                        break;
                    case Types.CustomFunction:
                        res = Convert.ToString(TranslateFunction.Invoke(node));
                        break;
                }

                if (res == null) return res;
                //add space if necessary
                var terminal = node.Term as Terminal;
                if (terminal == null) return res;
                if (terminal.SkipsWhitespaceAfter)res += " ";
                
                return res;
            }
        }
    }
}
