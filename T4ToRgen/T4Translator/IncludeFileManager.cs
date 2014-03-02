using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Irony.Parsing;
using Microsoft.Win32;

namespace T4ToRgen.T4Translator
{
    public partial class T4Translator
    {
        /// <summary>
        /// Look for @Include directives, find the the files, parse it and store it in the IncludeDirectiveNode.Tag
        /// </summary>
        /// <remarks></remarks>
        private class IncludeFileManager
        {
            private readonly DirectoryInfo[] _includeDirs;
            private readonly HashSet<string> _expandedFiles;
            private IncludeFileManager(string dteVersion)
            {
                _includeDirs = GetT4IncludeDirectories(dteVersion);
                _expandedFiles = new HashSet<string>();
            }

           
            /// <summary>
            /// Insert the string from the include file inside the include directive tag
            /// </summary>
            /// <remarks></remarks>
            private void ExpandIncludeFiles(ParseTreeNode rootNode, T4Grammar grammar, Parser parser)
            {
                var includes = from n in rootNode.ChildNodes
                                   let segmentChild = n.ChildNodes.First()
                                   where segmentChild.Term == grammar.Directive
                                   let dir = segmentChild
                                   let dirName = grammar.GetDirectiveName(dir)
                                   where T4Grammar.IsEqualText(Convert.ToString(dirName), "include")
                                   select dir;
                var includeFiles = from incDir in includes
                                       let filepath = grammar.GetDirectiveAttributes(incDir)["File"]
                                       select new
                                   { DirectiveNode = incDir, File = GetIncludeFile(Convert.ToString(filepath)) };
                foreach (var incFile in includeFiles)
                {
                    if (_expandedFiles.Contains(incFile.File.FullName))
                    {
                        continue;
                    }
                    var text = File.ReadAllText(Convert.ToString(incFile.File.FullName));
                    var content = grammar.GetContentNode(parser.Parse(text));
                    incFile.DirectiveNode.Tag = new TranslationTag { ExpandedParseTreeNode = content };
                    _expandedFiles.Add(incFile.File.FullName);

                    //recursively process content from include file
                    ExpandIncludeFiles(content, grammar, parser);
                }
            }
            public static void ExpandIncludeFiles(string dteVersion, ParseTreeNode rootNode, T4Grammar grammar, Parser parser)
            {
                var mgr = new IncludeFileManager(dteVersion);
                mgr.ExpandIncludeFiles(rootNode, grammar, parser);
            }

            private FileInfo GetIncludeFile(string path)
            {
                FileInfo file = null;
                if (Path.IsPathRooted(path))
                {
                    file = new FileInfo(path);
                }
                else
                {
                    foreach (var d in _includeDirs)
                    {
                        var fileInDir = new FileInfo(Path.Combine(d.FullName, path));
                        if (fileInDir.Exists)
                        {
                            file = fileInDir;
                            break;
                        }
                    }
                }
                return file;
            }
            private static DirectoryInfo[] GetT4IncludeDirectories(string dteVersion)
            {
                const string regPattern = "Software\\Microsoft\\VisualStudio\\{0}_Config\\TextTemplating\\IncludeFolders\\.tt";
                var regPath = string.Format(regPattern, dteVersion);
                var key = Registry.CurrentUser.OpenSubKey(regPath);
                if (key == null)
                {
                    throw new Exception(string.Format("{0} for VS v{1} T4 include folders was not found", regPath, dteVersion));
                }

                return key.GetValueNames().
                        Select(vn => key.GetValue(vn).ToString()).
                        Select(path => new DirectoryInfo(path)).ToArray();
            }
        }
    }
}
