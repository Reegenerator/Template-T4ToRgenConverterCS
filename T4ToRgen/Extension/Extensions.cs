using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using EnvDTE;
using EnvDTE80;
using Expression = System.Linq.Expressions.Expression;
using ProjectItem = Kodeo.Reegenerator.Wrappers.ProjectItem;
using Solution = Kodeo.Reegenerator.Wrappers.Solution;

namespace T4ToRgen.Extension
{
    internal static class Extensions
    {
        public const RegexOptions DefaultRegexOption = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline;
        private static readonly ConcurrentDictionary<CodeClass, Type> ToPropertyInfoClassCache = new ConcurrentDictionary<CodeClass, Type>();

        
        public static IEnumerable<CodeClass2> GetClassesEx(this ProjectItem item)
        {
            var classes = item.GetClasses().Values.SelectMany(x => x).Cast<CodeClass2>();
            return classes;
        }
        
        public static IEnumerable<CodeClass2> GetClassesWithAttributes(this ProjectItem item, Type[] attributes)
        {
            var fullNames = attributes.Select(x => x.DottedFullName()).ToArray();
            var res = item.GetClassesEx().
                        Where(cclass => fullNames.All(attrName => cclass.Attributes.Cast<CodeAttribute>().
                        Any(cAttr => cAttr.FullName == attrName))
                );
            return res;
        }
        /// <summary>
        /// Returns full name delimited by only dots (and no +(plus sign))
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        /// <remarks>Nested class is separated with +, while CodeClass delimit them using dots</remarks>
        
        public static string DottedFullName(this Type x)
        {
            return x.FullName.Replace("+", ".");
        }
        
        public static IEnumerable<CodeClass2> GetClassesWithAttribute(this ProjectItem item, Type attribute)
        {
            var fullName = attribute.DottedFullName();
            var res = item.GetClassesEx().Where(cclass => cclass.Attributes.Cast<CodeAttribute>().Any(x => x.FullName == fullName)
                );
            return res;
        }
        
        public static IEnumerable<CodeClass2> GetClassesWithAttribute(this DTE dte, Type attribute)
        {
            var projects = Solution.GetSolutionProjects(dte.Solution).Values;
            var res = from p in projects
                    from eleList in p.GetCodeElements<CodeClass2>().Values
                    from ele in eleList
                    where ele.Attributes.Cast<CodeAttribute>().Any(x => x.AsElement().IsEqual(attribute))
                    select ele;
            return res;
        }
        /// <summary>
        /// Use this to convert Code element into a more generic CodeElement and get CodeElement based extensions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cc"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        
        public static CodeElement2 AsElement<T>(this T cc)
        {
            return ((CodeElement2) cc);
        }
        
        public static CodeType AsCode<T>(this T cc)
        {
            return ((CodeType) cc);
        }
        
        public static bool HasAttribute(this CodeType ct, Type attrType)
        {
            return ct.GetCustomAttribute(attrType) != null;
        }

        
        public static CodeAttribute2 GetCustomAttribute(this CodeElement2 cc, Type attrType)
        {
            var res = cc.GetCustomAttributes().FirstOrDefault(x => x.AsElement().IsEqual(attrType));
            return res;
        }
        /// <summary>
        /// Get Custom Attributes
        /// </summary>
        /// <param name="ce"></param>
        /// <returns></returns>
        /// <remarks>
        /// Requires Named Argument when declaring the Custom Attribute, otherwise Name will be empty.
        /// Not using reflection because it requires successful build
        /// </remarks>
        
        public static IEnumerable<CodeAttribute2> GetCustomAttributes(this CodeElement2 ce)
        {
            var prop = ce as CodeProperty2;
            if (prop != null)
            {
                return prop.Attributes.Cast<CodeAttribute2>();
            }
            var func = ce as CodeFunction2;
            if (func != null)
            {
                return func.Attributes.Cast<CodeAttribute2>();
            }
            var cc = ce as CodeClass2;
            if (cc != null)
            {
                return cc.Attributes.Cast<CodeAttribute2>();
            }
            throw (new Exception("CodeElement not recognized"));
            //return Enumerable.Empty<CodeAttribute2>();
        }
        
        public static bool HasAttribute(this CodeElement2 ct, Type attrType)
        {
            return ct.GetCustomAttribute(attrType) != null;
        }


        
        public static IEnumerable<CustomAttributeData> GetCustomAttributes(this CodeType ct)
        {
            var type = Type.GetType(ct.FullName);
            return type != null ? type.CustomAttributes : null;
        }

