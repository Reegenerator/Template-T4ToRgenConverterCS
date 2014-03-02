using System.Collections.Generic;
using System;
using System.Linq;


using Irony.Parsing;
using System.Collections.ObjectModel;


namespace T4ToRgen
{
    public class T4Grammar : Grammar
    {
        /// <summary>
        /// InvariantCultureIgnoreCase Comparer
        /// </summary>
        /// <remarks></remarks>
        public static readonly StringComparer Comparer = StringComparer.InvariantCultureIgnoreCase;
        private NonTerminal _Directive;
        public NonTerminal Directive
        {
            get
            {
                return _Directive;
            }
        }
   
        private NonTerminal _EmbeddedCs;
        public NonTerminal EmbeddedCs
        {
            get
            {
                return _EmbeddedCs;
            }
        }
        private NonTerminal _EmbeddedExpr;
        public NonTerminal EmbeddedExpr
        {
            get
            {
                return _EmbeddedExpr;
            }
        }
        private NonTerminal _EmbeddedClassMember;
        public NonTerminal EmbeddedClassMember
        {
            get
            {
                return _EmbeddedClassMember;
            }
        }
        private IdentifierTerminal _DirectiveName;
        public IdentifierTerminal DirectiveName
        {
            get
            {
                return _DirectiveName;
            }
        }
        private NonTerminal _AttributeList;
        public NonTerminal AttributeList
        {
            get
            {
                return _AttributeList;
            }
        }
        private KeyTerm _AttributeConnector;
        public KeyTerm AttributeConnector
        {
            get
            {
                return _AttributeConnector;
            }
        }
        private IdentifierTerminal _AttributeName;
        public IdentifierTerminal AttributeName
        {
            get
            {
                return _AttributeName;
            }
        }
        private StringLiteral _AttributeValue;
        public StringLiteral AttributeValue
        {
            get
            {
                return _AttributeValue;
            }
        }
        private NonTerminal _Attribute;
        public NonTerminal Attribute
        {
            get
            {
                return _Attribute;
            }
        }
        private KeyTerm _BeginSegment;
        public KeyTerm BeginSegment
        {
            get
            {
                return _BeginSegment;
            }
        }
        private KeyTerm _EndSegment;
        public KeyTerm EndSegment
        {
            get
            {
                return _EndSegment;
            }
        }
        private KeyTerm _EmbeddedClassMemberPrefix;
        public KeyTerm EmbeddedClassMemberPrefix
        {
            get
            {
                return _EmbeddedClassMemberPrefix;
            }
        }
        private FreeTextLiteral _EmbeddedClassMemberText;
        public FreeTextLiteral EmbeddedClassMemberText
        {
            get
            {
                return _EmbeddedClassMemberText;
            }
        }
        private FreeTextLiteral _ExpandedIncludeText;
        public FreeTextLiteral ExpandedIncludeText
        {
            get
            {
                return _ExpandedIncludeText;
            }
        }
        public static void CheckNodeType(ParseTreeNode node, BnfTerm term)
        {
            if (node.Term != term)
            {
                throw (new ArgumentException(string.Format("The node {0} is not a {1}", node.Term.Name, term.Name)));
            }
        }
        private NonTerminal InitDirective(KeyTerm beginSegment, KeyTerm endSegment)
        {
            _DirectiveName = new IdentifierTerminal("directiveName");
            _ExpandedIncludeText = new FreeTextLiteral("expandedIncludeText");
            _AttributeConnector = ToTerm("=");
            _AttributeName = new IdentifierTerminal("attributeName") { SkipsWhitespaceAfter = false };
            _AttributeValue = new StringLiteral("attributeValue", "\"");
            _Attribute = new NonTerminal("attribute") {
                    Rule = _AttributeName + _AttributeConnector + _AttributeValue };
            _AttributeList = new NonTerminal("attributeList");
            _AttributeList.Rule = MakePlusRule(_AttributeList, _Attribute);
            var directivePrefix = ToTerm("@");
            _Directive = new NonTerminal("directive") {
                        Rule = beginSegment + directivePrefix + DirectiveName + AttributeList + endSegment };
            return Directive;
        }
        private NonTerminal InitEmbeddedCs(KeyTerm beginSegment, KeyTerm endSegment)
        {
            var regexNonDirective = new RegexBasedTerminal("NonDirectivePrefix", "(?![+=@])") {
                            ErrorAlias = "<#@ can only be declared before any text or embedded code",
                            SkipsWhitespaceAfter = false };
            _EmbeddedCs = new NonTerminal("embeddedCs");
            var embeddedCsText = new FreeTextLiteral("embeddedCsText", endSegment.Text);
            var stringLit = new StringLiteral("embeddedCsText");
            stringLit.AddStartEnd(beginSegment.Text, endSegment.Text, StringOptions.AllowsLineBreak);
            _EmbeddedCs.Rule = beginSegment + regexNonDirective + embeddedCsText + endSegment;
            return EmbeddedCs;
        }
        private NonTerminal InitEmbeddedExpr(KeyTerm beginSegment, KeyTerm endSegment)
        {
            _EmbeddedExpr = new NonTerminal("embeddedExpr");
            var embeddedExprText = new FreeTextLiteral("embeddedCsText", endSegment.Text);
            var embeddedExprPrefix = ToTerm("=");
            _EmbeddedExpr.Rule = beginSegment + embeddedExprPrefix + embeddedExprText + endSegment;
            return EmbeddedExpr;
        }
        private NonTerminal InitEmbeddedClassMember(KeyTerm beginSegment, KeyTerm endSegment)
        {
            _EmbeddedClassMemberText = new FreeTextLiteral("embeddedClassMemberText", endSegment.Text);
            _EmbeddedClassMemberPrefix = ToTerm("+");
            _EmbeddedClassMember = new NonTerminal("embeddedClassMember");
            _EmbeddedClassMember.Rule = beginSegment + EmbeddedClassMemberPrefix + EmbeddedClassMemberText + endSegment;
            return EmbeddedClassMember;
        }
        public T4Grammar()
            : base(false)
        {
            _BeginSegment = ToTerm("<#", "beginSegment");
            _BeginSegment.SkipsWhitespaceAfter = false;
            _EndSegment = ToTerm("#>", "endSegment");
            _EndSegment.SkipsWhitespaceAfter = false;
            var directive = InitDirective(BeginSegment, EndSegment);
            var embeddedCs = InitEmbeddedCs(BeginSegment, EndSegment);
            var embeddedClassMember = InitEmbeddedClassMember(BeginSegment, EndSegment);
            var embeddedExpr = InitEmbeddedExpr(BeginSegment, EndSegment);
            var staticText = new FreeTextLiteral("staticText", FreeTextOptions.AllowEof, BeginSegment.Text);
            var segment = new NonTerminal("segment") { Rule = staticText | embeddedCs | embeddedExpr | embeddedClassMember | directive };
            var content = new NonTerminal("content");
            content.Rule = MakeStarRule(content, segment);
            var root = new NonTerminal("root");
            root.Rule = content;
            Root = root;
        }
        public ParseTreeNode GetContentNode(ParseTree parseTree)
        {
            return parseTree.Root.ChildNodes.First();
        }
        public void RegisterBracePair(KeyTerm open, KeyTerm close)
        {
            open.SetFlag(TermFlags.IsOpenBrace);
            open.IsPairFor = close;
            close.SetFlag(TermFlags.IsCloseBrace);
            close.IsPairFor = open;
        }
        public string GetDirectiveName(ParseTreeNode directiveNode)
        {
            return Convert.ToString( directiveNode.ChildNodes.First(x => x.Term == DirectiveName).FindTokenAndGetText());
        }
        public string GetEmbeddedClassMemberText(ParseTreeNode node)
        {
            return Convert.ToString( node.ChildNodes.First(x => x.Term == EmbeddedClassMemberText).FindTokenAndGetText());
        }
        public string[] GetReferencedAssemblies(IEnumerable<ParseTreeNode> directives)
        {
            var assemblyDirs = from d in directives
                                                                  let name = GetDirectiveName(d)
                                                                  where IsEqualText(Convert.ToString(name), "Assembly")
                                                                  select GetDirectiveAttributes(d)["Name"];
            return assemblyDirs.ToArray();
        }
        public ReadOnlyDictionary<string, string> GetDirectiveAttributes(ParseTreeNode directiveNode)
        {
            var dict = new Dictionary<string, string>(Comparer);
            var attrlist = directiveNode.FindChildNode(AttributeList);
            foreach (var attr in attrlist.ChildNodes)
            {
                var attrName = string.Empty;
                var attrValue = string.Empty;
                foreach (var n in attr.ChildNodes)
                {
                    if (n.Term == AttributeName)
                    {
                        attrName = Convert.ToString(n.Token.ValueString);
                    }
                    else
                    {
                        if (n.Term == AttributeValue)
                        {
                            attrValue = Convert.ToString(n.Token.ValueString);
                        }
                    }
                }
                dict.Add(attrName, attrValue);
            }
            return new ReadOnlyDictionary<string, string>(dict);
        }
        public static bool IsEqualText(string s1, string s2)
        {
            return Comparer.Compare(s1, s2) == 0;
        }
    }
}
