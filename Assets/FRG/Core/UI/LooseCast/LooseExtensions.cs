using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace FRG.Core
{
    public static class LooseExtensions
    {
        /// <summary>
        /// Extension method that loosens a delegate's constraints, returning a LooseDelegate.
        /// </summary>
        /// <param name="del">The delegate to loosen.</param>
        /// <returns>A LooseDelegate with the default CultureInfo.</returns>
        public static LooseDelegate ToLooseDelegate(this Delegate del)
        {
            return new LooseDelegate(del);
        }

        /// <summary>
        /// Loosely cast a value to a given type, doing numeric, string, implicit and explicit conversions.
        /// </summary>
        /// <typeparam name="T">The type to cast to.</typeparam>
        /// <param name="value">The value to cast.</param>
        /// <returns>The cast value.</returns>
        /// <exception cref="System.InvalidCastException">The given value could not be cast to the given type.</exception>
        public static T LooseCast<T>(this object value)
        {
            if (value is T)
            {
                return (T)value;
            }

            return LooseCast<T>(value, null);
        }

        /// <summary>
        /// Loosely cast a value to a given type, doing numeric, string, implicit and explicit conversions.
        /// </summary>
        /// <typeparam name="T">The type to cast to.</typeparam>
        /// <param name="value">The value to cast.</param>
        /// <param name="culture">The culture to use to conver the value.</param>
        /// <returns>The cast value.</returns>
        /// <exception cref="System.InvalidCastException">The given value could not be cast to the given type.</exception>
        public static T LooseCast<T>(this object value, CultureInfo culture)
        {
            if (value is T)
            {
                return (T)value;
            }

            return (T)LooseCast(value, typeof(T), culture);
        }

        /// <summary>
        /// Loosely cast a value to a given type, doing numeric, string, implicit and explicit conversions.
        /// </summary>
        /// <param name="castType">The type to cast to.</param>
        /// <param name="value">The value to cast.</param>
        /// <returns>The cast value.</returns>
        /// <exception cref="System.InvalidCastException">The given value could not be cast to the given type.</exception>
        public static object LooseCast(this object value, Type castType)
        {
            return LooseCast(value, castType, null);
        }

        /// <summary>
        /// Loosely cast a value to a given type, doing numeric, string, implicit and explicit conversions.
        /// </summary>
        /// <param name="castType">The type to cast to.</param>
        /// <param name="value">The value to cast.</param>
        /// <param name="culture">The culture to use to conver the value.</param>
        /// <returns>The cast value.</returns>
        /// <exception cref="System.InvalidCastException">The given value could not be cast to the given type.</exception>
        public static object LooseCast(this object value, Type castType, CultureInfo culture)
        {
            // Default to standard C# cast behavior
            object result;
            if (ReflectionUtil.TryRuntimeCast(value, castType, out result))
            {
                return result;
            }

            // Let null mean default(value)
            if (value == null)
            {
                if (castType.IsValueType)
                {
                    value = Activator.CreateInstance(castType);
                }
                return value;
            }

            // Lots of built-in types can use IConvertible.
            if (value is IConvertible && IsConvertibleTarget(castType))
            {
                return System.Convert.ChangeType(value, castType, culture);
            }

            Type valueType = value.GetType();
            throw new InvalidCastException("Could not loosely convert the value of type \"" + ReflectionUtil.CSharpFullName(valueType) + "\" to " +
                "\"" + ReflectionUtil.CSharpFullName(castType) + "\".");
        }

        /// <summary>
        /// Returns true if <param name="type" /> is one of the types <see cref="IConvertible"/> can convert to.
        /// </summary>
        private static bool IsConvertibleTarget(Type type)
        {
            TypeCode code = Type.GetTypeCode(type);
            switch (code)
            {
                case TypeCode.Empty:
                case TypeCode.Object:
                case TypeCode.DBNull:
                    return false;
                default:
                    return true;
            }
        }
    }
}
