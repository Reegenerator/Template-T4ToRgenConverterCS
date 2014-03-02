using System.Collections.Generic;
using System;
using System.Linq;


using System.Runtime.CompilerServices;
using EnvDTE;


namespace T4ToRgen
{
    public static class TextPointExtensions
    {
        
        public static TextDocument ToTextDocument(this Document doc)
        {
            return ((TextDocument) (doc.Object("TextDocument")));
        }
        
        public static string GetLineText(this TextPoint point)
        {
            var start = point.CreateEditPoint();
            start.StartOfLine();
            var endP = point.CreateEditPoint();
            endP.EndOfLine();
            return start.GetText(endP);
        }
        
        public static string GetText(this Document doc)
        {
            var textdoc = (TextDocument) (doc.Object("TextDocument"));
            return textdoc.StartPoint.CreateEditPoint().GetText(textdoc.EndPoint);
        }
        
        public static string GetLineTextAndNeighbor(this TextPoint point)
        {
            var start = point.CreateEditPoint();
            start.LineUp(1);
            start.StartOfLine();
            var endP = point.CreateEditPoint();
            endP.LineDown(1);
            endP.EndOfLine();
            return start.GetText(endP);
        }
        /// <summary>
        /// Inserts text and format the text (=Format Selection command)
        /// </summary>
        /// <param name="tp"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        
        public static TextPoint InsertAndFormat(this TextPoint tp, string text)
        {
            var start = tp.CreateEditPoint();
            var ep = tp.CreateEditPoint();
            ep.Insert(text);
            start.SmartFormat(ep);
            return ep;
        }
        /// <summary>
        /// Unlike  CharRight, CharRightExact counts newline \r\n as two instead of one char.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <remarks>
        /// DTE functions that moves editpoint counts newline as single character,
        /// since we get the character count from regular regex not the DTE find, the char count is slightly off
        /// </remarks>
        
        public static EditPoint CharRightExact(this EditPoint point, int count)
        {
            return CharMoveExact(point, count, 1);
        }
        /// <summary>
        /// See CharMoveExact
        /// </summary>
        /// <param name="point"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <remarks>See CharMoveExact</remarks>
        
        public static EditPoint CharLeftExact(this EditPoint point, int count)
        {
            return CharMoveExact(point, count, -1);
        }
        /// <summary>
        /// Moves cursor/editpoint exactly.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="count"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        /// <remarks>
        /// DTE functions that moves editpoint counts newline as single character,
        /// since we get the character count from regular regex not the DTE find, the char count is slightly off
        /// </remarks>
        public static EditPoint CharMoveExact(EditPoint point, int count, int direction)
        {
            while (count > 0)
            {
                if (direction > 1)
                {
                    direction = 1;
                }
                else
                {
                    if (direction < 0)
                    {
                        direction = -1;
                    }
                }
                if (point.GetText(direction).Length == 2)
                {
                    count--;
                }
                if (direction < 0)
                {
                    point.CharLeft(1);
                }
                else
                {
                    point.CharRight(1);
                }
                count--;
            }
            return point;
        }
    }
}
