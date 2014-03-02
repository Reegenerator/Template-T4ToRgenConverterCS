using System;
using System.Linq;
using Irony.Parsing;
using Kodeo.Reegenerator.Generators;
using System.IO;
using System.Windows.Forms;
using T4ToRgen.Extension;


namespace T4ToRgen
{
    public partial class T4ToRgenCs
    {
   
        [TemplateProperty(Description = "In addition to directory specified by VS in registry, also look for included T4 in these directories")]
        public string AdditionalIncludePaths { get; set; }
        public bool WarnResultFileExists(string rgenFile, string codeBehindFile)
        {
            const string msgFormat = "The destination file {0} already exists. Please remove the file";
            if (File.Exists(rgenFile))
            {
                MessageBox.Show(string.Format(msgFormat, rgenFile));
                return true;
            }
            if (File.Exists(codeBehindFile))
            {
                MessageBox.Show(string.Format(msgFormat, rgenFile));
                return true;
            }
            return false;
        }
        public override RenderResults Render()
        {
            var itemName = Path.GetFileNameWithoutExtension(ProjectItem.FileName);
            var className = itemName.ToIdentifier();
            var rgenFile = new FileInfo(ProjectItem.ExpandPath(className + ".rgt"));
            var codeBehindFile = new FileInfo(ProjectItem.ExpandPath(className + ".rgt.cs"));
            //check existing file
            if (WarnResultFileExists(rgenFile.FullName, codeBehindFile.FullName))
            {
                return null;
            }
            //init grammar
            var grammar = new T4Grammar();
            var languageData = new LanguageData(grammar);
            var parser = new Parser(languageData);
            //parse
            var t4Text = ProjectItem.GetCurrentContentAsString();
            ParseTree parseTree;
            parseTree = parser.Parse(t4Text);
            //check parse result
            if (parseTree.Status == ParseTreeStatus.Error)
            {
                var errors = string.Join(Newline, 
                    parseTree.ParserMessages.Select(m =>  
                        m.Location + " " + m.Message
                    ).ToArray());
                MessageBox.Show("Parsing Failed.\r\n" + errors);
                return new RenderResults(errors);
            }


            var content = grammar.GetContentNode(parseTree);
            //Translate
            var translator = new T4Translator.T4Translator(ProjectItem, grammar, parser);
            var writers = new RgenFileBuilders(Convert.ToString(className));
            translator.Translate(writers, content);
            File.WriteAllText(rgenFile.FullName, writers.TemplateFile.ToString());
            var rgenProjItem = ProjectItem.Project.DteObject.ProjectItems.AddFromFile(rgenFile.FullName);
            File.WriteAllText(codeBehindFile.FullName, writers.GenCodeFile());
            rgenProjItem.ProjectItems.AddFromFile(codeBehindFile.FullName);

            var msg =
                GetIsolatedOutput(
                    () => GenReadme(rgenFile.FullName, codeBehindFile.FullName, writers.ImportedNamespaces.ToArray()));
         
            return new RenderResults(msg);;
        }
    }
}
