
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace FRG.Core
{
    /// <summary>
    /// A delegate-like object that takes any number of arguments, loosening them to a given type.
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct LooseDelegate : IEquatable<LooseDelegate>
    {
        #region Thread-Local Storage
        /// <summary>
        /// Thread-local storage specific to LooseDelegate.
        /// </summary>
        private class ArgumentListAllocator
        {
            private static LocalDataStoreSlot Slot;
            public readonly Stack<object[]> Stack = new Stack<object[]>();

            static ArgumentListAllocator()
            {
                Slot = Thread.AllocateDataSlot();
            }

            public static ArgumentListAllocator Get()
            {
                ArgumentListAllocator allocator = (ArgumentListAllocator)Thread.GetData(Slot);
                if (allocator == null)
                {
                    allocator = new ArgumentListAllocator();
                    Thread.SetData(Slot, allocator);
                }
                return allocator;
            }
        }
        #endregion

        #region Fields and Properties

        /// <summary>
        /// An empty loose delegate that does nothing and returns null (or default(T)).
        /// </summary>
        public static readonly LooseDelegate Empty = default(LooseDelegate);

        /// <summary>
        /// The culture used to convert strings, dates, etc by default. You may want to use LocaleManager.CurrentCulture at runtime.
        /// </summary>
        private static CultureInfo DefaultCulture { get { return CultureInfo.InvariantCulture; } }

        private readonly Delegate singleDelegate;
        private readonly Delegate[] multipleDelegates;
        private readonly CultureInfo culture;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new loose-casting delegate.
        /// </summary>
        /// <param name="del">The existing delegate to wrap.</param>
        /// <param name="culture">The optional culture to associate.</param>
        public LooseDelegate(Delegate del, CultureInfo culture = null)
            : this(del, culture, null)
        {
        }

        private LooseDelegate(Delegate singleDelegate, CultureInfo culture, Delegate[] additionalDelegates)
        {
            this.singleDelegate = singleDelegate;
            this.culture = culture ?? DefaultCulture;
            this.multipleDelegates = additionalDelegates;
        }

        /// <summary>
        /// Allows selection of culture on an existing FuncRef.
        /// </summary>
        /// <param name="culture">The CultureInfo to use with this FuncRef.</param>
        /// <returns>A new immutable FuncRef with the same delegate, but the given culture.</returns>
        public LooseDelegate WithCulture(CultureInfo culture)
        {
            return new LooseDelegate(this.singleDelegate, culture);
        }
        #endregion

        #region Typed Static Constructors
        /// <summary>
        /// Converts to a new loose-casting delegate.
        /// </summary>
        /// <param name="del">The existing delegate to wrap.</param>
        /// <param name="culture">The optional culture to associate.</param>
        public static LooseDelegate Create(Delegate del, CultureInfo culture = null)
        {
            return new LooseDelegate(del, culture, null);
        }

        /// <summary>
        /// Converts to a new loose-casting delegate.
        /// </summary>
        /// <param name="action">The existing delegate to wrap.</param>
        /// <param name="culture">The optional culture to associate.</param>
        public static LooseDelegate Create(Action action, CultureInfo culture = null)
        {
            return new LooseDelegate(action, culture, null);
        }

        /// <summary>
        /// Converts to a new loose-casting delegate.
        /// </summary>
        /// <param name="action">The existing delegate to wrap.</param>
        /// <param name="culture">The optional culture to associate.</param>
        public static LooseDelegate Create<T>(Action<T> action, CultureInfo culture = null)
        {
            return new LooseDelegate(action, culture, null);
        }

        /// <summary>
        /// Converts to a new loose-casting delegate.
        /// </summary>
        /// <param name="action">The existing delegate to wrap.</param>
        /// <param name="culture">The optional culture to associate.</param>
        public static LooseDelegate Create<T1, T2>(Action<T1, T2> action, CultureInfo culture = null)
        {
            return new LooseDelegate(action, culture, null);
        }

        /// <summary>
        /// Converts to a new loose-casting delegate.
        /// </summary>
        /// <param name="action">The existing delegate to wrap.</param>
        /// <param name="culture">The optional culture to associate.</param>
        public static LooseDelegate Create<T1, T2, T3>(Action<T1, T2, T3> action, CultureInfo culture = null)
        {
            return new LooseDelegate(action, culture, null);
        }

        /// <summary>
        /// Converts to a new loose-casting delegate.
        /// </summary>
        /// <param name="action">The existing delegate to wrap.</param>
        /// <param name="culture">The optional culture to associate.</param>
        public static LooseDelegate Create<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, CultureInfo culture = null)
        {
            return new LooseDelegate(action, culture, null);
        }

        /// <summary>
        /// Converts to a new loose-casting delegate.
        /// </summary>
        /// <param name="action">The existing delegate to wrap.</param>
        /// <param name="culture">The optional culture to associate.</param>
        public static LooseDelegate Create<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, CultureInfo culture = null)
        {
            return new LooseDelegate(action, culture, null);
        }

        /// <summary>
        /// Converts to a new loose-casting delegate.
        /// </summary>
        /// <param name="action">The existing delegate to wrap.</param>
        /// <param name="culture">The optional culture to associate.</param>
        public static LooseDelegate Create<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action, CultureInfo culture = null)
        {
            return new LooseDelegate(action, culture, null);
        }

        /// <summary>
        /// Converts to a new loose-casting delegate.
        /// </summary>
        /// <param name="function">The existing delegate to wrap.</param>
        /// <param name="culture">The optional culture to associate.</param>
        public static LooseDelegate Create<TResult>(Func<TResult> function, CultureInfo culture = null)
        {
            return new LooseDelegate(function, culture, null);
        }

        /// <summary>
        /// Converts to a new loose-casting delegate.
        /// </summary>
        /// <param name="function">The existing delegate to wrap.</param>
        /// <param name="culture">The optional culture to associate.</param>
        public static LooseDelegate Create<T, TResult>(Func<T, TResult> function, CultureInfo culture = null)
        {
            return new LooseDelegate(function, culture, null);
        }

        /// <summary>
        /// Converts to a new loose-casting delegate.
        /// </summary>
        /// <param name="function">The existing delegate to wrap.</param>
        /// <param name="culture">The optional culture to associate.</param>
        public static LooseDelegate Create<T1, T2, TResult>(Func<T1, T2, TResult> function, CultureInfo culture = null)
        {
            return new LooseDelegate(function, culture, null);
        }

        /// <summary>
        /// Converts to a new loose-casting delegate.
        /// </summary>
        /// <param name="function">The existing delegate to wrap.</param>
        /// <param name="culture">The optional culture to associate.</param>
        public static LooseDelegate Create<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> function, CultureInfo culture = null)
        {
            return new LooseDelegate(function, culture, null);
        }

        /// <summary>
        /// Converts to a new loose-casting delegate.
        /// </summary>
        /// <param name="function">The existing delegate to wrap.</param>
        /// <param name="culture">The optional culture to associate.</param>
        public static LooseDelegate Create<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> function, CultureInfo culture = null)
        {
            return new LooseDelegate(function, culture, null);
        }

        /// <summary>
        /// Converts to a new loose-casting delegate.
        /// </summary>
        /// <param name="function">The existing delegate to wrap.</param>
        /// <param name="culture">The optional culture to associate.</param>
        public static LooseDelegate Create<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> function, CultureInfo culture = null)
        {
            return new LooseDelegate(function, culture, null);
        }

        /// <summary>
        /// Converts to a new loose-casting delegate.
        /// </summary>
        /// <param name="function">The existing delegate to wrap.</param>
        /// <param name="culture">The optional culture to associate.</param>
        public static LooseDelegate Create<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> function, CultureInfo culture = null)
        {
            return new LooseDelegate(function, culture, null);
        }
        #endregion

        #region Invoke/Evaluate
        /// <summary>
        /// Invokes the delegate with no arguments. Any unprovided arguments will use default values.
        /// </summary>
        /// <returns>
        /// Invoked delegate return type, but does no converting. May return null or something unexpected. Null on a default-constructed LooseDelegate.
        /// </returns>
        public void Invoke()
        {
            Invoke((object[])null);
        }

        /// <summary>
        /// Invokes the delegate with a variable list of arguments. Any unprovided arguments will use default values.
        /// </summary>
        /// <param name="arguments">The arguments to invoke with.</param>
        /// <returns>
        /// Invoked delegate return type, but does no converting. May return null or something unexpected. Null on a default-constructed LooseDelegate.
        /// </returns>
        public void Invoke(params object[] arguments)
        {
            EvaluateUnchecked(arguments);
        }

        /// <summary>
        /// Invokes the delegate with no arguments. Any unprovided arguments will use default values.
        /// </summary>
        /// <param name="returnType">The type to loosen the return value to.</param>
        /// <returns>
        /// Invoked delegate return type, converted to return type, if possible. Will return default value on a default-constructed LooseDelegate.
        /// Does not return null for value types.
        /// </returns>
        public object Evaluate(Type returnType)
        {
            object result = EvaluateUnchecked((object[])null);
            result = LoosenResult(result, returnType);
            return result;
        }

        /// <summary>
        /// Invokes the delegate with a variable list of arguments. Any unprovided arguments will use default values.
        /// </summary>
        /// <param name="returnType">The type to loosen the return value to.</param>
        /// <param name="arguments">The arguments to invoke with.</param>
        /// <returns>
        /// Invoked delegate return type, converted to return type, if possible. Will return default value on a default-constructed LooseDelegate.
        /// Does not return null for value types.
        /// </returns>
        public object Evaluate(Type returnType, params object[] arguments)
        {
            object result = EvaluateUnchecked(arguments);
            result = LoosenResult(result, returnType);
            return result;
        }

        /// <summary>
        /// Invokes the delegate no arguments. Any unprovided arguments will use default values.
        /// </summary>
        /// <typeparam name="T">The type to loosen the return value to.</typeparam>
        /// <returns>
        /// Invoked delegate return type, converted to return type, if possible. Will return default value on a default-constructed LooseDelegate.
        /// </returns>
        public T Evaluate<T>()
        {
            return (T)Evaluate(typeof(T), (object[])null);
        }

        /// <summary>
        /// Invokes the delegate with a variable list of arguments. Any unprovided arguments will use default values.
        /// </summary>
        /// <typeparam name="T">The type to loosen the return value to.</typeparam>
        /// <param name="arguments">The arguments to invoke with.</param>
        /// <returns>
        /// Invoked delegate return type, converted to return type, if possible. Will return default value on a default-constructed LooseDelegate.
        /// </returns>
        public T Evaluate<T>(params object[] arguments)
        {
            return (T)Evaluate(typeof(T), arguments);
        }

        private object EvaluateUnchecked(object[] arguments)
        {
            if (singleDelegate == null && multipleDelegates == null)
            {
                return null;
            }

            ArgumentListAllocator allocator = ArgumentListAllocator.Get();
            allocator.Stack.Push(arguments);
            try
            {
                return Execute(arguments);
            }
            finally
            {
                allocator.Stack.Pop();
            }
        }
        #endregion

        #region ChainInvoke/ChainEvaluate
        /// <summary>
        /// Invokes the delegate inherting the current floating set of arguments.
        /// </summary>
        /// <returns>
        /// Invoked delegate return type, but does no converting. May return null or something unexpected. Null on a default-constructed LooseDelegate.
        /// </returns>
        public object ChainInvoke()
        {
            // Default constructor is valid on structs
            if (singleDelegate == null && multipleDelegates == null)
            {
                return null;
            }

            ArgumentListAllocator allocator = ArgumentListAllocator.Get();
            object[] args = null;
            if (allocator.Stack.Count > 0)
            {
                args = allocator.Stack.Peek();
            }
            else
            {
                throw new InvalidOperationException("Attempting to chain a LooseDelegate before invoking any on the stack.");
            }

            return Execute(args);
        }

        /// <summary>
        /// Invokes the delegate inherting the current floating set of arguments.
        /// </summary>
        /// <param name="returnType">The type to loosen the return value to.</param>
        /// <returns>
        /// Invoked delegate return type, loosened to return type, if possible. Will return default value on a default-constructed LooseDelegate.
        /// Does not return null for value types.
        /// </returns>
        public object ChainEvaluate(Type returnType)
        {
            object result = ChainInvoke();
            result = LoosenResult(result, returnType);
            return result;
        }

        /// <summary>
        /// Invokes the delegate inherting the current floating set of arguments.
        /// </summary>
        /// <typeparam name="T">The type to loosen the return value to.</typeparam>
        /// <returns>
        /// Invoked delegate return type, loosened to return type, if possible. Will return default value on a default-constructed LooseDelegate.
        /// </returns>
        public T ChainEvaluate<T>()
        {
            return (T)ChainEvaluate(typeof(T));
        }
        #endregion

        #region Implementation
        private object Execute(object[] arguments)
        {
            object result = null;
            if (singleDelegate != null)
            {
                result = Execute_Single(singleDelegate, arguments, culture);
            }
            if (multipleDelegates != null)
            {
                foreach (Delegate del in multipleDelegates)
                {
                    result = Execute_Single(del, arguments, culture);
                }
            }
            return result;
        }

        private static object Execute_Single(Delegate currentDelegate, object[] arguments, CultureInfo currentCulture)
        {
            MethodInfo method = currentDelegate.Method;
            if (method == null)
            {
                throw new ArgumentException("Could not run delegate \"" + currentDelegate.ToString() + "\" because it specifies a null Method.");
            }

            // Figure out the target for instance methods
            int startingArgument = 0;
            object target = null;
            if (!method.IsStatic)
            {
                target = currentDelegate.Target;
                if (target == null)
                {
                    startingArgument = 1;
                    target = (arguments != null && arguments.Length > 0 ? arguments[0] : null);

                    if (!method.DeclaringType.IsInstanceOfType(target))
                    {
                        throw new TargetException("Cannot call " + method.ToString() + " with " + (target != null ? target.ToString() : "<null>") + " as the target parameter.");
                    }
                }
            }

            object[] adjustedArguments;
            ParameterInfo[] parameters = method.GetParameters();
            int parameterCount = parameters.Length;
            int argumentCount = (arguments != null ? arguments.Length : 0);
            int copyLength = Math.Min(argumentCount - startingArgument, parameterCount);

            // Readjust array to correct number of arguments, filling leftover entries with null
            if (parameterCount != argumentCount || startingArgument != 0)
            {
                adjustedArguments = new object[parameterCount];
                if (arguments != null) Array.Copy(arguments, startingArgument, adjustedArguments, 0, copyLength);
                // rest are null
            }
            // Simple optimization
            else
            {
                adjustedArguments = arguments;
            }

            // Watch null arrays
            if (parameterCount > 0)
            {
                for (int i = 0; i < adjustedArguments.Length; ++i)
                {
                    adjustedArguments[i] = LooseExtensions.LooseCast(adjustedArguments[i], parameters[i].ParameterType);
                }
            }

            return method.Invoke(target, adjustedArguments);
        }

        private object LoosenResult(object result, Type returnType)
        {
            return LooseExtensions.LooseCast(result, returnType, culture);
        }
        #endregion

        #region Conversions
        public Action ToAction()
        {
            LooseDelegate action = this;
            return () => action.Invoke();
        }

        public Action<T1> ToAction<T1>()
        {
            LooseDelegate func = this;
            return (arg1) => func.Invoke(arg1);
        }

        public Action<T1, T2> ToAction<T1, T2>()
        {
            LooseDelegate func = this;
            return (arg1, arg2) => func.Invoke(arg1, arg2);
        }

        public Action<T1, T2, T3> ToAction<T1, T2, T3>()
        {
            LooseDelegate func = this;
            return (arg1, arg2, arg3) => func.Invoke(arg1, arg2, arg3);
        }

        public Action<T1, T2, T3, T4> ToAction<T1, T2, T3, T4>()
        {
            LooseDelegate func = this;
            return (arg1, arg2, arg3, arg4) => func.Invoke(arg1, arg2, arg3, arg4);
        }
        public Action<T1, T2, T3, T4, T5> ToAction<T1, T2, T3, T4, T5>()
        {
            LooseDelegate func = this;
            return (arg1, arg2, arg3, arg4, arg5) => func.Invoke(arg1, arg2, arg3, arg4, arg5);
        }
 
        public Action<T1, T2, T3, T4, T5, T6> ToAction<T1, T2, T3, T4, T5, T6>()
        {
            LooseDelegate func = this;
            return (arg1, arg2, arg3, arg4, arg5, arg6) => func.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
        }

        public Func<TResult> ToFunc<TResult>()
        {
            return Evaluate<TResult>;
        }

        public Func<T1, TResult> ToFunc<T1, TResult>()
        {
            LooseDelegate func = this;
            return (arg1) => func.Evaluate<TResult>(arg1);
        }

        public Func<T1, T2, TResult> ToFunc<T1, T2, TResult>()
        {
            LooseDelegate func = this;
            return (arg1, arg2) => func.Evaluate<TResult>(arg1, arg2);
        }

        public Func<T1, T2, T3, TResult> ToFunc<T1, T2, T3, TResult>()
        {
            LooseDelegate func = this;
            return (arg1, arg2, arg3) => func.Evaluate<TResult>(arg1, arg2, arg3);
        }

        public Func<T1, T2, T3, T4, TResult> ToFunc<T1, T2, T3, T4, TResult>()
        {
            LooseDelegate func = this;
            return (arg1, arg2, arg3, arg4) => func.Evaluate<TResult>(arg1, arg2, arg3, arg4);
        }

        public Func<T1, T2, T3, T4, T5, TResult> ToFunc<T1, T2, T3, T4, T5, TResult>()
        {
            LooseDelegate func = this;
            return (arg1, arg2, arg3, arg4, arg5) => func.Evaluate<TResult>(arg1, arg2, arg3, arg4, arg5);
        }

        public Func<T1, T2, T3, T4, T5, T6, TResult> ToFunc<T1, T2, T3, T4, T5, T6, TResult>()
        {
            LooseDelegate func = this;
            return (arg1, arg2, arg3, arg4, arg5, arg6) => func.Evaluate<TResult>(arg1, arg2, arg3, arg4, arg5, arg6);
        }


        public override string ToString()
        {
            if (multipleDelegates != null)
            {
                return "LooseDelegate[multiple wrapped delegates]";
            }
            else if (singleDelegate != null)
            {
                return "LooseDelegate[" + singleDelegate.ToString() + "]";
            }
            else
            {
                return "";
            }
        }
        #endregion

        #region Equality
        public bool Equals(LooseDelegate other)
        {
            return (singleDelegate == other.singleDelegate && multipleDelegates == other.multipleDelegates && culture == other.culture);
        }

        public override bool Equals(object other)
        {
            if (!(other is LooseDelegate))
            {
                return false;
            }
            return Equals((LooseDelegate)other);
        }

        // NOTE: Not really intended to be used as a hash key.
        public override int GetHashCode()
        {
            int hash = (singleDelegate != null ? singleDelegate.GetHashCode() : 0);
            hash = BitUtil.CombineHashCodes(hash, culture != null ? culture.GetHashCode() : 0);
            hash = BitUtil.CombineHashCodes(hash, multipleDelegates != null ? multipleDelegates.Length : 0);
            return hash;
        }

        public static bool operator ==(LooseDelegate a, LooseDelegate b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(LooseDelegate a, LooseDelegate b)
        {
            return !a.Equals(b);
        }

        #endregion

        #region Combining
        public static LooseDelegate Combine(LooseDelegate a, LooseDelegate b)
        {
            CultureInfo culture = a.culture ?? b.culture;
            Delegate single;
            Delegate[] multiple;
            if (a.singleDelegate == null && a.multipleDelegates == null)
            {
                single = b.singleDelegate;
                multiple = b.multipleDelegates;
            }
            else if (b.singleDelegate == null && (a.multipleDelegates == null || b.multipleDelegates == null))
            {
                single = a.singleDelegate;
                multiple = a.multipleDelegates ?? b.multipleDelegates;
            }
            else
            {
                single = a.singleDelegate;

                int multipleCount = 0;
                if (a.multipleDelegates != null)
                {
                    multipleCount += a.multipleDelegates.Length;
                }
                if (b.singleDelegate != null)
                {
                    multipleCount += 1;
                }
                if (b.multipleDelegates != null)
                {
                    multipleCount += b.multipleDelegates.Length;
                }

                multiple = new Delegate[multipleCount];
                int index = 0;
                if (a.multipleDelegates != null)
                {
                    a.multipleDelegates.CopyTo(multiple, index);
                    index += a.multipleDelegates.Length;
                }
                if (b.singleDelegate != null)
                {
                    multiple[index] = b.singleDelegate;
                    index += 1;
                }
                if (b.multipleDelegates != null)
                {
                    b.multipleDelegates.CopyTo(multiple, index);
                    index += b.multipleDelegates.Length;
                }
            }

            return new LooseDelegate(single, culture, multiple);
        }

        public static LooseDelegate operator +(LooseDelegate a, LooseDelegate b)
        {
            return Combine(a, b);
        }

        public static LooseDelegate operator +(LooseDelegate a, Delegate b)
        {
            return Combine(a, new LooseDelegate(b));
        }

        public static LooseDelegate operator +(Delegate a, LooseDelegate b)
        {
            return Combine(new LooseDelegate(a), b);
        }
        #endregion

        #region Removing

        private static bool NeedRemove(Delegate removeFrom, LooseDelegate loose)
        {
            if (loose.singleDelegate != null)
            {
                Delegate result = Delegate.Remove(removeFrom, loose.singleDelegate);
                if (result != removeFrom)
                {
                    return true;
                }
            }
            if (loose.multipleDelegates != null)
            {
                foreach (Delegate toRemove in loose.multipleDelegates)
                {
                    Delegate result = Delegate.Remove(removeFrom, toRemove);
                    if (result != removeFrom)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool NeedRemove(LooseDelegate removeFrom, LooseDelegate loose)
        {
            if (removeFrom.singleDelegate != null)
            {
                if (NeedRemove(removeFrom.singleDelegate, loose))
                {
                    return true;
                }
            }
            if (removeFrom.multipleDelegates != null)
            {
                foreach (Delegate entry in removeFrom.multipleDelegates)
                {
                    if (NeedRemove(entry, loose))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static Delegate RemoveAndIntersect(Delegate removeFrom, List<Delegate> intersection)
        {
            for (int i = 0; i < intersection.Count; ++i)
            {
                Delegate entry = intersection[i];
                Delegate result = Delegate.Remove(removeFrom, entry);
                if (result != removeFrom)
                {
                    Delegate entryIntersect = Delegate.Remove(entry, removeFrom);
                    if (entryIntersect == null)
                    {
                        intersection.RemoveAt(i);
                        i -= 1;
                    }

                    removeFrom = result;
                    if (removeFrom == null)
                    {
                        break;
                    }
                }
            }
            return removeFrom;
        }

        public static LooseDelegate Remove(LooseDelegate from, LooseDelegate loose)
        {
            if (from.multipleDelegates != null)
            {
                // optimization: Don't allocate on removal unless needed.
                if (!NeedRemove(from, loose))
                {
                    return from;
                }

                List<Delegate> resultList = new List<Delegate>();
                List<Delegate> delegateList = from.GetInvocationList();
                List<Delegate> looseList = loose.GetInvocationList();
                for (int i = 0; i < delegateList.Count; ++i)
                {
                    Delegate result = RemoveAndIntersect(delegateList[i], looseList);
                    if (result != null)
                    {
                        resultList.Add(result);
                    }
                }

                if (resultList.Count == 0)
                {
                    return new LooseDelegate(null, from.culture, null);
                }
                else if (resultList.Count == 1)
                {
                    return new LooseDelegate(resultList[0], from.culture, null);
                }
                else
                {
                    return new LooseDelegate(null, from.culture, resultList.ToArray());
                }
            }
            else
            {
                Delegate newSingle = from.singleDelegate;

                if (newSingle != null && loose.singleDelegate != null)
                {
                    newSingle = Delegate.Remove(newSingle, loose.singleDelegate);
                }
                if (loose.multipleDelegates != null)
                {
                    for (int i = 0; newSingle != null && i < loose.multipleDelegates.Length; ++i)
                    {
                        newSingle = Delegate.Remove(newSingle, loose.multipleDelegates[i]);
                    }
                }
                return new LooseDelegate(newSingle, from.culture);
            }
        }

        public static LooseDelegate Remove(LooseDelegate from, Delegate del)
        {
            return Remove(from, new LooseDelegate(del));
        }

        public static LooseDelegate operator -(LooseDelegate a, LooseDelegate b)
        {
            return Remove(a, b);
        }

        public static LooseDelegate operator -(LooseDelegate a, Delegate b)
        {
            return Remove(a, b);
        }

        public static explicit operator LooseDelegate(Delegate del)
        {
            return new LooseDelegate(del, null);
        }
        #endregion

        #region Introspection
        public List<Delegate> GetInvocationList()
        {
            List<Delegate> list = new List<Delegate>(multipleDelegates != null ? multipleDelegates.Length + 1 : 1);
            if (singleDelegate != null)
            {
                list.Add(singleDelegate);
            }
            if (multipleDelegates != null)
            {
                list.AddRange(multipleDelegates);
            }
            return list;
        }
        #endregion
    }
}
