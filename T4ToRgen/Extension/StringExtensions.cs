using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.VisualBasic;


using System.Runtime.CompilerServices;
using System.Text;


namespace T4ToRgen
{
    public static class StringExtensions
    {
        
        public static StringBuilder AppendFormatLine(this StringBuilder sb, string format, params object[] values)
        {
            return sb.AppendFormat(format, values).AppendLine();
        }
        private static char AppendIndentFormat_tab = Constants.vbTab.First();
        public static StringBuilder AppendIndentFormat(this StringBuilder sb, int tabCount, string format, params object[] values)
        {
            return sb.AppendFormat("{0}{1}", new string(AppendIndentFormat_tab, tabCount), string.Format(format, values));
        }
        private static char AppendIndent_tab = Constants.vbTab.First();
        public static StringBuilder AppendIndent(this StringBuilder sb, int tabCount, string text)
        {
            return sb.AppendFormat("{0}{1}", new string(AppendIndent_tab, tabCount), text);
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
