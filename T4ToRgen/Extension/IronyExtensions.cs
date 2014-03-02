using System.Collections.Generic;
using System;
using System.Linq;


using System.Runtime.CompilerServices;
using Irony.Parsing;


namespace T4ToRgen
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
            if (child == null)
            {
                return string.Empty;
            }
            else
            {
                return child.FindTokenAndGetText();
            }
        }
        
        public static string FindChildValue(this ParseTreeNode parentNode, BnfTerm term)
        {
            var child = parentNode.FindChildNode(term);
            if (child == null)
            {
                return string.Empty;
            }
            else
            {
                return child.FindToken().ValueString;
            }
        }
    }
}