        public static IEnumerable<CustomAttributeData> GetCustomAttributes(this CodeClass cc)
        {
            var type = Type.GetType(cc.FullName);
            return type != null ? type.CustomAttributes : null;
        }

        public static CustomAttributeData GetCustomAttribute(this CodeType ct, Type attrType)
        {
            return ct.GetCustomAttributes().FirstOrDefault(x => x.AttributeType == attrType);
        }
        
        public static CustomAttributeData GetCustomAttribute(this CodeClass cc, Type attrType)
        {
            return cc.GetCustomAttributes().FirstOrDefault(x => x.AttributeType == attrType);
        }
        
        public static IEnumerable<CodeAttributeArgument> GetCodeAttributeArguments(this CodeAttribute2 cattr)
        {
            if (cattr == null)
            {
                return Enumerable.Empty<CodeAttributeArgument>();
            }
            return cattr.Arguments.Cast<CodeAttributeArgument>();
        }
        private static readonly Type IsEqualAttrType = typeof(Attribute);
        public static bool IsEqual(this CodeElement2 ele, Type type)
        {
            return ele.FullName == type.FullName || ele.Name == type.Name ||
                (type.IsSubclassOf(IsEqualAttrType) &&
                (ele.FullName + "Attribute" == type.FullName || ele.Name + "Attribute" == type.Name)
                );
        }


        
        public static IEnumerable<CodeProperty2> GetProperties(this CodeClass cls)
        {
            return cls.Children.OfType<CodeProperty2>();
        }
        
        public static IEnumerable<CodeFunction2> GetFunctions(this CodeClass cls)
        {
            return cls.Children.OfType<CodeFunction2>();
        }
        
        public static CodeProperty2[] GetAutoProperties(this CodeClass2 cls)
        {
            var props = cls.GetProperties();
            return props.Where(x => x.ReadWrite == vsCMPropertyKind.vsCMPropertyKindReadWrite &&
                x.Setter == null &&
                x.OverrideKind != vsCMOverrideKind.vsCMOverrideKindAbstract).ToArray();
        }
        
        public static IEnumerable<CodeVariable> GetVariables(this CodeClass cls)
        {
            return cls.Children.OfType<CodeVariable>();
        }
        
