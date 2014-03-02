using System.Collections.Generic;
using System.Text;


namespace T4ToRgen
{
    /// <summary>
    /// Class to hold the Template StringBuilder and Code behind StringBuilder and other information required to build the Rgen template
    /// </summary>
    /// <remarks></remarks>
    public class RgenFileBuilders
    {
        public RgenFileBuilders(string className)
        {
            _templateFile = new StringBuilder();
            _classMembers = new StringBuilder();
            _className = className;
        }
        private readonly string _className;
        public string ClassName
        {
            get
            {
                return _className;
            }
        }
        private readonly StringBuilder _templateFile;
        public StringBuilder TemplateFile
        {
            get
            {
                return _templateFile;
            }
        }
        private readonly StringBuilder _classMembers;
        public StringBuilder ClassMembers
        {
            get
            {
                return _classMembers;
            }
        }
        private List<string> _importedNamespaces;
        public List<string> ImportedNamespaces
        {
            get { return _importedNamespaces ?? (_importedNamespaces = new List<string>()); }
        }
        public string GenCodeFile()
        {
            var codeBehindGen = new CodeBehind(ImportedNamespaces, ClassName, ClassMembers.ToString());
            var res = codeBehindGen.RenderToString();
            return res;
        }
    }
}
