using System.Collections.Generic;
using System;
using System.Linq;





namespace T4ToRgen
{
    public partial class CodeBehind
    {

        private string _ClassName;
        public string ClassName
        {
            get
            {
                return _ClassName;
            }
        }
        private IEnumerable<string> _ImportedNamespaces;
        public IEnumerable<string> ImportedNamespaces
        {
            get
            {
                return _ImportedNamespaces;
            }
        }
        private string _ClassMembers;
        public string ClassMembers
        {
            get
            {
                return _ClassMembers;
            }
        }
        public CodeBehind(IEnumerable<string> imprts, string cls, string members)
        {
            _ImportedNamespaces = imprts;
            _ClassName = cls;
            _ClassMembers = members;
        }
    }
}
