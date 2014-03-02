using System.Collections.Generic;


namespace T4ToRgen
{
    public partial class CodeBehind
    {

        private readonly string _className;
        public string ClassName
        {
            get
            {
                return _className;
            }
        }
        private readonly IEnumerable<string> _importedNamespaces;
        public IEnumerable<string> ImportedNamespaces
        {
            get
            {
                return _importedNamespaces;
            }
        }
        private readonly string _classMembers;
        public string ClassMembers
        {
            get
            {
                return _classMembers;
            }
        }
        public CodeBehind(IEnumerable<string> imprts, string cls, string members)
        {
            _importedNamespaces = imprts;
            _className = cls;
            _classMembers = members;
        }
    }
}
