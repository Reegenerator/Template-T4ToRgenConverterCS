using System;
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
            if (WarnResultFileExists(rgenFile.FullName, codeBehindFile.FullName))
            {
                return null;
            }
            var grammar = new T4Grammar();
            var languageData = new LanguageData(grammar);
            var parser = new Parser(languageData);
            var t4Text = ProjectItem.GetCurrentContentAsString();
            var content = grammar.GetContentNode(parser.Parse(t4Text));
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
            var res = new RenderResults(msg);
            return res;
        }
    }
}
