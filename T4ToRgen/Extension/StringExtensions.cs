using System;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;

namespace T4ToRgen.Extension
{
    public static class StringExtensions
    {
        
        public static StringBuilder AppendFormatLine(this StringBuilder sb, string format, params object[] values)
        {
            return sb.AppendFormat(format, values).AppendLine();
        }
        private static readonly char AppendIndentFormatTab = Constants.vbTab.First();
        public static StringBuilder AppendIndentFormat(this StringBuilder sb, int tabCount, string format, params object[] values)
        {
            return sb.AppendFormat("{0}{1}", new string(AppendIndentFormatTab, tabCount), string.Format(format, values));
        }
        private static readonly char AppendIndentTab = Constants.vbTab.First();
        public static StringBuilder AppendIndent(this StringBuilder sb, int tabCount, string text)
        {
            return sb.AppendFormat("{0}{1}", new string(AppendIndentTab, tabCount), text);
        }
        /// <summary>
        /// Join two strings , only if both are not empty strings
        /// </summary>
        /// <param name="leftSide"></param>
        /// <param name="conjunction"></param>
        /// <param name="rightSide"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string Conjoin(this string leftSide, string conjunction, string rightSide)
        {
            return Convert.ToString( leftSide + Convert.ToString(leftSide != string.Empty && rightSide != string.Empty ? conjunction : string.Empty) + rightSide);
        }

        
        public static string Quote(this string s)
        {
            return "\"" + s + "\"";
        }
        
        public static string ToIdentifier(this string s)
        {
            return s.Replace(".", "_");
        }
        /// <summary>
        /// Compares two strings using StringComparison.InvariantCultureIgnoreCase
        /// </summary>
        /// <param name="s"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        
        public static bool IsEqualText(this string s, string s2)
        {
            return string.Compare(s, s2, StringComparison.InvariantCultureIgnoreCase) == 0;
        }
    }
}
