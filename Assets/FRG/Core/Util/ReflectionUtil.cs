
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace FRG.Core
{
    /// <summary>
    /// Type and assembly utilities.
    /// </summary>
    public static class ReflectionUtil
    {
        public const BindingFlags InclusiveBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        #region CSharp Name Resolution
        /// <summary>
        /// Gets the non-namespace-qualified name for a type or member, handling generics. Does includes any parent classes (which means always for non-types).
        /// Non-types will always include a type name.
        /// </summary>
        public static string CSharpName(this MemberInfo memberInfo)
        {
            return GetCSharpNameCached(memberInfo, false, ArrayUtil.Empty<string>());
        }

        /// <summary>
        /// Gets the namespace-qualified name for a type or member, handling generics and parent classes appropriately.
        /// </summary>
        public static string CSharpFullName(this MemberInfo memberInfo)
        {
            return GetCSharpNameCached(memberInfo, true, ArrayUtil.Empty<string>());
        }

        /// <summary>
        /// Gets the namespace-qualified name for a type or member, handling generics and parent classes appropriately.
        /// </summary>
        /// <param name="fullNamespacesToExclude">The fully-qualified namespaces or types to exclude from the name.</param>
        public static string CSharpContextualName(MemberInfo memberInfo, ICollection<string> fullNamespacesToExclude)
        {
            return GetCSharpNameCached(memberInfo, true, fullNamespacesToExclude ?? ArrayUtil.Empty<string>());
        }

        private static string GetCSharpNameCached(MemberInfo memberInfo, bool includeNamespace, ICollection<string> fullNamespacesToExclude)
        {
            // Null check because these are often in warning messages
            if (memberInfo == null)
            {
                return "<null>";
            }

            // Do not cache with namespaces (typically just used in editor anyway)
            if (fullNamespacesToExclude.Count != 0)
            {
                string shortName = CSharpName(memberInfo);
                string fullName = CSharpFullName(memberInfo);
                if (string.Equals(shortName, fullName, StringComparison.Ordinal)) {
                    return fullName;
                }

                Type type = FindSingleDependentType(memberInfo);
                if (type != null && type.DeclaringType == null) {
                    includeNamespace = !fullNamespacesToExclude.Contains(type.Namespace);
                    fullNamespacesToExclude = ArrayUtil.Empty<string>();

                    // Fall through
                }
            }

            if (fullNamespacesToExclude.Count != 0) {
                return BuildCSharpNameForAny(memberInfo, includeNamespace, fullNamespacesToExclude);
            }

            lock (TypeNameStatics.SyncRoot)
            {
                Dictionary<MemberInfo, string> cache = includeNamespace ? CSharpNameStatics.MemberToFullNameLookup : CSharpNameStatics.MemberToNameLookup;
                string cachedName;
                if (cache.TryGetValue(memberInfo, out cachedName))
                {
                    // Cached; early out
                    return cachedName;
                }

                // Don't tie up lock; OK to compute multiple times
            }

            string name = BuildCSharpNameForAny(memberInfo, includeNamespace, ArrayUtil.Empty<string>());

            lock (TypeNameStatics.SyncRoot)
            {
                Dictionary<MemberInfo, string> cache = includeNamespace ? CSharpNameStatics.MemberToFullNameLookup : CSharpNameStatics.MemberToNameLookup;
                // Don't use add because it can happen multiple times
                cache[memberInfo] = string.Intern(name);
            }

            return name;
        }

        private static Type FindSingleDependentType(MemberInfo member)
        {
            Type type;
            switch (member.MemberType) {
                case MemberTypes.TypeInfo:
                case MemberTypes.NestedType:
                    type = (Type)member;
                    break;

                case MemberTypes.Method:
                    MethodBase method = (MethodBase)member;
                    bool isGenericMethod = method.IsGenericMethod || method.IsGenericMethodDefinition;
                    if (isGenericMethod) {
                        // Early exit
                        return null;
                    }

                    type = member.DeclaringType;
                    break;

                default:
                // Debug.Assert(false);
                // break;
                case MemberTypes.Event:
                case MemberTypes.Field:
                case MemberTypes.Property:
                // Should be raw name, even for generics and specials
                case MemberTypes.Constructor:
                    type = member.DeclaringType;
                    break;
            }

            while (type.IsByRef) {
                type = type.GetElementType();
            }
            while (type.IsArray) {
                type = type.GetElementType();
            }
            while (type.IsPointer) {
                type = type.GetElementType();
            }
            while (Nullable.GetUnderlyingType(type) != null) {
                type = Nullable.GetUnderlyingType(type);
            }

            if (type.IsGenericType || type.IsGenericTypeDefinition) {
                return null;
            }

            return type;
        }

        private static string BuildCSharpNameForAny(MemberInfo member, bool includeNamespace, ICollection<string> fullNamespacesToExclude)
        {
            switch (member.MemberType)
            {
                case MemberTypes.TypeInfo:
                case MemberTypes.NestedType:
                    // Need to do full monty because of inner generics
                    using (Pooled<StringWriter> pooled = RecyclingPool.SpawnStringWriter())
                    {
                        StringWriter writer = pooled.Value;
                        BuildComplexNameForType(writer, (Type)member, includeNamespace, fullNamespacesToExclude);
                        return writer.ToString();
                    }

                case MemberTypes.Method:
                    // Concatentate generic method name and type
                    using (Pooled<StringWriter> pooled = RecyclingPool.SpawnStringWriter())
                    {
                        StringWriter writer = pooled.Value;
                        MethodBase method = (MethodBase)member;

                        bool isGenericMethod = method.IsGenericMethod || method.IsGenericMethodDefinition;
                        bool isGenericType = method.DeclaringType.IsGenericType || method.DeclaringType.IsGenericTypeDefinition;
                        Type[] genericArguments = isGenericMethod ? method.GetGenericArguments() : isGenericType ? method.DeclaringType.GetGenericArguments() : null;
                        BuildNormalName(writer, method, isGenericMethod, genericArguments, includeNamespace, fullNamespacesToExclude);
                        return writer.ToString();
                    }

                default:
                // Debug.Assert(false);
                // break;
                case MemberTypes.Event:
                case MemberTypes.Field:
                case MemberTypes.Property:
                // Should be raw name, even for generics and specials
                case MemberTypes.Constructor:
                    // Concatenate optional declaring type with name
                    string declaringType = GetCSharpNameCached(member.DeclaringType, includeNamespace, fullNamespacesToExclude);
                    string memberName = member.Name;
                    return declaringType + "." + memberName;
            }
        }

        /// <summary>
        /// Does the nitty gritty stuff.
        /// Will not report errors, nor escape invalid identifiers.
        /// </summary>
        /// <remarks>
        /// Made with reference to 
        /// <see href="https://github.com/dotnet/roslyn/blob/master/src/ExpressionEvaluator/Core/Source/ResultProvider/Formatter.TypeNames.cs">open source Roslyn code</see>.
        /// Licensed under the Apache License, Version 2.0.
        /// </remarks>
        private static void BuildComplexNameForType(TextWriter writer, Type originalType, bool includeNamespace, ICollection<string> fullNamespacesToExclude)
        {
            Type currentType = originalType;

            int referenceCount = 0;
            while (currentType.IsByRef)
            {
                // Shouldn't ever be false, but maybe in some weird Mono contexts.
                Type nextType = currentType.GetElementType();
                if (nextType == null)
                {
                    break;
                }
                currentType = nextType;
                referenceCount += 1;
            }

            while (currentType.IsArray)
            {
                // Shouldn't ever be false, but maybe in some weird Mono contexts.
                Type nextType = currentType.GetElementType();
                if (nextType == null)
                {
                    break;
                }
                currentType = nextType;
            }

            int pointerCount = 0;
            while (currentType.IsPointer)
            {
                Type nextType = currentType.GetElementType();
                if (nextType == null)
                {
                    break;
                }
                currentType = nextType;
                pointerCount += 1;
            }

            int nullableCount = 0;
            while (true)
            {
                Type nextType = Nullable.GetUnderlyingType(currentType);
                // Very unlikely to ever be possible.
                if (nextType == null)
                {
                    break;
                }

                currentType = nextType;
                nullableCount += 1;
            }

            BuildConcreteTypeName(writer, currentType, includeNamespace, fullNamespacesToExclude);

            for (int i = 0; i < nullableCount; ++i)
            {
                writer.Write('?');
            }
            for (int i = 0; i < pointerCount; ++i)
            {
                writer.Write('*');
            }

            for (Type arrayType = originalType; arrayType != null && arrayType.IsArray; arrayType = arrayType.GetElementType())
            {
                writer.Write('[');
                int commaCount = arrayType.GetArrayRank() - 1;
                for (int i = 0; i < commaCount; ++i)
                {
                    writer.Write(',');
                }
                writer.Write(']');
            }

            for (int i = 0; i < referenceCount; ++i)
            {
                writer.Write('&');
            }
        }

        private static void BuildConcreteTypeName(TextWriter writer, Type concreteType, bool includeNamespace, ICollection<string> fullNamespacesToExclude)
        {
            string specialName = GetSpecialTypeName(concreteType);
            if (!string.IsNullOrEmpty(specialName))
            {
                writer.Write(specialName);
                return;
            }

            if (concreteType.IsGenericParameter)
            {
                writer.Write(concreteType.Name);
                return;
            }

            if (includeNamespace && !string.IsNullOrEmpty(concreteType.Namespace) && (fullNamespacesToExclude == null || !fullNamespacesToExclude.Contains(concreteType.Namespace)))
            {
                writer.Write(concreteType.Namespace);
                writer.Write('.');
            }

            bool isGeneric = concreteType.IsGenericType || concreteType.IsGenericTypeDefinition;
            Type[] genericArguments = isGeneric ? concreteType.GetGenericArguments() : null;
            BuildNormalName(writer, concreteType, isGeneric, genericArguments, includeNamespace, fullNamespacesToExclude);
        }

        private static void BuildNormalName(TextWriter writer, MemberInfo member, bool isGeneric, Type[] genericArguments, bool includeNamespace, ICollection<string> fullNamespacesToExclude)
        {
            genericArguments = genericArguments ?? ArrayUtil.Empty<Type>();
            isGeneric = (isGeneric && genericArguments.Length > 0);

            MemberInfo nestingDepthSearchMember = member;
            int nestingDepth = 0;
            while (nestingDepthSearchMember.DeclaringType != null)
            {
                nestingDepth += 1;
                nestingDepthSearchMember = nestingDepthSearchMember.DeclaringType;
            }

            int genericArgumentIndex = 0;

            for (int i = nestingDepth /* [sic] */; i >= 0; --i)
            {
                MemberInfo component = member;
                for (int j = 0; j < i; ++j)
                {
                    component = component.DeclaringType;
                }
                //Debug.Assert(componentType != null, "Searched through declaring types twice; got different results.");

                int genericArgumentCount = 0;
                if (isGeneric)
                {
                    // May be unspecialized here, but specialized in concreteType.
                    Type[] componentGenericArguments = (component == member) ? genericArguments : (((Type)component).GetGenericArguments() ?? ArrayUtil.Empty<Type>());
                    if (componentGenericArguments.Length > 0)
                    {
                        genericArgumentCount = componentGenericArguments.Length - genericArgumentIndex;
                        //Debug.Assert(genericArgumentCount >= 0, "More generic arguments than expected.");
                    }
                    //Debug.Assert(genericArgumentIndex + genericArgumentCount <= genericArguments.Length, "Too many generic arguments.");
                }

                BuildTypeNameComponent(
                    writer, component,
                    genericArguments, genericArgumentIndex, genericArgumentCount,
                    includeNamespace, fullNamespacesToExclude);
                genericArgumentIndex += genericArgumentCount;

                if (i != 0) writer.Write('.');
            }
        }

        private static string GetSpecialTypeName(Type specialType)
        {
            // Can otherwise return code of underlying type
            if (specialType.IsEnum)
            {
                return null;
            }

            switch (Type.GetTypeCode(specialType))
            {
                case TypeCode.Boolean:
                    return "bool";
                case TypeCode.Char:
                    return "char";
                case TypeCode.SByte:
                    return "sbyte";
                case TypeCode.Byte:
                    return "byte";
                case TypeCode.Int16:
                    return "short";
                case TypeCode.UInt16:
                    return "ushort";
                case TypeCode.Int32:
                    return "int";
                case TypeCode.UInt32:
                    return "uint";
                case TypeCode.Int64:
                    return "long";
                case TypeCode.UInt64:
                    return "ulong";
                case TypeCode.Single:
                    return "float";
                case TypeCode.Double:
                    return "double";
                case TypeCode.Decimal:
                    return "decimal";
                case TypeCode.String:
                    return "string";
            }

            if (specialType == typeof(object)) return "object";
            if (specialType == typeof(void)) return "void";

            return null;
        }

        private static void BuildTypeNameComponent(
            TextWriter writer, MemberInfo component,
            Type[] genericArguments, int genericArgumentIndex, int genericArgumentCount,
            bool includeNamespace, ICollection<string> fullNamespacesToExclude)
        {
            if (genericArgumentCount == 0)
            {
                writer.Write(component.Name);
            }
            else
            {
                string baseName = component.Name;
                int backtickIndex = baseName.IndexOf('`');
                if (backtickIndex < 0)
                {
                    backtickIndex = baseName.Length;
                }
                writer.Write(baseName.Substring(0, backtickIndex));

                writer.Write('<');
                for (int i = 0; i < genericArgumentCount; ++i)
                {
                    if (i != 0) writer.Write(", ");

                    Type genericType = genericArguments[genericArgumentIndex + i];
                    if (fullNamespacesToExclude.Count != 0)
                    {
                        BuildComplexNameForType(writer, genericType, includeNamespace, fullNamespacesToExclude);
                    }
                    else
                    {
                        writer.Write(GetCSharpNameCached(genericType, includeNamespace, fullNamespacesToExclude));
                    }
                }
                writer.Write('>');
            }
        }
        #endregion

        #region Inspector Naming Utilities
        /// <summary>
        /// Converts a member name like Unity does to an easier to read name in the inspector.
        /// </summary>
        public static string GetInspectorDisplayName(string memberName, bool allowSpaces = true)
        {
            //string displayName = UnityEditor.ObjectNames.NicifyVariableName(memberName);
            //if (!allowSpaces)
            //{
            //    displayName = displayName.Replace(" ", "");
            //}
            //return displayName;

            if (string.IsNullOrEmpty(memberName)) {
                return "";
            }

            int index = 0;
            switch (memberName[0]) {
            case 'm':
                if (memberName.Length > 1 && memberName[1] == '_') {
                    index += 2;
                }
                break;
            case 'k':
                if (memberName.Length > 1 && memberName[1] >= 'A' && memberName[1] <= 'Z') {
                    index += 1;
                }
                break;
            case '_':
                index += 1;
                break;
            }

            if (index >= memberName.Length) {
                return "";
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(char.ToUpperInvariant(memberName[index]));

            if (allowSpaces) {

                bool lastUpper = true;
                for (index = index + 1; index < memberName.Length; ++index) {
                    char c = memberName[index];
                    if (c >= 'A' && c <= 'Z' || c >= '0' && c <= '9') {
                        if (!lastUpper) {
                            sb.Append(' ');
                        }
                        lastUpper = true;
                    }
                    else {
                        lastUpper = !(c >= 'a' && c <= 'z');
                    }
                    sb.Append(c);
                }
            }
            else {
                sb.Append(memberName, index + 1, memberName.Length - (index + 1));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Pluralizes a noun according to english rules.
        /// </summary>
        /// <remarks>
        /// Done roughly according to wikipedia's rules http://en.wikipedia.org/wiki/English_plurals but
        /// without trying to guess sounds.
        /// </remarks>
        public static string Pluralize(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return word;
            }

            // add more as needed (make sure casing is correct)
            if (word.EndsWith("Die", StringComparison.Ordinal))
            {
                return word.Substring(0, word.Length - "Die".Length) + "Dice";
            }

            char firstLetter = word[word.Length - 1];
            if (firstLetter >= 'A' && firstLetter <= 'Z' || firstLetter >= '0' && firstLetter <= '9')
            {
                return word + "s";
            }

            if (firstLetter < 'a' || firstLetter > 'z')
            {
                return word;
            }

            switch (firstLetter)
            {
                case 's':
                case 'z':
                    return word + "es";
                case 'h':
                    if (word.Length > 1)
                    {
                        char secondLetter = word[word.Length - 2];
                        if (secondLetter < 'a' || secondLetter > 'z')
                        {
                            return word + "s";
                        }

                        switch (secondLetter)
                        {
                            case 't':
                                return word + "s";
                            default:
                                return word + "es";
                        }
                    }
                    else
                    {
                        return "hs";
                    }
                case 'y':
                    if (word.Length >= 2)
                    {
                        char secondLetter = word[word.Length - 2];
                        if (secondLetter < 'a' || secondLetter > 'z')
                        {
                            return word + "s";
                        }

                        switch (secondLetter)
                        {
                            case 'a':
                            case 'e':
                            case 'i':
                            case 'o':
                                return word + "s";
                            case 'u':
                                if (word.Length < 3 || char.ToLowerInvariant(word[word.Length - 3]) != 'q')
                                {
                                    return word + "s";
                                }
                                break;
                        }
                        return word.Substring(0, word.Length - 1) + "ies";
                    }
                    else
                    {
                        return "ys";
                    }
                case 'o':
                    if (word.Length > 1)
                    {
                        char secondLetter = word[word.Length - 2];
                        if (secondLetter < 'a' || secondLetter > 'z')
                        {
                            return word + "s";
                        }
                        return word + "es";
                    }
                    else
                    {
                        return "os";
                    }
                default:
                    return word + "s";
            }
        }
        #endregion

        #region Cached Type Lookup
        /// <summary>
        /// Gets a specified type with the given name. Should contain namespace.
        /// </summary>
        /// <remarks>
        /// Result may be stale if assemblies change.
        /// </remarks>
        public static Type GetType(string fullyQualifiedTypeName)
        {
            Type type = GetTypeCached(fullyQualifiedTypeName);
            if (type == null && fullyQualifiedTypeName.IndexOf('.') < 0)
            {
                fullyQualifiedTypeName = "UnityEngine." + fullyQualifiedTypeName;
                type = GetTypeCached(fullyQualifiedTypeName);
            }
            return type;
        }

        private static Type GetTypeCached(string fullyQualifiedTypeName)
        {
            lock (TypeNameStatics.SyncRoot)
            {
                Type cacheType;
                if (TypeNameStatics.NameToTypeLookup.TryGetValue(fullyQualifiedTypeName, out cacheType))
                {
                    // Cached; early out
                    return cacheType;
                }

                // Don't tie up lock; OK to compute multiple times
            }

            Type type = GetTypeExhaustive(fullyQualifiedTypeName, false);

            lock (TypeNameStatics.SyncRoot)
            {
                // Don't use add because it can happen multiple times
                TypeNameStatics.NameToTypeLookup[fullyQualifiedTypeName] = type;
            }
            return type;
        }

        /// <summary>
        /// Gets a specified type with the given name. Should contain namespace.
        /// </summary>
        /// <remarks>
        /// Result may be stale if assemblies change.
        /// </remarks>
        public static Type GetType(string fullyQualifiedTypeName, bool allowEditor)
        {
            Type type = GetType(fullyQualifiedTypeName);
            if (type == null && allowEditor)
            {
                type = GetTypeExhaustive(fullyQualifiedTypeName, true);
                if (type == null && fullyQualifiedTypeName.IndexOf('.') < 0)
                {
                    fullyQualifiedTypeName = "UnityEditor." + fullyQualifiedTypeName;
                    type = GetTypeExhaustive(fullyQualifiedTypeName, true);
                }
            }
            return type;
        }

        /// <summary>
        /// Gets a specified type with the given name. Should contain namespace.
        /// Expensive on the server.
        /// </summary>
        private static Type GetTypeExhaustive(string fullyQualifiedTypeName, bool allowEditor)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type found = assembly.GetType(fullyQualifiedTypeName, false, true);
                if (found != null)
                {
                    if (allowEditor || !IsEditorAssembly(found.Assembly))
                    {
                        return found;
                    }
                }
            }
            return null;
        }
        #endregion

        #region Assembly Tools
        /// <summary>
        /// Loads an assembly by name if it exists, otherwise returns null.
        /// </summary>
        public static Assembly GetAssembly(string assemblyName)
        {
            Assembly assembly;
            try
            {
                assembly = Assembly.Load(assemblyName);
            }
            catch (Exception e)
            {
                if (!IsIOException(e)) { throw; }

                return null;
            }

            return assembly;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Returns true if the assembly was recently modified.
        /// </summary>
        public static bool IsAssemblyRecentlyCompiled(string assemblyName)
        {
            Assembly assembly = GetAssembly(assemblyName);
            if (assembly == null)
            {
                return false;
            }

            // will never be true when this file exists in a dll
            DateTime writeTime = File.GetLastWriteTimeUtc(assembly.Location);
            DateTime now = DateTime.UtcNow;
            TimeSpan timeSinceCompile = now - writeTime;
            return (timeSinceCompile >= -TimeSpan.FromSeconds(30) && timeSinceCompile < TimeSpan.FromSeconds(30));
        }
#endif

        public static bool IsEditorAssembly(Assembly assembly)
        {
            if (Attribute.IsDefined(assembly, typeof(AssemblyIsEditorAssembly)))
            {
                return true;
            }

            if (assembly.FullName.Contains("-Editor"))
            {
                return true;
            }
            return false;
        }

        public static bool IsBuiltInAssembly(Assembly assembly)
        {
            string name = (assembly.FullName ?? "");
            var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
            return compareInfo.IsPrefix(name, "UnityEngine") ||
                compareInfo.IsPrefix(name, "UnityEditor") ||
                compareInfo.IsPrefix(name, "System");
        }

        public static Assembly[] GetAllAssemblies()
        {
            return AssemblyStatics.AllAssemblies;
        }
        public static Assembly[] GetRuntimeAssemblies()
        {
            return AssemblyStatics.RuntimeAssemblies;
        }
        public static Assembly[] GetEditorAssemblies()
        {
            return AssemblyStatics.EditorAssemblies;
        }

        public static Type[] GetRuntimeTypes()
        {
            return RuntimeTypeStatics.RuntimeTypes;
        }
        public static Type[] GetEditorTypes()
        {
            return EditorTypeStatics.EditorTypes;
        }

        /// <summary>
        /// Gets all types that are derived from or implement baseType.
        /// </summary>
        public static Type[] GetRecursivelyDerivedTypes(Type baseType)
        {
            Type[] assignableTypes;
            lock (TypeCacheStatics.SyncRoot) {
                if (TypeCacheStatics.AssignableCache.TryGetValue(baseType, out assignableTypes)) {
                    return assignableTypes;
                }
            }

            List<Type> types = new List<Type>();
            foreach (Type type in GetRuntimeTypes()) {
                if (baseType.IsAssignableFrom(type) && type != baseType) {
                    types.Add(type);
                }
            }
            assignableTypes = types.ToArray();

            lock (TypeCacheStatics.SyncRoot) {
                TypeCacheStatics.AssignableCache[baseType] = assignableTypes;
            }

            return assignableTypes;
        }
        #endregion

        #region Type Analysis
        /// <summary>
        /// Determines if the given type is Nullable.
        /// </summary>
        public static bool IsNullable(Type nullableType)
        {
            return (nullableType.IsGenericType && nullableType.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        /// <summary>
        /// Determines if this is a Unity serializable "List" type.
        /// </summary>
        public static bool IsUnitySerializableList(Type collectionType)
        {
            if (collectionType == null) return false;
            return (GetElementType(collectionType) != null);
        }

        /// <summary>
        /// Determines if this type is a generic list. Use type.IsArray to determine if it is an array.
        /// </summary>
        public static bool IsGenericList(Type collectionType)
        {
            if (collectionType == null) return false;
            return (!collectionType.IsArray && GetElementType(collectionType) != null);
        }

        public static bool IsDictionary(Type collectionType)
        {
            if (collectionType == null) return false;
            return (collectionType.IsGenericType && collectionType.GetGenericTypeDefinition() == typeof(Dictionary<,>));
        }

        /// <summary>
        /// Figures out the element type of an array or list.
        /// </summary>
        public static Type GetElementType(Type collectionType)
        {
            if (collectionType == null) return null;

            if (collectionType.IsArray && collectionType.GetArrayRank() == 1)
            {
                return collectionType.GetElementType();
            }

            if (collectionType.IsGenericType && collectionType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type[] arguments = collectionType.GetGenericArguments();
                if (arguments != null && arguments.Length == 1)
                {
                    return arguments[0];
                }
            }

            return null;
        }

        public static Type GetKeyType(FieldInfo fieldInfo)
        {
            Type fieldType = fieldInfo.FieldType;

            Type[] parameters = fieldType.GetGenericArguments();
            if (parameters.Length > 0)
            {
                return parameters[0];
            }
            return null;
        }

        /// <summary>
        /// Recursively calls Type.GetField.
        /// </summary>
        /// <param name="type">The type to search for the field.</param>
        /// <param name="flags">
        /// The flags specifying which fields to find.
        /// You must specify one or both of BindingFlags.Instance/BindingFlags.Static and also one or both of BindingFlags.Public/BindingFlags.NonPublic.
        /// The only other applicable flag is BindingFlags.IgnoreCase.
        /// (There is no benefit over Type.GetField unless specifying BindingFlags.NonPublic.)
        /// </param>
        /// <returns>The field if found, else null.</returns>
        public static FieldInfo GetFieldInHierarchy(Type type, string fieldName, BindingFlags flags = InclusiveBindingFlags)
        {
            // Fast path for public search.
            if ((flags & BindingFlags.NonPublic) == BindingFlags.Default)
            {
                flags |= BindingFlags.FlattenHierarchy;
                flags &= ~BindingFlags.DeclaredOnly;
                return type.GetField(fieldName, flags);
            }

            // Prioritize the most derived version, even if it's private.
            flags &= ~BindingFlags.FlattenHierarchy;
            flags |= BindingFlags.DeclaredOnly;

            while (type != null)
            {
                FieldInfo field = type.GetField(fieldName, flags);
                if (field != null)
                {
                    return field;
                }

                type = type.BaseType;
            }
            return null;
        }

        /// <summary>
        /// Returns the most derived matching property with given name.
        /// </summary>
        public static PropertyInfo GetPropertyByNameInHierarchy(Type type, string propertyName, BindingFlags flags = InclusiveBindingFlags)
        {
            // Fast path for public search.
            if ((flags & BindingFlags.NonPublic) == BindingFlags.Default)
            {
                flags |= BindingFlags.FlattenHierarchy;
                flags &= ~BindingFlags.DeclaredOnly;
                return type.GetProperty(propertyName, flags);
            }

            // Prioritize the most derived version, even if it's private.
            flags &= ~BindingFlags.FlattenHierarchy;
            flags |= BindingFlags.DeclaredOnly;

            while (type != null)
            {
                PropertyInfo propertyInfo = type.GetProperty(propertyName, flags);
                if (propertyInfo != null)
                {
                    return propertyInfo;
                }

                type = type.BaseType;
            }
            return null;
        }

        /// <summary>
        /// Returns the most derived matching property with given name.
        /// </summary>
        public static PropertyInfo[] GetPropertiesInHierarchy(Type type, BindingFlags flags = InclusiveBindingFlags)
        {
            // Fast path for public search.
            if ((flags & BindingFlags.NonPublic) == BindingFlags.Default)
            {
                flags |= BindingFlags.FlattenHierarchy;
                flags &= ~BindingFlags.DeclaredOnly;
                return type.GetProperties(flags);
            }

            // Prioritize the most derived version, even if it's private.
            flags &= ~BindingFlags.FlattenHierarchy;
            flags |= BindingFlags.DeclaredOnly;

            var propertyList = new List<PropertyInfo>();
            while (type != null)
            {
                PropertyInfo[] properties = type.GetProperties(flags);
                if (properties != null)
                {
                    propertyList.AddRange(properties);
                }

                type = type.BaseType;
            }
            return propertyList.ToArray();
        }

        /// <summary>
        /// Returns the most derived matching method with any parameters, or null if not found.
        /// </summary>
        public static MethodInfo FindMethodInHierarchy(Type type, string methodName, BindingFlags flags, Binder binder, Type[] parameterTypes, ParameterModifier[] modifiers = null)
        {
            // Fast path for public search.
            if ((flags & BindingFlags.NonPublic) == BindingFlags.Default)
            {
                flags |= BindingFlags.FlattenHierarchy;
                flags &= ~BindingFlags.DeclaredOnly;
                return type.GetMethod(methodName, flags, binder, parameterTypes, modifiers);
            }

            // Prioritize the most derived version, even if it's private.
            flags &= ~BindingFlags.FlattenHierarchy;
            flags |= BindingFlags.DeclaredOnly;

            while (type != null)
            {
                MethodInfo methodInfo = type.GetMethod(methodName, flags, binder, parameterTypes, modifiers);
                if (methodInfo != null)
                {
                    return methodInfo;
                }

                type = type.BaseType;
            }
            return null;
        }

        /// <summary>
        /// Returns all the matching members found in the most derived type with the given name.
        /// </summary>
        public static MemberInfo[] GetMembersByNameInHierarchy(Type type, string memberName, MemberTypes memberTypes, BindingFlags bindingFlags = InclusiveBindingFlags)
        {
            // Fast path for public search.
            if ((bindingFlags & BindingFlags.NonPublic) == BindingFlags.Default)
            {
                bindingFlags |= BindingFlags.FlattenHierarchy;
                bindingFlags &= ~BindingFlags.DeclaredOnly;
                return type.GetMember(memberName, memberTypes, bindingFlags);
            }

            // Prioritize the most derived version, even if it's private.
            bindingFlags &= ~BindingFlags.FlattenHierarchy;
            bindingFlags |= BindingFlags.DeclaredOnly;

            List<MemberInfo> members = new List<MemberInfo>();
            while (type != null)
            {
                MemberInfo[] memberInfo = type.GetMember(memberName, memberTypes, bindingFlags);
                if (memberInfo != null)
                {
                    members.AddRange(memberInfo);
                }

                type = type.BaseType;
            }
            return members.ToArray();
        }

        /// <summary>
        /// Returns all the matching members found in the most derived type.
        /// </summary>
        public static MemberInfo[] GetMembersInHierarchy(Type type, BindingFlags bindingFlags = InclusiveBindingFlags)
        {
            // Fast path for public search.
            if ((bindingFlags & BindingFlags.NonPublic) == BindingFlags.Default)
            {
                bindingFlags |= BindingFlags.FlattenHierarchy;
                bindingFlags &= ~BindingFlags.DeclaredOnly;
                return type.GetMembers(bindingFlags);
            }

            // Prioritize the most derived version, even if it's private.
            bindingFlags &= ~BindingFlags.FlattenHierarchy;
            bindingFlags |= BindingFlags.DeclaredOnly;

            List<MemberInfo> members = new List<MemberInfo>();
            while (type != null)
            {
                MemberInfo[] memberInfo = type.GetMembers(bindingFlags);
                if (memberInfo != null)
                {
                    members.AddRange(memberInfo);
                }

                type = type.BaseType;
            }
            return members.ToArray();
        }

        /// <summary>
        /// Recursively calls Type.GetFields.
        /// </summary>
        /// <param name="type">The type to search for the field.</param>
        /// <param name="flags">
        /// The flags specifying which fields to find.
        /// You must specify one or both of BindingFlags.Instance/BindingFlags.Static and also one or both of BindingFlags.Public/BindingFlags.NonPublic.
        /// The only other applicable flag is BindingFlags.IgnoreCase.
        /// (There is no benefit over Type.GetField unless specifying BindingFlags.NonPublic.)
        /// </param>
        /// <returns>The field if found, else null.</returns>
        public static FieldInfo[] GetFieldsInHierarchy(Type type, BindingFlags flags = InclusiveBindingFlags)
        {
            // Fast path for public search.
            if ((flags & BindingFlags.NonPublic) == BindingFlags.Default)
            {
                flags |= BindingFlags.FlattenHierarchy;
                flags &= ~BindingFlags.DeclaredOnly;
                return type.GetFields(flags);
            }

            // Prioritize the most derived version, even if it's private.
            flags &= ~BindingFlags.FlattenHierarchy;
            flags |= BindingFlags.DeclaredOnly;

            List<FieldInfo> fieldList = new List<FieldInfo>();
            while (type != null)
            {
                FieldInfo[] fields = type.GetFields(flags);
                if (fields != null)
                {
                    fieldList.AddRange(fields);
                }

                type = type.BaseType;
            }
            return fieldList.ToArray();
        }

        /// <summary>
        /// Returns true if type implements interface with exact name as provided, case sensitive.
        /// </summary>
        /// <remarks>
        /// Useful in situations where you need to check interface is implemented when it exists 
        /// in multiple versions across many dlls, like our migration version support.
        /// </remarks>
        public static bool ImplementsAnyByName(Type type, params string[] interfaces)
        {
            foreach (Type interfaceType in type.GetInterfaces())
            {
                foreach (var interfaceName in interfaces)
                {
                    if (interfaceType.Name == interfaceName)
                        return true;
                }
            }
            return false;
        }
        #endregion

        #region Exception Analysis
        /// <summary>
        /// Determines whether an exception is one of the few exception types you should never catch.
        /// The only valid response to these exceptions is to crash because there are no longer any guarantees any state is valid.
        /// Does not rethrow the exception itself, because the "throw;" statement should be used to preserve the stack.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>true if the exception is dangerous, else false.</returns>
        /// <example>
        /// try {
        ///     // arbitrary code that causes uncertain exceptions
        /// }
        /// catch (Exception e) {
        ///     CheckDangerousException(e);
        ///     
        ///     if (logger.IsErrorEnabled) {
        ///         // Note the exception is passed
        ///         logger.Error("Unhandled exception when _____. Erroring out of _____.", e);
        ///     }
        ///     // generic error handling code
        /// }
        /// </example>
        /// <remarks>
        /// It might work better with some sort of "abort" or "failfast" method than throw, but those aren't always available.
        /// </remarks>
        public static void CheckDangerousException(Exception exception)
        {
            if (!IsDangerousException(exception))
            {
                return;
            }

            // Don't allocate a string here; might be an out-of-memory error
            FailFast("Terminating. Fatal exception caught.", exception);
        }



        private static void FailFast(string message, Exception exception)
        {
            try
            {
                Debug.LogError(typeof(ReflectionUtil) + message + exception);
            }
            finally
            {

                Environment.Exit(1);

                // If it didn't take, throw
                throw new Exception(message, exception);
            }
        }

        private static bool IsDangerousException(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            if (exception is SystemException && !IsIOException(exception))
            {
                // Critical; violates crucial assumptions we make
                if (exception is StackOverflowException || exception is OutOfMemoryException || exception is ThreadAbortException)
                {
                    return true;
                }

#pragma warning disable 612, 618
                // Can be critical, often AOT-related
                // Deprecated, but still happens sometimes, even in modern C# (often in interop; maybe just old APIs)
                if (exception is ExecutionEngineException)
                {
                    return true;
                }
#pragma warning restore 612, 618

                // Exceptions involved in executing native code; a nicer segfault that is generally uncatchable
                if (exception is AccessViolationException)
                {
                    return true;
                }

                // Windows API errors, mostly. Unmanaged code is tricky.
                // Could go either way on these
                if (exception is ExternalException)
                {
                    return true;
                }

                // Non-critical exceptions that break common assumptions and can lead to bad state:
                //     SecurityException
                //     NullReferenceException
                //     IndexOutOfRangeException
            }
            else
            {
                // Ignoring ContractException, which means we failed an assertion, meaning we're likely in a bad state
                // There are other ways to handle contract failures if we want to, but they can generally be logged
            }

            // Recurse
            if (exception.InnerException != null && IsDangerousException(exception.InnerException))
            {
                return true;
            }


            return false;
        }

        /// <summary>
        /// Checks the exception type to see if it is possibly related to I/O.
        /// Not recursive into <see cref="Exception.InnerException"/>.
        /// </summary>
        public static bool IsIOException(Exception exception)
        {
            if (exception is IOException || exception is IsolatedStorageException)
            {
                return true;
            }

            // Invalid path characters, text encoding errors that we have no control over.
            if (exception is ArgumentException || exception is NotSupportedException)
            {
                return true;
            }

            if (exception is System.Net.Sockets.SocketException)
            {
                return true;
            }

            // SecurityException can also be thrown, but that is programmer error
            if (exception is UnauthorizedAccessException)
            {
                return true;
            }


            // Is IOException
            //if (exception is NetworkSerializationException)
            //{
            //    return true;
            //}


            //bu - not using jsonreader yet
            //if (exception is Newtonsoft.Json.JsonReaderException || exception is Newtonsoft.Json.JsonWriterException || exception is Newtonsoft.Json.JsonSerializationException)
            //{
            //    return true;
            //}

            return false;
        }

        /// <summary>
        /// Checks the exception type to see if it is possibly related to string formatting.
        /// Not recursive into <see cref="Exception.InnerException"/>.
        /// </summary>
        public static bool IsStringFormatException(Exception exception)
        {
            if (exception is FormatException)
            {
                return true;
            }

            // Apparently mono doesn't check arguments correctly.
            if (exception is IndexOutOfRangeException || exception is ArgumentNullException)
            {
                return true;
            }

            return false;
        }
        #endregion

        #region Cached Attribute Access
        /// <summary>
        /// Cached (fast) check to see if a type or member has an attribute.
        /// </summary>
        /// <param name="attributeProvider">The <see cref="System.Type"/>, <see cref="System.Reflection.MemberInfo"/>, etc to look up.</param>
        public static bool HasAttribute<TAttribute>(ICustomAttributeProvider attributeProvider)
            where TAttribute : Attribute
        {
            return (FindAttribute<TAttribute>(attributeProvider) != null);
        }

        /// <summary>
        /// Cached (fast) check to look for a specific attribute for a type or member, or return null.
        /// </summary>
        /// <param name="customAttributeProvider">The <see cref="System.Type"/>, <see cref="System.Reflection.MemberInfo"/>, etc to look up.</param>
        public static TAttribute FindAttribute<TAttribute>(ICustomAttributeProvider customAttributeProvider)
            where TAttribute : Attribute
        {
            foreach (Attribute attribute in GetAllAttributes(customAttributeProvider))
            {
                if (attribute is TAttribute) return (TAttribute)attribute;
            }
            return null;
        }

        /// <summary>
        /// Cached (fast) check to look for a specific type of attributes, or return null.
        /// </summary>
        /// <param name="customAttributeProvider">The <see cref="System.Type"/>, <see cref="System.Reflection.MemberInfo"/>, etc to look up.</param>
        public static void AppendAttributes<TAttribute>(List<TAttribute> buffer, ICustomAttributeProvider customAttributeProvider)
            where TAttribute : Attribute
        {
            foreach (Attribute attribute in GetAllAttributes(customAttributeProvider))
            {
                if (attribute is TAttribute) buffer.Add((TAttribute)attribute);
            }
        }

        /// <summary>
        /// Cached (fast) check to get all attributes for a type or member.
        /// Includes inherited attributes.
        /// Treat array as immutable.
        /// </summary>
        /// <param name="customAttributeProvider">The <see cref="System.Type"/>, <see cref="System.Reflection.MemberInfo"/>, etc to look up.</param>
        /// <returns>A never-null array of attributes.</returns>
        public static Attribute[] GetAllAttributes(ICustomAttributeProvider customAttributeProvider)
        {
            var lookup = AttributeStatics.AttributeProviderToAttributesLookup.Value;
            Attribute[] attributes;
            if (!lookup.TryGetValue(customAttributeProvider, out attributes))
            {
                attributes = GetNonCachedAttributes(customAttributeProvider) ?? ArrayUtil.Empty<Attribute>();
                lookup.Add(customAttributeProvider, attributes);
            }
            return attributes;
        }

        private static Attribute[] GetNonCachedAttributes(ICustomAttributeProvider customAttributeProvider)
        {
            if (customAttributeProvider is MemberInfo) return Attribute.GetCustomAttributes((MemberInfo)customAttributeProvider);

            if (customAttributeProvider is ParameterInfo) return Attribute.GetCustomAttributes((ParameterInfo)customAttributeProvider);

            return (Attribute[])customAttributeProvider.GetCustomAttributes(typeof(Attribute), false);
        }
        #endregion

        #region Cached Enum Values
        /// <summary>
        /// Returns the set of enum values for the given enum type.
        /// Do not edit this array.
        /// </summary>
        public static Array GetEnumValues(Type enumType)
        {
            Array values;
            if (!EnumStatics.EnumValues.Value.TryGetValue(enumType, out values)) {
                values = Enum.GetValues(enumType);
                Array.Sort(values);

                object previous = null;
                int output = 0;
                for (int i = 0; i < values.Length; ++i) {
                    object current = values.GetValue(i);
                    if (!current.Equals(previous)) {
                        values.SetValue(current, output);
                        previous = current;
                        output += 1;
                    }
                }
                if (output != values.Length) {
                    Array original = values;
                    values = Array.CreateInstance(enumType, output);
                    Array.Copy(original, values, output);
                }

                EnumStatics.EnumValues.Value[enumType] = values;
            }
            return values;
        }

        /// <summary>
        /// Gets the values of the specified enum.
        /// Do not edit this array.
        /// </summary>
        public static T[] GetEnumValues<T>()
            where T : struct, IComparable
        {
            return (T[])GetEnumValues(typeof(T));
        }

        public static int[] GetEnumIntegers(Type enumType)
        {
            int[] result;
            if (!EnumStatics.EnumIntegers.Value.TryGetValue(enumType, out result))
            {
                Array values = GetEnumValues(enumType);
                result = new int[values.Length];
                for (int i = 0; i < values.Length; ++i)
                {
                    result[i] = Convert.ToInt32(values.GetValue(i));
                }
                EnumStatics.EnumIntegers.Value[enumType] = result;
            }
            return result;
        }
        #endregion

        #region Runtime C# Casts
        /// <summary>
        /// Performs standard C# cast behavior on runtime types.
        /// </summary>
        public static T RuntimeCast<T>(object value)
        {
            if (value is T) return (T)value;

            return (T)RuntimeCast(value, typeof(T));
        }

        /// <summary>
        /// Performs standard C# cast behavior on runtime types.
        /// </summary>
        public static object RuntimeCast(object value, Type castType)
        {
            if (value == null)
            {
                if (!castType.IsValueType || ReflectionUtil.IsNullable(castType))
                {
                    return null;
                }
            }
            else if (castType.IsInstanceOfType(value))
            {
                return value;
            }
            else
            {
                object result;
                if (TryComplexRuntimeCast(value, castType, out result))
                {
                    return result;
                }
            }

            throw new InvalidCastException(
                    "Cannot runtime-cast from " +
                    (value == null ? "null" : "\"" + ReflectionUtil.CSharpFullName(value.GetType()) + "\"") +
                    " to \"" + ReflectionUtil.CSharpFullName(castType) + "\".");
        }

        /// <summary>
        /// Performs standard C# cast behavior on runtime types.
        /// </summary>
        public static bool TryRuntimeCast<T>(object value, out T result)
        {
            if (value is T)
            {
                result = (T)value;
                return true;
            }

            object untypedResult;
            bool success = TryRuntimeCast(value, typeof(T), out untypedResult);
            result = success ? (T)untypedResult : default(T);
            return true;
        }

        /// <summary>
        /// Performs standard C# cast behavior on runtime types.
        /// </summary>
        public static bool TryRuntimeCast(object value, Type castType, out object result)
        {
            if (castType == null) throw new ArgumentNullException("castType");

            if (value == null)
            {
                result = null;
                return (!castType.IsValueType || ReflectionUtil.IsNullable(castType));
            }
            else if (castType.IsInstanceOfType(value))
            {
                result = value;
                return true;
            }
            else
            {
                try
                {
                    return TryComplexRuntimeCast(value, castType, out result);
                }
                catch (Exception e)
                {
                    // NOTE: We shouldn't swallow in this type of context.
                    CheckDangerousException(e);

                    result = null;
                    return false;
                }
            }
        }

        /// <summary>
        /// RuntimeCast section that may throw.
        /// </summary>
        private static bool TryComplexRuntimeCast(object value, Type castType, out object result)
        {
            if (value == null)
            {
                result = null;
                return (!castType.IsValueType || ReflectionUtil.IsNullable(castType));
            }
            else
            {
                Type valueType = value.GetType();
                if (castType.IsPrimitive && valueType.IsPrimitive)
                {
                    if (castType != typeof(bool) && valueType != typeof(bool))
                    {
                        result = System.Convert.ChangeType(value, castType, CultureInfo.InvariantCulture);
                        return true;
                    }
                }

                MethodInfo conversionMethod = GetConversionOperator(valueType, castType);
                if (conversionMethod != null)
                {
                    result = conversionMethod.Invoke(null, new[] { value });
                    return true;
                }

                result = null;
                return false;
            }
        }

        private static MethodInfo GetConversionOperator(Type valueType, Type castType)
        {
            TypePair key = new TypePair(valueType, castType);

            // synchronize static variable usage
            lock (CastStatics.Sync)
            {
                MethodInfo conversionMethod;
                if (!CastStatics.CastLookup.TryGetValue(key, out conversionMethod))
                {
                    // may be null
                    conversionMethod = FindConversionOperator(valueType, castType);
                    CastStatics.CastLookup.Add(key, conversionMethod);
                }
                return conversionMethod;
            }
        }

        /// <summary>
        /// Only supports direct casts, not chains of MyType -> int -> long.
        /// </summary>
        private static MethodInfo FindConversionOperator(Type valueType, Type castType)
        {
            // Already synchronized, so we can share a list.
            List<MethodInfo> methods = CastStatics.MethodTemporaryBuffer;
            methods.Clear();

            // Find all relevant methods.
            BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            methods.AddRange(castType.GetMethods(flags));
            methods.AddRange(valueType.GetMethods(flags));

            // Figure out the best one, if any.
            MethodInfo method = null;
            for (int i = 0; i < methods.Count; ++i)
            {
                if (!IsConversionOperatorValid(methods[i], valueType, castType))
                {
                    continue;
                }

                if (method == null || CompareConversionOperators(method, methods[i], valueType, castType) > 0)
                {
                    method = methods[i];
                }
            }
            return method;
        }

        /// <summary>
        /// Determines whether the given method will be able to cast from <paramref name="valueType"/> to <paramref name="castType"/>.
        /// </summary>
        private static bool IsConversionOperatorValid(MethodInfo method, Type valueType, Type castType)
        {
            // Check method name.
            switch (method.Name)
            {
                case "op_Implicit":
                case "op_Explicit":
                    // Fall through
                    break;
                default:
                    return false;
            }

            // Check the parameter.
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != 1)
            {
                return false;
            }
            Type parameterType = parameters[0].ParameterType;
            if (!parameterType.IsAssignableFrom(valueType))
            {
                return false;
            }

            // Check return type.
            if (castType.IsAssignableFrom(method.ReturnType))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Weigh two valid methods, returning a higher number for one with higher priority.
        /// </summary>
        private static int CompareConversionOperators(MethodInfo left, MethodInfo right, Type valueType, Type castType)
        {
            // C# values exact casts much more highly.
            // Supposed to be a compiler error if there any ambiguity, but we're being permissive.
            int compare = (left.ReturnType == castType).CompareTo(right.ReturnType == castType);
            if (compare != 0) return compare;

            Type leftParameter = left.GetParameters()[0].ParameterType;
            Type rightParameter = right.GetParameters()[0].ParameterType;

            compare = (leftParameter == valueType).CompareTo(rightParameter == valueType);
            if (compare != 0) return compare;

            // Prefer a real match over an indirect IConvertible match
            compare = castType.IsAssignableFrom(left.ReturnType).CompareTo(castType.IsAssignableFrom(right.ReturnType));
            if (compare != 0) return compare;

            // Prefer more derived types
            compare = right.ReturnType.IsAssignableFrom(left.ReturnType).CompareTo(left.ReturnType.IsAssignableFrom(right.ReturnType));
            if (compare != 0) return compare;
            compare = rightParameter.IsAssignableFrom(leftParameter).CompareTo(leftParameter.IsAssignableFrom(rightParameter));
            if (compare != 0) return compare;

            // Prefer implicit over explicit
            return StringComparer.Ordinal.Compare(left.Name, right.Name);
        }
        #endregion

        #region Private Types
        /// <summary>
        /// Specialized version of ImmutableTuple to avoid mono compiler bug.
        /// </summary>
        [StructLayout(LayoutKind.Auto)]
        public struct TypePair : IEquatable<TypePair>
        {
            /// <summary>
            /// A tuple member.
            /// </summary>
            public readonly Type Item1;
            /// <summary>
            /// A tuple member.
            /// </summary>
            public readonly Type Item2;

            /// <summary>
            /// Creates a new key tuple.
            /// </summary>
            /// <param name="item1">A tuple member.</param>
            /// <param name="item2">A tuple member.</param>
            public TypePair(Type item1, Type item2)
            {
                Item1 = item1;
                Item2 = item2;
            }

            /// <summary>
            /// Compare two key tuples for equality.
            /// </summary>
            /// <param name="a">A key tuple.</param>
            /// <param name="b">Another key tuple.</param>
            /// <returns>true if equal, else false.</returns>
            public static bool operator ==(TypePair a, TypePair b)
            {
                return a.Equals(b);
            }

            /// <summary>
            /// Compare two key tuples for inequality.
            /// </summary>
            /// <param name="a">A key tuple.</param>
            /// <param name="b">Another key tuple.</param>
            /// <returns>true if not equal, else false.</returns>
            public static bool operator !=(TypePair a, TypePair b)
            {
                return !a.Equals(b);
            }

            /// <summary>
            /// Compares against another object for equality.
            /// </summary>
            /// <param name="obj">The object to compare with.</param>
            /// <returns>true if equal, else false.</returns>
            public bool Equals(TypePair obj)
            {
                return (Item1 == obj.Item1 && Item2 == obj.Item2);
            }

            /// <summary>
            /// Compares against another object for equality.
            /// </summary>
            /// <param name="obj">The object to compare with.</param>
            /// <returns>true if equal, else false.</returns>
            public override bool Equals(object obj)
            {
                if (!(obj is TypePair))
                {
                    return false;
                }
                return (this == (TypePair)obj);
            }

            /// <summary>
            /// Gets a hash code for this tuple.
            /// </summary>
            /// <returns>The hash code for this tuple.</returns>
            public override int GetHashCode()
            {
                return BitUtil.CombineHashCodes(Item1.GetHashCode(), Item2.GetHashCode());
            }

            /// <summary>
            /// Converts this request to a string representation.
            /// </summary>
            /// <returns>The string representation of this object.</returns>
            public override string ToString()
            {
                return ReflectionUtil.CSharpFullName(GetType()) + "(" + ReflectionUtil.CSharpFullName(Item1) + ", " + ReflectionUtil.CSharpFullName(Item2) + ")";
            }
        }

        private static class CastStatics
        {
            public static readonly object Sync = new object();
            public static readonly Dictionary<TypePair, MethodInfo> CastLookup = new Dictionary<TypePair, MethodInfo>();
            public static readonly List<MethodInfo> MethodTemporaryBuffer = new List<MethodInfo>();
        }

        private static class AttributeStatics {
            public static readonly System.Threading.ThreadLocal<Dictionary<ICustomAttributeProvider, Attribute[]>> AttributeProviderToAttributesLookup =
                new System.Threading.ThreadLocal<Dictionary<ICustomAttributeProvider, Attribute[]>>(
                    () => new Dictionary<ICustomAttributeProvider, Attribute[]>());
        }

        private static class EnumStatics {
            public static readonly System.Threading.ThreadLocal<Dictionary<Type, Array>> EnumValues = new System.Threading.ThreadLocal<Dictionary<Type, Array>>(() => new Dictionary<Type, Array>());
            public static readonly System.Threading.ThreadLocal<Dictionary<Type, int[]>> EnumIntegers = new System.Threading.ThreadLocal<Dictionary<Type, int[]>>(() => new Dictionary<Type, int[]>());
        }

        private static class AssemblyStatics
        {
            public static readonly Assembly[] AllAssemblies;
            public static readonly Assembly[] RuntimeAssemblies;
            public static readonly Assembly[] EditorAssemblies;

            static AssemblyStatics()
            {
                AllAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                Array.Sort(AllAssemblies, CompareAssemblies);

                RuntimeAssemblies = AllAssemblies;
                EditorAssemblies = ArrayUtil.Empty<Assembly>();

#if UNITY_EDITOR
                RuntimeAssemblies = AllAssemblies.Where(assembly => !IsEditorAssembly(assembly)).ToArray();
                EditorAssemblies = AllAssemblies.Where(assembly => IsEditorAssembly(assembly)).ToArray();
#endif
            }

            private static int CompareAssemblies(Assembly left, Assembly right)
            {
                int compare = -IsBuiltInAssembly(left).CompareTo(IsBuiltInAssembly(right));
                if (compare == 0) { compare = string.Compare(left.FullName ?? "", right.FullName ?? "", StringComparison.OrdinalIgnoreCase); }
                return compare;
            }
        }

        private static class RuntimeTypeStatics
        {
#if GAME_SERVER
            // Server doesn't like to look at this type until the static constructor runs
            public static Type[] RuntimeTypes { get { return SelectAll(AssemblyStatics.RuntimeAssemblies); } }
#else
            public static readonly Type[] RuntimeTypes = SelectAll(AssemblyStatics.RuntimeAssemblies);
#endif

            public static Type[] SelectAll(Assembly[] assemblies)
            {
                // Needs to be deterministic
                List<Type> output = new List<Type>(4096);
                foreach (Assembly assembly in assemblies) {
                    try {
                        Type[] types = assembly.GetTypes();
                        Array.Sort(types, CompareTypes);
                        output.AddRange(types);
                    }
                    catch (Exception e) {
                        Debug.LogException(e);

                        throw;
                    }
                }
                return output.ToArray();
            }

            public static int CompareTypes(Type left, Type right)
            {
                return StringComparer.Ordinal.Compare(left.FullName, right.FullName);
            }
        }

        private static class EditorTypeStatics
        {
            public static readonly Type[] EditorTypes =
#if UNITY_EDITOR
                RuntimeTypeStatics.SelectAll(AssemblyStatics.EditorAssemblies);
#else
                ArrayUtil.Empty<Type>();
#endif
        }

        private static class TypeCacheStatics
        {
            public static readonly object SyncRoot = new object();
            public static readonly Dictionary<Type, Type[]> AssignableCache = new Dictionary<Type, Type[]>();
        }

        private static class TypeNameStatics
        {
            public static readonly object SyncRoot = new object();
            public static readonly Dictionary<string, Type> NameToTypeLookup = new Dictionary<string, Type>(StringComparer.Ordinal);
        }

        private static class CSharpNameStatics
        {
            public static readonly object SyncRoot = new object();
            public static readonly Dictionary<MemberInfo, string> MemberToNameLookup = new Dictionary<MemberInfo, string>();
            public static readonly Dictionary<MemberInfo, string> MemberToFullNameLookup = new Dictionary<MemberInfo, string>();
        }
#endregion
    }
}
