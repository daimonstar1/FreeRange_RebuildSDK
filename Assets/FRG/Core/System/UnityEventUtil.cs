
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using UnityEngine;
using UnityEngine.Events;

namespace FRG.Core {
    
    [Serializable] public class UnityEvent_Int : UnityEvent<int> { }
    [Serializable] public class UnityEvent_Float : UnityEvent<float> { }
    [Serializable] public class UnityEvent_Bool : UnityEvent<bool> { }
    [Serializable] public class UnityEvent_String : UnityEvent<string> { }
    [Serializable] public class UnityEvent_Object : UnityEvent<UnityEngine.Object> { }
    [Serializable] public class UnityEvent_Sprite : UnityEvent<UnityEngine.Sprite> { }

    /// <summary>
    /// Static utility class for UnityEvents
    /// </summary>
    public static class UnityEventUtil {
        
        //bindingFlags for public/nonpublic instance members
        private static BindingFlags instanceBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        [Serializable]
        public enum PersistentListenerMode {
            EventDefined,
            Void,
            Object,
            Int,
            Float,
            String,
            Bool
        }

        #region WRAPPER_STRUCTS
        /// <summary>
        /// Reflection wrapper for internal UnityEngine.Events.PersistentCallGroup
        /// </summary>
        public struct HackedPersistentCallGroup {

            //the base (PersistentCallGroup) to modify
            public object root;

            //UnityEngine.Events.PersistentCallGroup.AddListener()
            private static MethodInfo _addListenerMethod;
            //UnityEngine.Events.PersistentCallGroup.AddListener()
            private static MethodInfo _addPersistentListenerMethod;
            //UnityEngine.Events.PersistentCallGroup.RemoveListener(int)
            private static MethodInfo _removeListenerMethod;
            //UnityEngine.Events.PersistentCallGroup.Clear()
            private static MethodInfo _clearMethod;

            /// <summary>
            /// Initialize
            /// </summary>
            static HackedPersistentCallGroup() {
                _addListenerMethod = _persistentCallGroupType.GetMethod("AddListener", instanceBindingFlags, null, Type.EmptyTypes, null);
                _addPersistentListenerMethod = _persistentCallGroupType.GetMethod("AddPersistentListener", instanceBindingFlags, null, Type.EmptyTypes, null);
                _removeListenerMethod = _persistentCallGroupType.GetMethod("RemoveListener", instanceBindingFlags);
                _clearMethod = _persistentCallGroupType.GetMethod("Clear", instanceBindingFlags);
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="root">The base (UnityEngine.Events.PersistentCallGroup) to modify</param>
            public HackedPersistentCallGroup(object root) {
                this.root = root;
            }

            /// <summary>
            /// Return all persistent calls contained within this persistent call group
            /// </summary>
            public HackedPersistentCall[] Calls {
                get {
                    IList callsList = _persistentCallGroup_calls.GetValue(root) as IList;
                    if(callsList != null) {
                        HackedPersistentCall[] ret = new HackedPersistentCall[callsList.Count];
                        for(int i=0; i<callsList.Count; ++i) {
                            ret[i] = new HackedPersistentCall(callsList[i]);
                        }
                        return ret;
                    }
                    return null;
                }
            }

            /// <summary>
            /// Add a listener to this persistent call group
            /// </summary>
            public void Add(UnityAction action) {
                _addListenerMethod.Invoke(root, new object[] { action });
            }

            /// <summary>
            /// Add a persistent listener to this persistent call group
            /// </summary>
            /// <param name="action"></param>
            public void AddPersistentListener(UnityAction action) {
                _addPersistentListenerMethod.Invoke(root, new object[] { action });
            }

            /// <summary>
            /// Remove a listener from this persistent call group
            /// </summary>
            /// <param name="index">Index of the listener to remove</param>
            public void Remove(int index) {
                _removeListenerMethod.Invoke(root, new object[] { index });
            }

            /// <summary>
            /// Clear all listeners from this persistent call group
            /// </summary>
            public void Clear() {
                _clearMethod.Invoke(root, null);
            }
        }

        /// <summary>
        /// Reflection wrapper for internal UnityEngine.Events.PersistentCall
        /// </summary>
        public struct HackedPersistentCall {

            //the base (PersistentCall) to modify
            public object root;

            //cache storing the expected parameter type for each [methodName,targetType]
            private static Dictionary<string, Dictionary<Type, Type>> _cachedMethodArgumentTypes
                = new Dictionary<string, Dictionary<Type, Type>>();
            
            //cache mapping which method-names are valid for each type
            private static Dictionary<Type, Dictionary<string, bool>> _cachedValidMethods
                = new Dictionary<Type, Dictionary<string, bool>>();

