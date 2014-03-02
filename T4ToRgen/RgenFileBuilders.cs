using System.Collections.Generic;
using System;
using System.Linq;
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
            _TemplateFile = new StringBuilder();
            _ClassMembers = new StringBuilder();
            _className = className;
        }
        private string _className;
        public string ClassName
        {
            get
            {
                return _className;
            }
        }
        private StringBuilder _TemplateFile;
        public StringBuilder TemplateFile
        {
            get
            {
                return _TemplateFile;
            }
        }
        private StringBuilder _ClassMembers;
        public StringBuilder ClassMembers
        {
            get
            {
                return _ClassMembers;
            }
        }
        private List<string> _ImportedNamespaces;
        public List<string> ImportedNamespaces
        {
            get
            {
                if (_ImportedNamespaces == null)
                {
                    _ImportedNamespaces = new List<string>();
                }
                return _ImportedNamespaces;
            }
        }
        public string GenCodeFile()
        {
            var codeBehindGen = new CodeBehind(ImportedNamespaces, ClassName, ClassMembers.ToString());
            var res = codeBehindGen.RenderToString();
            return res;
        }
    }
}
