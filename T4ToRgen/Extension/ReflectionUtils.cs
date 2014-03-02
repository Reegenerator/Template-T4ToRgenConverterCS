using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using EnvDTE80;

namespace T4ToRgen.Extension
{
    public class TypeCacheList
    {
        readonly Dictionary<string, TypeCache> _byNameCache = new Dictionary<string, TypeCache>();
        readonly Dictionary<Type, TypeCache> _byTypeCache = new Dictionary<Type, TypeCache>();
        public bool Contains(Type type)
        {
            return _byTypeCache.ContainsKey(type);
        }
        public TypeCache ByName(CodeClass2 cc)
        {
            if (!_byNameCache.ContainsKey(cc.FullName))
            {
                var tc = new TypeCache(cc.FullName);
                _byNameCache.Add(cc.FullName, tc);
                _byTypeCache.Add(tc.TypeInfo.AsType(), tc);
            }
            return _byNameCache[cc.FullName];
        }
        public TypeCache ByType(Type type)
        {
            if (!_byTypeCache.ContainsKey(type))
            {
                var tc = new TypeCache(type);
                _byNameCache.Add(tc.TypeInfo.Name, tc);
                _byTypeCache.Add(tc.TypeInfo.AsType(), tc);
            }
            return _byTypeCache[type];
        }
    }
    public class TypeCache
    {
        readonly Dictionary<string, MemberInfo> _cache;
        public TypeInfo TypeInfo { get; set; }
        // ReSharper disable once UnusedParameter.Local
        public TypeCache(string typeName, bool caseSensitiveMembers = false) : this(Type.GetType(typeName)){}
        public TypeCache(Type type, bool caseSensitiveMembers = false)
        {
            var comparer = caseSensitiveMembers ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
            _cache = new Dictionary<string, MemberInfo>(comparer);
            try
            {
                TypeInfo = type.GetTypeInfo();
                foreach (var m in TypeInfo.GetMembers(BindingFlags.Instance | BindingFlags.Public))
                {
                    //Prevent error on multiple cctor
                    if (!_cache.ContainsKey(m.Name))
                    {
                        _cache.Add(m.Name, m);
                    }
                }
            }
            catch (Exception)
            {
                DebugExtensions.DebugHere();
            }
        }
        public MemberInfo this[string name]
        {
            get
            {
                if (!_cache.ContainsKey(name))
                {
                    throw (new Exception(string.Format("Member {0} not found in {1}", name, TypeInfo.Name)));
                }
                return _cache[name];
            }
        }
        public IEnumerable<MemberInfo> GetMembers()
        {
            return _cache.Values;
        }
        public MemberInfo GetMember(string name)
        {
            return _cache[name];
        }
        public bool Contains(string name)
        {
            return _cache.ContainsKey(name);
        }
        public MemberInfo TryGetMember(string key)
        {
            MemberInfo value;
            _cache.TryGetValue(key, out value);
            return value;
        }
        public void AddAlias(string name, string alternateName)
        {
            try
            {
                _cache.Add(alternateName, this[name]);
            }
            catch (Exception)
            {
                Debugger.Launch();
            }
        }
    }
    public class TypeResolver
    {
        public static TypeCacheList TypeCacheList = new TypeCacheList();
        public static TypeCache ByType(Type type)
        {
            return TypeCacheList.ByType(type);
        }
        public static bool Contains(Type type)
        {
            return TypeCacheList.Contains(type);
        }
        public static TypeCache ByName(CodeClass2 cc)
        {
            return TypeCacheList.ByName(cc);
        }
        //Shared DteServiceProvider As IServiceProvider
        //Overloads Shared Function GetService(Of T)(dte As EnvDTE.DTE) As T
        //    If DteServiceProvider Is Nothing Then DteServiceProvider = New ServiceProvider(CType(dte, Microsoft.VisualStudio.OLE.Interop.IServiceProvider))
        //    Return CType(ServiceProvider.GlobalProvider.GetService(GetType(T)), T)
        //End Function
        // ''' <summary>
        // '''
        // ''' </summary>
        // ''' <returns></returns>
        // ''' <remarks>http://blogs.clariusconsulting.net/kzu/how-to-get-a-system-type-from-an-envdte-codetyperef-or-envdte-codeclass/</remarks>
        //Private Shared Function GetResolutionService(project As EnvDTE.Project) As ITypeResolutionService
        //    ''If DteServiceProvider Is Nothing Then DteServiceProvider =
        //    ''New ServiceProvider(CType(project.DTE, Microsoft.VisualStudio.OLE.Interop.IServiceProvider))
        //    ''Debugger.Launch()
        //    'Dim typeService As Object = ServiceProvider.GlobalProvider.GetService(GetType(DynamicTypeService)) ' CType(DteServiceProvider.GetService(GetType(DynamicTypeService)), DynamicTypeService)
        //    'Const vsDesign As String = "Microsoft.VisualStudio.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
        //    'Dim assm = Assembly.Load(vsDesign)
        //    'Dim dtsType = assm.GetType("Microsoft.VisualStudio.Design.VSDynamicTypeService")
        //    ''Dim constructor = dtsType.GetConstructor(BindingFlags.Instance Or BindingFlags.NonPublic, Nothing, {GetType(IServiceProvider)}, Nothing)
        //    ''Dim dts = constructor.Invoke({ServiceProvider.GlobalProvider})
        //    ''Dim methods = dtsType.GetMethods()
        //    ''Dim GetTypeResolutionServiceMethod = dtsType.GetMethod("GetTypeResolutionService", BindingFlags.Public Or BindingFlags.Instance)
        //    ''Dim x = GetTypeResolutionServiceMethod.Invoke(typeService, Nothing)
        //    'Dim dts2 = Activator.CreateInstance(dtsType, BindingFlags.Instance Or BindingFlags.NonPublic, Nothing, {ServiceProvider.GlobalProvider}, Nothing)
        //    ''Dim dts As Object = Activator.CreateInstanceFrom(, )
        //    ''GetService(Of DynamicTypeService)(project.DTE)
        //    'Dim sln As IVsSolution = GetService(Of IVsSolution)(project.DTE)
        //    'Dim hier As IVsHierarchy = Nothing
        //    'sln.GetProjectOfUniqueName(project.UniqueName, hier)
        //    'Debug.Assert(hier IsNot Nothing, "No active hierarchy is selected.")
        //    'Dim memberInfo = typeService.GetType.GetMethod("GetTypeResolutionService")
        //    'Return CType(typeService, DynamicTypeService).GetTypeResolutionService(hier)
        //End Function
        //Public Shared Function ResolveType(cc As CodeClass2) As Type
        //    Dim resolver = GetResolutionService(cc.ProjectItem.ContainingProject)
        //    Return resolver.GetType(cc.FullName, True)
        //End Function
    }
}