            //in case we set the method target to something which does NOT have the method we specified in the (UnityEvent) inspector,
            //   if we set with a (GameObject), let's map which (Components) are valid for the existing method info
            private static Dictionary<int, Dictionary<string, Type>> _cachedValidComponentTypes
                = new Dictionary<int, Dictionary<string, Type>>();

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="root">The base (UnityEngine.Events.PersistentCall) to modify</param>
            public HackedPersistentCall(object root) {
                this.root = root;
            }

            /// <summary>
            /// The method target. If this (PersistentCall) refers to a method contained within a (Component),
            ///   and we set this value with a (GameObject)
            /// </summary>
            public UnityEngine.Object Target {
                get {
                    //return the existing target
                    return _persistentCall_target.GetValue(root).LooseCast<UnityEngine.Object>();
                }
                
                set {
                    //if the new target is null, we do not need to perform any type compatability checks as listed below. 
                    //  We can assume the user is just clearing the target. If the existing target is null, we still need to
                    //  perform some type-checking to ensure that we set with the corrected-version of the target if needed
                    if(value == null){
                        _persistentCall_target.SetValue(root, value);
                    } else {
                        
                        //if neither value is null, it means we are replacing a possibly-valid target with a possibly-invalid one.
                        //  We need to perform some type checks to ensure that the call will still be able to invoke
                        if(IsTargetValid(value, MethodName)) {
                            //target is valid; set the value
                            _persistentCall_target.SetValue(root, value);
                        } else {
                            //the value is non-null and is non-valid; attempt to find the corrected-version of the value
                            var corrected = GetCorrectObjectForMethod(value, MethodName);

                            if((corrected != null) && (corrected != value)) {
                                Target = corrected;
                            } else {
                                //if we're in EDIT mode and couldn't find a valid target; just set the invalid targret anywayss
                                if(!Application.isPlaying) {
                                    _persistentCall_target.SetValue(root, value);
                                }
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// The method-name which will be invoked via (MethodInfo)
            /// </summary>
            public string MethodName {
                get { return _persistentCall_methodName.GetValue(root).LooseCast<string>(); }
                set {

                    //read the existing method name
                    string existingMethodName = MethodName;

                    //if the new method name is different than the existing, do the modification
                    if(existingMethodName != value) {
                        _persistentCall_methodName.SetValue(root, value);

                        //this will call the Target.Set() property method; which will do some target validation
                        //  to try and ensure that our method target is something valid
                        Target = Target;
                    }
                }
            }

            public PersistentListenerMode Mode {
                get {
                    return (PersistentListenerMode)_persistentCall_mode.GetValue(root, null).LooseCast<int>();
                }
                set {
                    _persistentCall_mode.SetValue(root, (int)value, null);
                }
            }

            /// <summary>
            /// Deduce the expected type of the first parameter of the current method via reflection/caching
            /// </summary>
            public Type ExpectedPersistentArgumentType {
                get {

                    ///TODO use the PersistentCall.Mode to resolve ambiguous methods?

                    if(_cachedMethodArgumentTypes == null) {
                        _cachedMethodArgumentTypes = new Dictionary<string, Dictionary<Type, Type>>();
                    }

                    string methName = MethodName;
                    if(!_cachedMethodArgumentTypes.ContainsKey(methName)) {
                        _cachedMethodArgumentTypes[methName] = new Dictionary<Type, Type>();
                    }

                    Type targetType = ((Target != null) ? Target.GetType() : null);
                    if(targetType != null) {

                        //if we have not calculated the expected type for this [methodName,target] yet, do so
                        if(!_cachedMethodArgumentTypes[methName].ContainsKey(targetType)) {

                            //get the method via reflection
                            var meth = targetType.GetMethod(methName, instanceBindingFlags);
                            if(meth != null) {

                                //if the method takes at least one parameter, cache its parameterType
                                var methParams = meth.GetParameters();
                                if(methParams != null && methParams.Length > 0) {

                                    //cache and return the type of the FIRST parameter taken by the method; disregard other parameters
                                    var ret = methParams[0].ParameterType;
                                    _cachedMethodArgumentTypes[methName][targetType] = ret;
                                    return ret;

                                    //if the method exists but takes no parameters, cache and return null
                                } else {
                                    _cachedMethodArgumentTypes[methName][targetType] = null;
                                    return null;
                                }

                                //if the method does not exist, cache and return null
                            } else {
                                _cachedMethodArgumentTypes[methName][targetType] = null;
                                return null;
                            }

                            //if the value was already cached before, return the cached value
                        } else {
                            return _cachedMethodArgumentTypes[methName][targetType];
                        }

                        //the target/targetType is null; this makes no sense; return null
                    } else {
                        return null;
                    }
                }
            }

            /// <summary>
            /// Argument data for this call
            /// </summary>
            public HackedArgumentCache Arguments {
                get { return new HackedArgumentCache(_persistentCall_arguments.GetValue(root)); }
            }

            /// <summary>
            /// Return whether the specified unity object is a valid target for the specified method-name
            /// </summary>
            public bool IsTargetValid(UnityEngine.Object tar, string methName) {
                
                //if this event has no method, allow any sort of target
                if(string.IsNullOrEmpty(methName)) {
                    return true;
                }

                //null targets are never viable for unity events with method-names
                if(tar == null) {
                    return false;
                }

                var tarType = tar.GetType();
                if(!_cachedValidMethods.ContainsKey(tarType)) {
                    _cachedValidMethods[tarType] = new Dictionary<string, bool>();
                }

                var thisCache = _cachedValidMethods[tarType];
                if(!thisCache.ContainsKey(methName)) {
                    thisCache[methName] = (null != UnityEventBase.GetValidMethodInfo(tar, methName, new Type[] { ExpectedPersistentArgumentType }));
                }

                return thisCache[methName];
            }

            /// <summary>
            /// Attempts to return a valid target according to the specified method name
            /// </summary>
            /// <param name="obj">The original object; May be a game-object, component, or some other unity object</param>
            public UnityEngine.Object GetCorrectObjectForMethod(UnityEngine.Object obj, string methName) {
                if(obj == null)      throw new ArgumentNullException("obj");
                if(methName == null) throw new ArgumentNullException("methName");

                int valueInstanceId = obj.GetInstanceID();
                if(!_cachedValidComponentTypes.ContainsKey(valueInstanceId)) {
                    _cachedValidComponentTypes[valueInstanceId] = new Dictionary<string, Type>();
                }
                
                var thisCache = _cachedValidComponentTypes[valueInstanceId];
                if(!thisCache.ContainsKey(methName)) {
                    Type correctedComponentType = null;

                    if(obj is GameObject) {
                        foreach(var comp in ((GameObject)obj).GetComponents<Component>()) {
                            if(comp != null && IsTargetValid(comp, methName)) {
                                correctedComponentType = comp.GetType();
                                break;
                            }
                        }
                    }else if(obj is Component) {
                        foreach(var comp in ((Component)obj).GetComponents<Component>()) {
                            if(comp != null && IsTargetValid(comp, methName)) {
                                correctedComponentType = comp.GetType();
                                break;
                            }
                        }
                    } else {
                        correctedComponentType = typeof(UnityEngine.Object);
                    }
                    
                    thisCache[methName] = correctedComponentType;
                }

                var retType = thisCache[methName];

                if(retType != null) {
                    if(typeof(GameObject).IsAssignableFrom(retType)) {
                        if(obj is GameObject) {
                            return (GameObject)obj;
                        } else if(obj is Component) {
                            return ((Component)obj).gameObject;
                        } else {
                            return obj;
                        }

                    }else if(typeof(Component).IsAssignableFrom(retType)) {
                        if(obj is GameObject) {
                            return ((GameObject)obj).GetComponent(retType);
                        } else if(obj is Component) {
                            return ((Component)obj).GetComponent(retType);
                        } else {
                            return obj;
                        }
                    } else {
                        return obj;
                    }
                } else {
                    return obj;
                }
            }
        }

        /// <summary>
        /// Reflection wrapper for internal UnityEngine.Events.PersistentCall
        /// </summary>
        public struct HackedArgumentCache {
            public object root;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="root">The base (UnityEngine.Events.HackedArgumentCache) to modify</param>
            public HackedArgumentCache(object root) {
                this.root = root;
            }

            /// <summary>
            /// Set the appropriate argument according to the value's type
            /// </summary>
            public void SetArgument(object value) {
                if(value == null) {
                    SetArgument<int>(0, typeof(int));
                    SetArgument<float>(0f, typeof(float));
                    SetArgument<string>("", typeof(string));
                    SetArgument<bool>(false, typeof(bool));
                    SetArgument<UnityEngine.Object>(null, typeof(UnityEngine.Object));
                } else {
                    if(value is int) {
                        SetArgument<int>((int)value, typeof(int));
                    } else if(value is float) {
                        SetArgument<float>((float)value, typeof(float));
                    } else if(value is string) {
                        SetArgument<string>((string)value, typeof(string));
                    } else if(value is bool) {
                        SetArgument<bool>((bool)value, typeof(bool));
                    } else if(value is UnityEngine.Object) {
                        SetArgument<UnityEngine.Object>((UnityEngine.Object)value, typeof(UnityEngine.Object));
                    }
                }
            }
            
            /// <summary>
            /// Set the appropriate argument according to the specified type parameter
            /// </summary>
            public void SetArgument<ARG>(ARG value, Type expectedArgType) {
                
                var argumentType = expectedArgType;

                ///TODO: cover more specified conversion cases; 
                /// IE: int->gameObject should do instance id lookup?
                if(typeof(UnityEngine.Object).IsAssignableFrom(argumentType)) {
                    ObjectArgument = value.LooseCast<UnityEngine.Object>();
                } else if(argumentType == typeof(int)) {
                    IntArgument = value.LooseCast<int>();
                } else if(argumentType == typeof(string)) {
                    StringArgument = value.ToString();
                } else if(argumentType == typeof(float)) {
                    FloatArgument = value.LooseCast<float>();
                } else if(argumentType == typeof(bool)) {
                    BoolArgument = value.LooseCast<bool>();
                } else {
                    if(typeof(UnityEngine.Object).IsAssignableFrom(typeof(ARG))) {
                        ObjectArgument = value.LooseCast<UnityEngine.Object>();
                    }
                    IntArgument = value.LooseCast<int>();
                    StringArgument = value.LooseCast<string>();
                    FloatArgument = value.LooseCast<float>();
                    BoolArgument = value.LooseCast<bool>();
                }
            }

            /// <summary>
            /// Return the appropriate argument according to the specified type parameter
            /// </summary>
            public ARG GetArgument<ARG>() {
                return (ARG)GetArgument(typeof(ARG));
            }

            /// <summary>
            /// Return the appropriate argument according to the specified type
            /// </summary>
            public object GetArgument(Type argumentType) {

                if(typeof(UnityEngine.Object).IsAssignableFrom(argumentType)) {
                    return ObjectArgument;
                } else if(argumentType == typeof(int)) {
                    return IntArgument;
                } else if(argumentType == typeof(string)) {
                    return StringArgument;
                } else if(argumentType == typeof(float)) {
                    return FloatArgument;
                } else if(argumentType == typeof(bool)) {
                    return BoolArgument;
                } else {
                    throw new ArgumentException("argumentType must be  (UnityEngine.Object), (int), (float), (bool), or (string)", "argumentType");
                }
            }

            //object argument
            public UnityEngine.Object ObjectArgument {
                get { return _argumentCache_objectArg.GetValue(root, null).LooseCast<UnityEngine.Object>(); }
                set {
                    _argumentCache_objectArg.SetValue(root, value, null);
                }
            }

            //object argument type
            public Type ObjectArgumentType {
                get { return Type.GetType(ObjectArgumentTypeName); }
            }

            //object argument type-name
            public string ObjectArgumentTypeName {
                get { return _argumentCache_objectTypeName.GetValue(root, null).LooseCast<string>(); }
            }

            //int argument
            public int IntArgument {
                get { return _argumentCache_intArg.GetValue(root).LooseCast<int>(); }
                set { _argumentCache_intArg.SetValue(root, value); }
            }

            //float argument
            public float FloatArgument {
                get { return _argumentCache_floatArg.GetValue(root).LooseCast<float>(); }
                set { _argumentCache_floatArg.SetValue(root, value); }
            }

            //string argument
            public string StringArgument {
                get { return _argumentCache_stringArg.GetValue(root).LooseCast<string>(); }
                set { _argumentCache_stringArg.SetValue(root, value); }
            }

            //bool argument
            public bool BoolArgument {
                get { return _argumentCache_boolArg.GetValue(root).LooseCast<bool>(); }
                set { _argumentCache_boolArg.SetValue(root, value); }
            }
        }
        #endregion WRAPPER_STRUCTS

        #region REFLECTION_INFOS
        //UnityEngine.Events.UnityEventBase.Assembly
        private static Assembly _unityEventAsm;
        //private UnityEngine.Events.PersistentCallGroup UnityEngine.Events.UnityEventBase.m_PersistentCalls
        private static FieldInfo _unityEvent_persistentCalls;
        //public void UnityEngine.Events.UnityEventBase.DirtyPersistentCalls()
        private static MethodInfo _unityEvent_dirtyPersistentCalls;

        //typeof(UnityEngine.Events.PersistentCallGroup)
        private static Type _persistentCallGroupType;
        //private List<UnityEngine.Events.PersistentCall> UnityEngine.Events.PersistentCallGroup.m_Calls;
        private static FieldInfo _persistentCallGroup_calls;
        
        //typeof(UnityEngine.Events.PersistentCall)
        private static Type _persistentCallType;
        //private UnityEngine.Object UnityEngine.Events.PersistentCall.m_Target
        private static FieldInfo _persistentCall_target;
        //private string UnityEngine.Events.PersistentCall.m_MethodName
        private static FieldInfo _persistentCall_methodName;
        //private UnityEngine.Events.ArgumentCache UnityEngine.Events.PersistentCall.m_Arguments
        private static FieldInfo _persistentCall_arguments;
        //public UnityEngine.Events.PersistentListenerMode UnityEngine.Events.PersistentCall.mode
        private static PropertyInfo _persistentCall_mode;
        
        //typeof(UnityEngine.Events.ArgumentCache)
        private static Type _argumentCacheType;
        //private UnityEngine.Object UnityEngine.Events.ArgumentCache.m_ObjectArgument
        private static PropertyInfo _argumentCache_objectArg;
        //private string UnityEngine.Events.ArgumentCache.m_ObjectArgumentAssemblyTypeName
        private static PropertyInfo _argumentCache_objectTypeName;
        //private int UnityEngine.Events.ArgumentCache.m_IntArgument
        private static FieldInfo _argumentCache_intArg;
        //private float UnityEngine.Events.ArgumentCache.m_FloatArgument
        private static FieldInfo _argumentCache_floatArg;
        //private string UnityEngine.Events.ArgumentCache.m_StringArgument
        private static FieldInfo _argumentCache_stringArg;
        //private bool UnityEngine.Events.ArgumentCache.m_BoolArgument
        private static FieldInfo _argumentCache_boolArg;
        #endregion REFLECTION_INFOS

        #region INITIALIZATION
        static UnityEventUtil() {

            _unityEventAsm = typeof(UnityEventBase).Assembly;

            _unityEvent_persistentCalls = typeof(UnityEventBase).GetField("m_PersistentCalls", instanceBindingFlags)
                                           ?? typeof(UnityEventBase).GetField("m_PersistentListeners");

            _unityEvent_dirtyPersistentCalls = typeof(UnityEventBase).GetMethod("DirtyPersistentCalls", instanceBindingFlags);

            _persistentCallGroupType   = _unityEventAsm.GetType("UnityEngine.Events.PersistentCallGroup");

            _persistentCallGroup_calls = _persistentCallGroupType.GetField("m_Calls", instanceBindingFlags)
                                      ?? _persistentCallGroupType.GetField("m_Listeners", instanceBindingFlags);

            _persistentCallType        = _unityEventAsm.GetType("UnityEngine.Events.PersistentCall");

            _persistentCall_target     = _persistentCallType.GetField("m_Target",     instanceBindingFlags)
                                      ?? _persistentCallType.GetField("instance",     instanceBindingFlags);

            _persistentCall_methodName = _persistentCallType.GetField("m_MethodName", instanceBindingFlags)
                                      ?? _persistentCallType.GetField("methodName",   instanceBindingFlags);

            _persistentCall_arguments  = _persistentCallType.GetField("m_Arguments",  instanceBindingFlags)
                                      ?? _persistentCallType.GetField("arguments",    instanceBindingFlags);

            _persistentCall_mode       = _persistentCallType.GetProperty("mode",      instanceBindingFlags);

            _argumentCacheType         = _unityEventAsm.GetType("UnityEngine.Events.ArgumentCache");

            _argumentCache_objectArg   = _argumentCacheType.GetProperty("unityObjectArgument", instanceBindingFlags);

            _argumentCache_objectTypeName = _argumentCacheType.GetProperty("unityObjectArgumentAssemblyTypeName", instanceBindingFlags);

            _argumentCache_intArg      = _argumentCacheType.GetField("m_IntArgument",    instanceBindingFlags)
                                      ?? _argumentCacheType.GetField("intArgument",      instanceBindingFlags);

            _argumentCache_floatArg    = _argumentCacheType.GetField("m_FloatArgument",  instanceBindingFlags)
                                      ?? _argumentCacheType.GetField("floatArgument",    instanceBindingFlags);

            _argumentCache_stringArg   = _argumentCacheType.GetField("m_StringArgument", instanceBindingFlags)
                                      ?? _argumentCacheType.GetField("stringArgument",   instanceBindingFlags);

            _argumentCache_boolArg     = _argumentCacheType.GetField("m_BoolArgument",   instanceBindingFlags)
                                      ?? _argumentCacheType.GetField("boolArgument",     instanceBindingFlags); //can't find this former name in the assembly; though adding it anyways
        }
        #endregion INITIALIZATION

        #region STATIC_METHODS

        #region GENERAL_METHODS
        /// <summary>
        /// Get the persistent calls array for this UnityEvent. 
        /// 
        /// NOTE: making modifications to the returned array itself have no effect on the event,
        ///    while modifying the contained (HackerPersistentCall)s WILL have an effect
        /// 
        /// For adding/removing calls, see: 
        ///    - UnityEventUtil.GetPersistentCallGroup(), 
        ///    - HackedPersistentcallGroup.Add(), 
        ///    - HackedPersistentCallGroup.Remove()
        /// </summary>
        public static HackedPersistentCall[] GetPersistentCalls(this UnityEventBase thisEvent) {
            if(thisEvent == null) {
                throw new ArgumentNullException("thisEvent");
            }

            return new HackedPersistentCallGroup(_unityEvent_persistentCalls.GetValue(thisEvent)).Calls;
        }

        /// <summary>
        /// Get the persistent-call-group for this UnityEvent
        /// </summary>
        public static HackedPersistentCallGroup GetPersistentCallGroup(this UnityEventBase thisEvent) {
            if(thisEvent == null) {
                throw new ArgumentNullException("thisEvent");
            }

            return new HackedPersistentCallGroup(thisEvent);
        }

        /// <summary>
        /// Mark this (UnityEvent)'s persistent calls dirty, queuing unity to re-build its method cache required for proper invokation
        /// </summary>
        /// <param name="thisEvent">The event to modify</param>
        public static void DirtyPersistentCalls(this UnityEventBase thisEvent) {
            if(thisEvent == null) {
                throw new ArgumentNullException("thisEvent");
            }

            _unityEvent_dirtyPersistentCalls.Invoke(thisEvent, null);
        }
        #endregion GENERAL_METHODS

        #region INVOKATION_METHODS
        /// <summary>
        /// Invoke this (UnityEvent) on a different target than what is specified in the inspector
        /// </summary>
        /// <typeparam name="TARGET"></typeparam>
        /// <param name="thisEvent"></param>
        /// <param name="target"></param>
        public static void InvokeOn<TARGET>(this UnityEvent thisEvent, TARGET target)
            where TARGET : UnityEngine.Object {

            //disallow null event
            if(thisEvent == null) {
                throw new ArgumentNullException("thisEvent");
            }

            //disallow null target
            if(target == null) {
                throw new ArgumentNullException("target");
            }

            //replace the persistent call targets
            thisEvent.ReplacePersistentCallTargets<TARGET>(target);

            //invoke the unity event
            thisEvent.Invoke();
        }

        public static void InvokeOn<TARGET, PARAM>(this UnityEvent thisEvent, TARGET target, PARAM persistentParameter)
            where TARGET : UnityEngine.Object {

            //disallow null event
            if(thisEvent == null) {
                throw new ArgumentNullException("thisEvent");
            }
            //disallow null target
            if(target == null) {
                throw new ArgumentNullException("target");
            }

            //replace the persistent call targets
            thisEvent.ReplacePersistentCallTargets<TARGET>(target);

            //replace the persistent parameter
            thisEvent.ReplacePersistentCallArguments<PARAM>(persistentParameter);
            
            //invoke the unity event
            thisEvent.Invoke();
        }
        
        /// <summary>
        /// Override the parameter within all persistent calls in this argument-less (UnityEvent), then invoke it
        /// </summary>
        /// <typeparam name="PARAM">Type of persistent argument to pass</typeparam>
        /// <param name="thisEvent">The event to modify and invoke</param>
        /// <param name="persistentParameter">The new persistent call parameter to override with</param>
        public static void Invoke<PARAM>(this UnityEvent thisEvent, PARAM persistentParameter) {
            if(thisEvent == null) {
                throw new ArgumentNullException("thisEvent");
            }

            thisEvent.ReplacePersistentCallArguments<PARAM>(persistentParameter);
            thisEvent.Invoke();
        }

        /// <summary>
        /// Override the parameter within all persistent calls in this single-argument (UnityEvent), then invoke it
        /// </summary>
        /// <typeparam name="ARG0">Type of the non-persistent argument to pass to the event</typeparam>
        /// <typeparam name="PARAM">Type of the persistent argument to pass</typeparam>
        /// <param name="thisEvent">The event to modify and invoke</param>
        /// <param name="arg0">The non-persistent argument to pass to the invokation</param>
        /// <param name="persistentArgOverride">The new persistent call parameter to override with</param>
        public static void Invoke<ARG0, PARAM>(this UnityEvent<ARG0> thisEvent, ARG0 arg0, PARAM persistentArgOverride) {
            if(thisEvent == null) {
                throw new ArgumentNullException("thisEvent");
            }

            thisEvent.ReplacePersistentCallArguments<PARAM>(persistentArgOverride);
            thisEvent.Invoke(arg0);
        }
        
        /// <summary>
        /// Override the parameter within all persistent calls in this two-argument (UnityEvent), then invoke it
        /// </summary>
        /// <typeparam name="ARG0">Type of the first non-persistent argument to pass to the event</typeparam>
        /// <typeparam name="ARG1">Type of the second non-persistent argument to pass to the event</typeparam>
        /// <typeparam name="PARAM">Type of the persistent argument to pass</typeparam>
        /// <param name="thisEvent">The event to modify and invoke</param>
        /// <param name="arg0">The first non-persistent argument to pass to the invokation</param>
        /// <param name="arg1">The second non-persistent argument to pass to the invokation</param>
        /// <param name="persistentArgOverride">The new persistent call parameter to override with</param>
        public static void Invoke<ARG0, ARG1, PARAM>(this UnityEvent<ARG0,ARG1> thisEvent, ARG0 arg0, ARG1 arg1, PARAM persistentArgOverride) {
            if(thisEvent == null) {
                throw new ArgumentNullException("thisEvent");
            }

            thisEvent.ReplacePersistentCallArguments<PARAM>(persistentArgOverride);
            thisEvent.Invoke(arg0, arg1);
        }
        #endregion INVOKATION_METHODS
        
        #region FIELD_REPLACEMENT_METHODS
        /// <summary>
        /// Replace certain persistent call arguments with the specfied new value
        /// </summary>
        /// <typeparam name="OBJ">Type of call argument to require</typeparam>
        /// <param name="thisEvent">The unity event to modify</param>
        /// <param name="oldParam">The old argument to replace</param>
        /// <param name="newParam">The new argument to replace the old one with</param>
        public static void ReplacePersistentCallArguments<OBJ>(this UnityEventBase thisEvent, OBJ oldParam, OBJ newParam) {
            ReplacePersistentCallArguments(thisEvent, oldParam, newParam, SafeEqualityComparer<OBJ>.Default);
        }

        /// <summary>
        /// Replace certain persistent call arguments with the specfied new value
        /// </summary>
        /// <typeparam name="OBJ">Type of call argument to require</typeparam>
        /// <param name="thisEvent">The unity event to modify</param>
        /// <param name="oldParam">The old argument to replace</param>
        /// <param name="newParam">The new argument to replace the old one with</param>
        /// <param name="comparer">Equality comparer to evaluate arguments equality</param>
        public static void ReplacePersistentCallArguments<OBJ>(this UnityEventBase thisEvent, OBJ oldParam, OBJ newParam, IEqualityComparer<OBJ> comparer) {

            if(comparer == null) {
                throw new ArgumentNullException("comparer");
            }

            if(thisEvent == null) {
                throw new ArgumentNullException("thisEvent");
            }

            var callArray = thisEvent.GetPersistentCalls();
            for(int c=0; c<callArray.Length; ++c) {
                bool replace;
                var theseArgs = callArray[c].Arguments;
                var thisParam = theseArgs.GetArgument<OBJ>();
                
                if(ReferenceEquals(oldParam, null) && ReferenceEquals(thisParam, null)) {
                    replace = true;
                } else {
                    replace = comparer.Equals(thisParam, oldParam);
                }

                if(replace) {
                    theseArgs.SetArgument(newParam, callArray[c].ExpectedPersistentArgumentType);
                }
            }

            thisEvent.DirtyPersistentCalls();
        }

        /// <summary>
        /// Replace certain persistent call arguments with the specfied new value
        /// </summary>
        /// <typeparam name="OBJ">Type of call argument to require</typeparam>
        /// <param name="thisEvent">The unity event to modify</param>
        /// <param name="newParam">The new argument to replace the old one with</param>
        public static void ReplacePersistentCallArguments<OBJ>(this UnityEventBase thisEvent, OBJ newParam) {
            
            if(thisEvent == null) {
                throw new ArgumentNullException("thisEvent");
            }

            var callArray = thisEvent.GetPersistentCalls();
            for(int c=0; c<callArray.Length; ++c) {
                var expectedType = callArray[c].ExpectedPersistentArgumentType;
                callArray[c].Arguments.SetArgument(newParam, expectedType);
            }

            thisEvent.DirtyPersistentCalls();
        }
        
        /// <summary>
        /// Replace all method-info target references to an object with a new object reference
        /// </summary>
        /// <typeparam name="OBJ">Type of method target to require</typeparam>
        /// <param name="thisEvent">The unity event to modify</param>
        /// <param name="oldObject">Old method target to replace</param>
        /// <param name="newObject">The new method target</param>
        public static void ReplacePersistentCallTargets<OBJ>(this UnityEventBase thisEvent, OBJ oldObject, OBJ newObject)
          where OBJ : UnityEngine.Object {
            ReplacePersistentCallTargets(thisEvent, oldObject, newObject, SafeEqualityComparer<OBJ>.Default);
        }
        
        /// <summary>
        /// Replace all method-info target references to an object with a new object reference
        /// </summary>
        /// <typeparam name="OBJ">Type of method target to require</typeparam>
        /// <param name="thisEvent">The unity event to modify</param>
        /// <param name="oldObject">Old method target to replace</param>
        /// <param name="newObject">The new method target</param>
        /// <param name="comparer">Equality comparer to evaluate object equality</param>
        public static void ReplacePersistentCallTargets<OBJ>(this UnityEventBase thisEvent, OBJ oldObject, OBJ newObject, IEqualityComparer<OBJ> comparer)
          where OBJ : UnityEngine.Object {

            if(comparer == null) {
                throw new ArgumentNullException("comparer");
            }

            if(thisEvent == null) {
                throw new ArgumentNullException("thisEvent");
            }

            //get all the calls for this event
            var callArray = thisEvent.GetPersistentCalls();

            for(int c=0; c<callArray.Length; ++c) {
                bool replace;
                var thisTarget = callArray[c].Target;
                                        
                //if we specify to replace null target
                if((thisTarget == null) && (oldObject == null)) {
                    replace = true;

                //otherwise, if the current target is valid
                } else if(thisTarget != null && typeof(OBJ).IsAssignableFrom(thisTarget.GetType())) {
                    replace = comparer.Equals(thisTarget as OBJ, oldObject);
                } else {
                    replace = false;
                }
                
                if(replace) {
                    callArray[c].Target = newObject;
                }
            }
            
            //mark the persistent calls dirty
            thisEvent.DirtyPersistentCalls();
        }

        /// <summary>
        /// Clear all persistent calls from a UnityEvent
        /// </summary>
        public static void ClearPersistentCalls(this UnityEventBase thisEvent) {
            if(thisEvent == null) {
                throw new ArgumentNullException("thisEvent");
            }

            //set a new instance of the persistent call group
            _unityEvent_persistentCalls.SetValue(thisEvent, Activator.CreateInstance(_unityEvent_persistentCalls.FieldType));
            _unityEvent_dirtyPersistentCalls.Invoke(thisEvent, null);
        }

        /// <summary>
        /// Add a new persistent call to a UnityEvent
        /// </summary>
        /// <param name="method">Method to reference by the new persistent call</param>
        /// <param name="argument">Argument to pass to the method when invoking</param>
        public static void AddPersistentCall(this UnityEventBase thisEvent, UnityEngine.Object target, MethodInfo method, object argument) {
            if(thisEvent == null) throw new ArgumentNullException("thisEvent");

            var hackedCallGroup = thisEvent.GetPersistentCallGroup();
            hackedCallGroup.Add(null);

            if(method != null) {
                var calls = hackedCallGroup.Calls;
                if(calls != null && calls.Length > 0) {
                    calls[calls.Length-1].MethodName = method.Name;
                    calls[calls.Length-1].Arguments.SetArgument(argument);
                    calls[calls.Length-1].Target = target;
                }
            }
        }
        
        /// <summary>
        /// Replace all method-info target references to an object with a new object reference
        /// </summary>
        /// <typeparam name="OBJ">Type of method target to require</typeparam>
        /// <param name="thisEvent">The unity event to modify</param>
        /// <param name="newObject">The new method target</param>
        public static void ReplacePersistentCallTargets<OBJ>(this UnityEventBase thisEvent, OBJ newObject, bool requireTargetIsValid = true)
          where OBJ : UnityEngine.Object {

            if(thisEvent == null) {
                throw new ArgumentNullException("thisEvent");
            }

            //get all the calls for this event
            var callArray = thisEvent.GetPersistentCalls();

            for(int c=0; c<callArray.Length; ++c) {
                //set the target; this may set the actual target to something else (ie sub-component or parent gameObject)
                callArray[c].Target = newObject;
            }

            //mark the persistent calls dirty
            thisEvent.DirtyPersistentCalls();
        }

        /// <summary>
        /// Sets the method-info target on all listeners of this event to the specified target object
        /// </summary>
        /// <param name="thisEvent">UnityEvent to modify</param>
        /// <param name="newTarget">New target object to invoke all event calls on</param>
        public static void SetTargetOnAllPersistentCalls(this UnityEventBase thisEvent, UnityEngine.Object newTarget) {
            if(thisEvent != null) {
                using (ProfileUtil.PushSample("UnityEventUtil.SetTargetOnAllPersistentCalls", newTarget)) {
                    var persistentCallGroup = new HackedPersistentCallGroup(_unityEvent_persistentCalls.GetValue(thisEvent));
                    var callArray = persistentCallGroup.Calls;
                    for (int c = 0; c < callArray.Length; ++c) {
                        callArray[c].Target = newTarget;
                    }

                    //mark the persistent calls dirty
                    thisEvent.DirtyPersistentCalls();
                }
            }
        }
        #endregion FIELD_REPLACEMENT_METHODS
        #endregion STATIC_METHODS
    }
}