        public static IEnumerable<CodeVariable> GetDependencyProperties(this CodeClass cls)
        {
            try
            {
                var sharedFields = cls.GetVariables().Where(x => x.IsShared && x.Type.CodeType != null);
                return sharedFields.Where(x => x.Type.CodeType.FullName == "System.Windows.DependencyProperty");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return default(IEnumerable<CodeVariable>);
        }
        /// <summary>
        /// Get Bases recursively
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        
        public static IEnumerable<CodeClass2> GetAncestorClasses(this CodeClass2 cc)
        {
            var bases = cc.Bases.Cast<CodeClass2>().ToArray();
            if (bases.FirstOrDefault() == null)
            {
                return bases;
            }
            var grandBases = bases.SelectMany(x => x.GetAncestorClasses());
            return bases.Concat(grandBases);
        }


        
        public static string GetText(this CodeProperty2 prop, vsCMPart part = vsCMPart.vsCMPartWholeWithAttributes)
        {
            var p = prop.GetStartPoint(part);
            if (p == null)
            {
                return string.Empty;
            }
            return p.CreateEditPoint().GetText(prop.GetEndPoint(part));
        }
        
        public static string GetText(this CodeClass2 cls, vsCMPart part = vsCMPart.vsCMPartWholeWithAttributes)
        {
            var p = cls.GetStartPoint(part);
            if (p == null)
            {
                return string.Empty;
            }
            return p.CreateEditPoint().GetText(cls.GetEndPoint(part));
        }
        private readonly static string InterfaceImplementationPattern = XElement.Parse("<String><![CDATA[                           " + "^.*?\\sAs\\s.*?(?<impl>Implements\\s.*?)$" + "]]></String>").Value;
        private static readonly Regex GetInterfaceImplementationRegex = new Regex(InterfaceImplementationPattern, DefaultRegexOption);
        public static string GetInterfaceImplementation(this CodeProperty2 prop)
        {
            var g = GetInterfaceImplementationRegex.Match(Convert.ToString(prop.GetText(vsCMPart.vsCMPartHeader))).Groups["impl"];
            if (g.Success)
            {
                return " " + g.Value;
            }
            return null;
        }
        
        public static TextPoint GetAttributeStartPoint(this CodeProperty2 prop)
        {
            // ReSharper disable once RedundantArgumentDefaultValue
            return prop.GetStartPoint(vsCMPart.vsCMPartWholeWithAttributes);
        }
        private static Regex _docCommentRegex;
        /// <summary>
        /// Lazy Regex property to match doc comments
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public static Regex CsDocCommentRegex
        {
            get
            {
                const string docCommentPattern = @"\s\\\";
                return _docCommentRegex ?? (_docCommentRegex = new Regex(docCommentPattern));
            }
        }
        
        public static EditPoint GetCommentStartPointCs(this CodeElement ce)
        {
            return ce.GetStartPoint(vsCMPart.vsCMPartHeaderWithAttributes).GetCommentStartPointCs();
        }
        
        public static EditPoint GetCommentStartPointCs(this CodeProperty2 ce)
        {
            return ce.GetStartPoint(vsCMPart.vsCMPartHeaderWithAttributes).GetCommentStartPointCs();
        }
        /// <summary>
        /// Get to the beginning of doc comments for startPoint
        /// </summary>
        /// <param name="startPoint"></param>
        /// <returns></returns>
        /// <remarks>
        /// EnvDte does not have a way to get to the starting point of a code element doc comment.
        /// If we need to insert some text before a code element that has doc comments we need to go to the beggining of the comments.
        /// </remarks>
        
        public static EditPoint
        GetCommentStartPointCs(this TextPoint startPoint)
        {
            var sp = startPoint.CreateEditPoint();
            do
            {
                sp.LineUp();
            }
            while (CsDocCommentRegex.IsMatch(Convert.ToString(sp.GetLineText())));
            sp.LineDown();
            sp.StartOfLine();
            return sp;
        }
        
        public static string ToStringFormatted(this XElement xml)
        {
            var settings = new XmlWriterSettings {OmitXmlDeclaration = true};
            var result = new StringBuilder();
            using (var writer = XmlWriter.Create(result, settings))
            {
                xml.WriteTo(writer);
            }
            return result.ToString();
        }

        public static string[] ExprsToString<T>(params Expression<Func<T, object>>[] exprs)
        {
            var strings = (from x in exprs
                           select ((LambdaExpression) x).ExprToString()).ToArray();
            return strings;
        }
        
        public static string ExprToString<T, T2>(this Expression<Func<T, T2>> expr)
        {
            return ((LambdaExpression) expr).ExprToString();
        }
        
        public static string ExprToString(this LambdaExpression memberExpr)
        {
            if (memberExpr == null)
            {
                return string.Empty;
            }
            var convertedToObject = memberExpr.Body as UnaryExpression;
            Expression currExpr = convertedToObject != null ? convertedToObject.Operand : memberExpr.Body;
            switch (currExpr.NodeType)
            {
                case ExpressionType.MemberAccess:
                    var ex = (MemberExpression) currExpr;
                    return ex.Member.Name;
            }
            throw (new Exception("Expression ToString() extension only processes MemberExpression"));
        }

        public static List<Type> FindAllDerivedTypes<T>()
        {
            return FindAllDerivedTypes<T>(Assembly.GetAssembly(typeof(T)));
        }
        public static List<Type> FindAllDerivedTypes<T>(Assembly assembly)
        {
            var derivedType = typeof(T);
            return assembly.GetTypes().Where(x => x != derivedType && derivedType.IsAssignableFrom(x)).ToList();
        }
        
        public static IEnumerable<CodeClass2> GetSubclasses(this CodeClass2 cc)
        {
            var fullname = cc.FullName;
            var list = new List<CodeClass2>();
            Kodeo.Reegenerator.Wrappers.CodeElement.TraverseSolutionForCodeElements<CodeClass2>(
                cc.DTE.Solution, list.Add, x => x.FullName != fullname && x.IsDerivedFrom[fullname]);
            return list.ToArray();
        }
        private static readonly Regex RemoveEmptyLinesRegex = new Regex("^\\s+$[\\r\\n]*", RegexOptions.Multiline);
        public static string RemoveEmptyLines(this string s)
        {
            return RemoveEmptyLinesRegex.Replace(s, string.Empty);
        }
        
        public static TResult SelectOrDefault<T, TResult>(this T obj, Func<T, TResult> selectFunc, TResult defaultValue = null) where T : class where TResult : class
        {
            if (obj == null)
            {
                return defaultValue;
            }
            return selectFunc(obj);
        }

        private static readonly Dictionary<string, Assembly> GetTypeFromProjectCache = new Dictionary<string, Assembly>();
        /// <summary>
        /// Returns a type from an assembly reference by ProjectItem.Project. Cached.
        /// </summary>
        /// <param name="pi"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static Type GetTypeFromProject(this EnvDTE.ProjectItem pi, string typeName)
        {
            var path = pi.GetAssemblyPath();
            if (!GetTypeFromProjectCache.ContainsKey(path))
            {
                GetTypeFromProjectCache.Add(path, Assembly.LoadFrom(Convert.ToString(path)));
            }
            var asm = GetTypeFromProjectCache[path];
            return asm.GetType(typeName);
        }
        
        public static Type ToType(this CodeClass cc)
        {
            return cc.ProjectItem.GetTypeFromProject(cc.FullName);
        }


        /// <summary>
        /// Convert CodeProperty2 to PropertyInfo. Cached
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static PropertyInfo ToPropertyInfo(this CodeProperty2 prop)
        {
            var classType = ToPropertyInfoClassCache.GetOrAdd(prop.Parent, x => prop.Parent.ToType());
            return classType.GetProperty(prop.Name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
        
        public static Assembly GetAssemblyOfProjectItem(this EnvDTE.ProjectItem pi)
        {
            var path = Convert.ToString(pi.GetAssemblyPath());
            return path != string.Empty ? Assembly.LoadFrom(path) : null;
        }
        
        public static string GetAssemblyPath(this EnvDTE.ProjectItem pi)
        {
            pi.ContainingProject.Properties.Cast<Property>().FirstOrDefault(x => x.Name == "AssemblyName").SelectOrDefault(x => x.Value);
            return pi.ContainingProject.GetAssemblyPath();
        }
        /// <summary>
        /// Currently unused. If we require a succesful build, a project that requires succesful generation would never build, catch-22
        /// </summary>
        /// <param name="vsProject"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        
        public static string GetAssemblyPath(this Project vsProject)
        {
            var fullPath = Convert.ToString(vsProject.Properties.Item("FullPath").Value.ToString());
            var outputPath = Convert.ToString(vsProject.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString());
            var outputDir = Path.Combine(fullPath, outputPath);
            var outputFileName = Convert.ToString(vsProject.Properties.Item("OutputFileName").Value.ToString());
            var assemblyPath = Path.Combine(outputDir, outputFileName);
            return assemblyPath;
        }
        private readonly static Regex TypeWithoutQualifierRegex = new Regex(
        XElement.Parse("<String><![CDATA[    " + "(?<=\\.?)[^\\.]+?$" + "]]></String>").Value, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        
        public static string StripQualifier(this string s)
        {
            var stripped = TypeWithoutQualifierRegex.Match(s).Value;
            return stripped;
        }
        
        public static T ParseAsEnum<T>(this string qualifiedName, T defaultValue)
            where T: struct
        {
            if (qualifiedName == null)
            {
                return defaultValue;
            }
            T res;
            Enum.TryParse(Convert.ToString(qualifiedName.StripQualifier()), out res);
            return res;
        }
        public static T GetOrInit<T>( ref T x, Func<T> initFunc) where T:class 
        {
            return x ?? (x = initFunc());
        }

        /// <summary>
        /// Create type instance from string
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        
        public static dynamic ConvertFromString(this Type type, string value)
        {
            return TypeDescriptor.GetConverter(type).ConvertFromString(value);
        }
        /// <summary>
        /// Set property value from string representation
        /// </summary>
        /// <param name="propInfo"></param>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        /// <remarks></remarks>
        
        public static void SetValueFromString(this PropertyInfo propInfo, object obj, string value)
        {
            var setValue = propInfo.PropertyType == typeof(Version) ? 
                        Version.Parse(value) : 
                        propInfo.PropertyType.ConvertFromString(value);
            propInfo.SetValue(obj, setValue);
        }
        
        public static void AddInterfaceIfNotExists(this CodeClass2 cls, string interfaceName)
        {
            if (cls.ImplementedInterfaces.OfType<CodeInterface>().All(x => x.FullName != interfaceName))
            {
                cls.AddImplementedInterface(interfaceName, null);
            }
        }
        
        public static string DotJoin(this string s, params string[] segments)
        {
            var all = new [] { s }.Concat(segments).ToArray();
            return string.Join(".", all);
        }
    }
}
