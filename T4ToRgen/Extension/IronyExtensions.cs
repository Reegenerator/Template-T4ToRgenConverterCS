using System.Linq;
using Irony.Parsing;

namespace T4ToRgen.Extension
{
    internal static class IronyExtensions
    {
        
        public static ParseTreeNode FindChildNode(this ParseTreeNode parentNode, BnfTerm term)
        {
            return parentNode.ChildNodes.FirstOrDefault(x => x.Term == term);
        }
        
        public static string FindChildText(this ParseTreeNode parentNode, BnfTerm term)
        {
            var child = parentNode.FindChildNode(term);
            return child == null ? string.Empty : child.FindTokenAndGetText();
        }
        
        public static string FindChildValue(this ParseTreeNode parentNode, BnfTerm term)
        {
            var child = parentNode.FindChildNode(term);
            return child == null ? string.Empty : child.FindToken().ValueString;
        }
    }
}
