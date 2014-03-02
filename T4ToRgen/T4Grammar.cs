using System.Collections.Generic;
using System;
using System.Linq;


using Irony.Parsing;
using System.Collections.ObjectModel;
using T4ToRgen.Extension;


namespace T4ToRgen
{
    public class T4Grammar : Grammar
    {
        /// <summary>
        /// InvariantCultureIgnoreCase Comparer
        /// </summary>
        /// <remarks></remarks>
        public static readonly StringComparer Comparer = StringComparer.InvariantCultureIgnoreCase;
        private NonTerminal _directive;
        public NonTerminal Directive
        {
            get
            {
                return _directive;
            }
        }
   
        private NonTerminal _embeddedCs;
        public NonTerminal EmbeddedCs
        {
            get
            {
                return _embeddedCs;
            }
        }
        private NonTerminal _embeddedExpr;
        public NonTerminal EmbeddedExpr
        {
            get
            {
                return _embeddedExpr;
            }
        }
        private NonTerminal _embeddedClassMember;
        public NonTerminal EmbeddedClassMember
        {
            get
            {
                return _embeddedClassMember;
            }
        }
        private IdentifierTerminal _directiveName;
        public IdentifierTerminal DirectiveName
        {
            get
            {
                return _directiveName;
            }
        }
        private NonTerminal _attributeList;
        public NonTerminal AttributeList
        {
            get
            {
                return _attributeList;
            }
        }
        private KeyTerm _attributeConnector;
        public KeyTerm AttributeConnector
        {
            get
            {
                return _attributeConnector;
            }
        }
        private IdentifierTerminal _attributeName;
        public IdentifierTerminal AttributeName
        {
            get
            {
                return _attributeName;
            }
        }
        private StringLiteral _attributeValue;
        public StringLiteral AttributeValue
        {
            get
            {
                return _attributeValue;
            }
        }
        private NonTerminal _attribute;
        public NonTerminal Attribute
        {
            get
            {
                return _attribute;
            }
        }
        private readonly KeyTerm _beginSegment;
        public KeyTerm BeginSegment
        {
            get
            {
                return _beginSegment;
            }
        }
        private readonly KeyTerm _endSegment;
        public KeyTerm EndSegment
        {
            get
            {
                return _endSegment;
            }
        }
        private KeyTerm _embeddedClassMemberPrefix;
        public KeyTerm EmbeddedClassMemberPrefix
        {
            get
            {
                return _embeddedClassMemberPrefix;
            }
        }
        private FreeTextLiteral _embeddedClassMemberText;
        public FreeTextLiteral EmbeddedClassMemberText
        {
            get
            {
                return _embeddedClassMemberText;
            }
        }
        private FreeTextLiteral _expandedIncludeText;
        public FreeTextLiteral ExpandedIncludeText
        {
            get
            {
                return _expandedIncludeText;
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
            _directiveName = new IdentifierTerminal("directiveName");
            _expandedIncludeText = new FreeTextLiteral("expandedIncludeText");
            _attributeConnector = ToTerm("=");
            _attributeName = new IdentifierTerminal("attributeName") { SkipsWhitespaceAfter = false };
            _attributeValue = new StringLiteral("attributeValue", "\"");
            _attribute = new NonTerminal("attribute") {
                    Rule = _attributeName + _attributeConnector + _attributeValue };
            _attributeList = new NonTerminal("attributeList");
            _attributeList.Rule = MakePlusRule(_attributeList, _attribute);
            var directivePrefix = ToTerm("@");
            _directive = new NonTerminal("directive") {
                        Rule = beginSegment + directivePrefix + DirectiveName + AttributeList + endSegment };
            return Directive;
        }
        private NonTerminal InitEmbeddedCs(KeyTerm beginSegment, KeyTerm endSegment)
        {
            var regexNonDirective = new RegexBasedTerminal("NonDirectivePrefix", "(?![+=@])") {
                            ErrorAlias = "<#@ can only be declared before any text or embedded code",
                            SkipsWhitespaceAfter = false };
            _embeddedCs = new NonTerminal("embeddedCs");
            var embeddedCsText = new FreeTextLiteral("embeddedCsText", endSegment.Text);
            var stringLit = new StringLiteral("embeddedCsText");
            stringLit.AddStartEnd(beginSegment.Text, endSegment.Text, StringOptions.AllowsLineBreak);
            _embeddedCs.Rule = beginSegment + regexNonDirective + embeddedCsText + endSegment;
            return EmbeddedCs;
        }
        private NonTerminal InitEmbeddedExpr(KeyTerm beginSegment, KeyTerm endSegment)
        {
            _embeddedExpr = new NonTerminal("embeddedExpr");
            var embeddedExprText = new FreeTextLiteral("embeddedCsText", endSegment.Text);
            var embeddedExprPrefix = ToTerm("=");
            _embeddedExpr.Rule = beginSegment + embeddedExprPrefix + embeddedExprText + endSegment;
            return EmbeddedExpr;
        }
        private NonTerminal InitEmbeddedClassMember(KeyTerm beginSegment, KeyTerm endSegment)
        {
            _embeddedClassMemberText = new FreeTextLiteral("embeddedClassMemberText", endSegment.Text);
            _embeddedClassMemberPrefix = ToTerm("+");
            _embeddedClassMember = new NonTerminal("embeddedClassMember")
            {
                Rule = beginSegment + EmbeddedClassMemberPrefix + EmbeddedClassMemberText + endSegment
            };
            return EmbeddedClassMember;
        }
        public T4Grammar()
            : base(false)
        {
            _beginSegment = ToTerm("<#", "beginSegment");
            _beginSegment.SkipsWhitespaceAfter = false;
            _endSegment = ToTerm("#>", "endSegment");
            _endSegment.SkipsWhitespaceAfter = false;
            var directive = InitDirective(BeginSegment, EndSegment);
            var embeddedCs = InitEmbeddedCs(BeginSegment, EndSegment);
            var embeddedClassMember = InitEmbeddedClassMember(BeginSegment, EndSegment);
            var embeddedExpr = InitEmbeddedExpr(BeginSegment, EndSegment);
            var staticText = new FreeTextLiteral("staticText", FreeTextOptions.AllowEof, BeginSegment.Text);
            var segment = new NonTerminal("segment") { Rule = staticText | embeddedCs | embeddedExpr | embeddedClassMember | directive };
            var content = new NonTerminal("content");
            content.Rule = MakeStarRule(content, segment);
            var root = new NonTerminal("root") {Rule = content};
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
