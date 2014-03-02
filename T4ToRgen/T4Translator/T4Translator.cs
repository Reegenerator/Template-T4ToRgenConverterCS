using System;
using System.Collections.Generic;
using System.Linq;
using Irony.Parsing;
using Kodeo.Reegenerator.Wrappers;
using T4ToRgen.Extension;
using VSLangProj;

namespace T4ToRgen.T4Translator
{
    public partial class T4Translator
    {
        private class TranslationTag
        {
            public ParseTreeNode ExpandedParseTreeNode { get; set; }
        }
        private readonly HashSet<ParseTreeNode> _processed = new HashSet<ParseTreeNode>();
        private readonly Dictionary<BnfTerm, TranslationRule> _rgenTranslation = new Dictionary<BnfTerm, TranslationRule>();
        private readonly T4Grammar _grammar;
        private readonly Parser _parser;
        private readonly string[] _requiredAssemblies = { "Microsoft.VisualStudio.TextTemplating", "EnvDTE" };
        private readonly ProjectItem _projectItem;

        private readonly BnfTerm[] _getTranslatedTextTitleCaseTerms;
        public T4Translator(ProjectItem projItem, T4Grammar grmr, Parser parsr)
        {
            _grammar = grmr;
            _parser = parsr;
            _projectItem = projItem;
            _rgenTranslation.Add(_grammar.AttributeName, new TranslationRule(
                new Dictionary<string, string>(T4Grammar.Comparer)
                {
                    { "language", "Language" },
                        { "extension", "DefaultExtension" },
                            { "namespace", "Namespace" }
                            }));
            _rgenTranslation.Add(_grammar.BeginSegment, new TranslationRule("<%"));
            _rgenTranslation.Add(_grammar.EndSegment, new TranslationRule("%>"));
            _rgenTranslation.Add(_grammar.EmbeddedClassMemberPrefix, new TranslationRule(string.Empty));
            _getTranslatedTextTitleCaseTerms = new BnfTerm[] { _grammar.AttributeName, _grammar.DirectiveName };
        }

        public void Translate(RgenFileBuilders writers, ParseTreeNode rootNode)
        {
            _processed.Clear();
            writers.ImportedNamespaces.AddRange(_requiredAssemblies);
            IncludeFileManager.ExpandIncludeFiles(Convert.ToString(_projectItem.DteObject.DTE.Version), rootNode, _grammar, _parser);
            TranslateTemplateDirectives(writers, rootNode);
            TranslateNodeRecursive(writers, rootNode);
        }
        private void TranslateNodeRecursive(RgenFileBuilders sb, ParseTreeNode node)
        {
            if (SkipNode(node))
            {
                return;
            }
            WriteCodeBehindImportsStatement(sb, node);
            var tag = node.Tag as TranslationTag;
            if (node.Term == _grammar.EmbeddedClassMember)
            {
                TranslateEmbeddedClassMember(sb, node);
            }
            else
            {
                if (tag != null)
                {
                    TranslateNodeRecursive(sb, tag.ExpandedParseTreeNode);
                }
                else
                {
                    if (node.ChildNodes.Any())
                    {
                        foreach (var n in node.ChildNodes)
                        {
                            TranslateNodeRecursive(sb, n);
                        }
                    }
                    else
                    {
                        var translated = GetTranslatedText(node);
                        if (translated != null)
                        {
                            sb.TemplateFile.Append(translated);
                        }
                        _processed.Add(node);
                    }
                }
            }
        }
        private void TranslateEmbeddedClassMember(RgenFileBuilders sb, ParseTreeNode node)
        {
            var text = _grammar.GetEmbeddedClassMemberText(node);
            sb.ClassMembers.Append(text);
        }
        private void WriteCodeBehindImportsStatement(RgenFileBuilders sb, ParseTreeNode directiveNode)
        {
            if (directiveNode.Term != _grammar.Directive)
            {
                return;
            }
            var name = _grammar.GetDirectiveName(directiveNode);
            if (!T4Grammar.IsEqualText(Convert.ToString(name), "Import"))
            {
                return;
            }
            var namespc = _grammar.GetDirectiveAttributes(directiveNode)["Namespace"];
            sb.ImportedNamespaces.Add(namespc);
        }
        /// <summary>
        /// Some directive needs to be combined, so they need to be processed separately before other nodes
        /// </summary>
        /// <param name="rfb"></param>
        /// <param name="rootNode"></param>
        /// <remarks></remarks>
        private void TranslateTemplateDirectives(RgenFileBuilders rfb, ParseTreeNode rootNode)
        {
            try
            {
                var directives = (from c in rootNode.ChildNodes
                                  from gc in c.ChildNodes
                                  where gc.Term == _grammar.Directive
                                  select gc).ToArray();
                var templateDirectiveNames = new[] { "template", "output" };
                var templateDirectives = from d in directives
                                         where templateDirectiveNames.Contains(_grammar.GetDirectiveName(d), T4Grammar.Comparer)
                                         select d;
                var templateDirectiveAttributes = (from d in templateDirectives
                                                   from c in d.ChildNodes
                                                   where c.Term == _grammar.AttributeList
                                                   select c).ToArray();
                rfb.TemplateFile.Append(string.Format("<%@Template ClassName={0} ", rfb.ClassName.Quote()));
                foreach (var a in templateDirectiveAttributes)
                {
                    TranslateNodeRecursive(rfb, a);
                }
                rfb.TemplateFile.Append("%>");
                AddAssemblyDirectiveAsProjectReference(rfb, directives);
            }
            catch (Exception)
            {
                DebugExtensions.DebugHere();
            }
        }
        private readonly string[] _skipNodeProcessDirectives = { "import", "include" };
        public bool SkipNode(ParseTreeNode node)
        {
            var skip = false;
            if (_processed.Contains(node)) return true;

            if (node.Term == _grammar.Directive)
            {
                var dirName = _grammar.GetDirectiveName(node);
                skip = !_skipNodeProcessDirectives.Contains(dirName, T4Grammar.Comparer);
            }
            else if (node.Term == _grammar.Attribute)
            {
                var attrName = node.ChildNodes.First().FindTokenAndGetText();
                var found = _rgenTranslation[_grammar.AttributeName].ReplacementDict.ContainsKey(attrName);
                skip = !found;
            }


            if (skip)
            {
                _processed.Add(node);
            }
            return skip;
        }

        private string GetTranslatedText(ParseTreeNode node)
        {
            TranslationRule rule;
            _rgenTranslation.TryGetValue(node.Term, out rule);
            if (rule != null) return rule.Translate(node);

            return _getTranslatedTextTitleCaseTerms.Contains(node.Term) ? 
                TranslationRule.AsIs.Translate(node) : 
                TranslationRule.AsIsTitleCase.Translate(node);
        }
        public void AddAssemblyDirectiveAsProjectReference(RgenFileBuilders rfb, IEnumerable<ParseTreeNode> directives)
        {
            var assemblies = _grammar.GetReferencedAssemblies(directives);
            var vsProject = (VSProject)_projectItem.Project.DteObject.Object;
            foreach (var asm in assemblies.Union(_requiredAssemblies))
            {
                vsProject.References.Add(Convert.ToString(asm));
            }
        }
    }
}
