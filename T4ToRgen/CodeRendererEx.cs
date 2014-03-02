using System.Collections.Generic;
using System;
using System.Linq;
using EnvDTE80;
using Kodeo.Reegenerator.Generators;
using EnvDTE;
using System.Text;






namespace T4ToRgen
{
    public abstract class CodeRendererEx : CodeRenderer
    {
        private string SavedOutput;
        public readonly string Newline = Environment.NewLine;
        public DTE DTE
        {
            get
            {
                return base.ProjectItem.DteObject.DTE;
            }
        }
        public DTE2 DTE2
        {
            get
            {
                return ((DTE2) DTE);
            }
        }
        public bool IsVs2013
        {
            get
            {
                return DTE2.Version == "12.0";
            }
        }
        public bool IsVs2012
        {
            get
            {
                return DTE2.Version == "11.0";
            }
        }
        public StringBuilder _OutputBuilder;
        public StringBuilder OutputBuilder
        {
            get
            {
                if (_OutputBuilder == null)
                {
                    _OutputBuilder = Output.GetStringBuilder();
                }
                return _OutputBuilder;
            }
        }
        public CodeElement[] GetCodeElementsAtCursor(vsCMElement? kind = null)
        {
            var sel = (TextSelection) DTE.ActiveDocument.Selection;
            var pnt = (TextPoint) sel.ActivePoint;
            var fcm =
                DTE.ActiveDocument.ProjectItem.FileCodeModel;
            var res = GetCodeElementsAtPoint(fcm, pnt);
            if (kind.HasValue)
            {
                res = res.Where(x => x.Kind == vsCMElement.vsCMElementProperty).ToArray();
            }
            return res;
        }
        public IEnumerable<T> GetCodeElementsAtCursor<T>()
            where T: class
        {
            var kind = default(vsCMElement);
            if (typeof(T) == typeof(CodeProperty))
            {
                kind = vsCMElement.vsCMElementProperty;
            }
            else
            {
                if (typeof(T) == typeof(CodeClass))
                {
                    kind = vsCMElement.vsCMElementClass;
                }
            }
            var ce = GetCodeElementsAtCursor(kind);
            return ce.Cast<T>();
        }
        public string RenderToString()
        {
            return ASCIIEncoding.ASCII.GetString(Render().GeneratedCode);
        }
        public CodeElement[] GetCodeElementsAtPoint(FileCodeModel fcm, TextPoint pnt)
        {
            var res = new List<CodeElement>();
            var elem = default(CodeElement);
            var scope = default(vsCMElement);
            foreach (vsCMElement tempLoopVar_scope in Enum.GetValues(scope.GetType()))
            {
                scope = tempLoopVar_scope;
                try
                {
                    elem = fcm.CodeElementFromPoint(pnt, scope);
                    if (elem != null)
                    {
                        res.Add(elem);
                    }
                }
                catch (Exception)
                {
                }
            }
            return res.ToArray();
        }
        public TextSelection GetTextSelection()
        {
            return ((TextSelection) DTE.ActiveDocument.Selection);
        }
        public string SaveAndClearOutput()
        {
            OutputBuilder.Insert(0, SavedOutput);
            SavedOutput = Output.ToString();
            OutputBuilder.Clear();
            return SavedOutput;
        }
        /// <summary>
        /// Instead of generating to a file. This is a workaround to return the value as string
        /// </summary>
        /// <param name="action"></param>
        /// <param name="removeEmptyLines"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string GetIsolatedOutput(Action action, bool removeEmptyLines = true)
        {
            SaveAndClearOutput();
            action();
            var s = RestoreOutput();
            if (removeEmptyLines)
            {
                s = Convert.ToString(s.RemoveEmptyLines());
            }
            return s;
        }
        /// <summary>
        /// Restore saved output while returning current output
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public string RestoreOutput()
        {
            var saved = SavedOutput;
            SavedOutput = string.Empty;
            var curr = Output.ToString();
            OutputBuilder.Clear();
            OutputBuilder.Append(saved);
            return curr;
        }
        public void DebugWrite(string text)
        {
            OutputPaneTraceListener.Write(text);
        }
        public void DebugWriteLine(string text)
        {
            OutputPaneTraceListener.WriteLine(text);
        }
    }
}
