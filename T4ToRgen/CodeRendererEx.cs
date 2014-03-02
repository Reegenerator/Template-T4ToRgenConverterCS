using System.Collections.Generic;
using System;
using System.Linq;
using EnvDTE80;
using Kodeo.Reegenerator.Generators;
using EnvDTE;
using System.Text;
using T4ToRgen.Extension;


namespace T4ToRgen
{
    public abstract class CodeRendererEx : CodeRenderer
    {
        private string _savedOutput;
        public readonly string Newline = Environment.NewLine;
        public DTE Dte
        {
            get
            {
                return ProjectItem.DteObject.DTE;
            }
        }
        public DTE2 Dte2
        {
            get
            {
                return ((DTE2) Dte);
            }
        }
        public bool IsVs2013
        {
            get
            {
                return Dte2.Version == "12.0";
            }
        }
        public bool IsVs2012
        {
            get
            {
                return Dte2.Version == "11.0";
            }
        }

        private StringBuilder _outputBuilder;
        public StringBuilder OutputBuilder
        {
            get { return _outputBuilder ?? (_outputBuilder = Output.GetStringBuilder()); }
        }
        public CodeElement[] GetCodeElementsAtCursor(vsCMElement? kind = null)
        {
            var sel = (TextSelection) Dte.ActiveDocument.Selection;
            var pnt = (TextPoint) sel.ActivePoint;
            var fcm =
                Dte.ActiveDocument.ProjectItem.FileCodeModel;
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
            return Encoding.ASCII.GetString(Render().GeneratedCode);
        }
        public CodeElement[] GetCodeElementsAtPoint(FileCodeModel fcm, TextPoint pnt)
        {
            var res = new List<CodeElement>();
            foreach (var scope in Enum.GetValues(typeof(vsCMElement)).Cast<vsCMElement>())
            {
                
                try
                {
                    var elem = fcm.CodeElementFromPoint(pnt, scope);
                    if (elem != null)
                    {
                        res.Add(elem);
                    }
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch (Exception)
                {
                    //don’t do anything -
                    //this is expected when no code elements are in scope
                }
            }
            return res.ToArray();
        }
        public TextSelection GetTextSelection()
        {
            return ((TextSelection) Dte.ActiveDocument.Selection);
        }
        public string SaveAndClearOutput()
        {
            OutputBuilder.Insert(0, _savedOutput);
            _savedOutput = Output.ToString();
            OutputBuilder.Clear();
            return _savedOutput;
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
            var saved = _savedOutput;
            _savedOutput = string.Empty;
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
