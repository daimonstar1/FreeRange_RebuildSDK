using System;
using System.Diagnostics;
using UnityEngine;

namespace FRG.Core
{
    /// <summary>
    /// Base class for attributes that extend the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum), Conditional("UNITY_EDITOR")]
    public abstract class InspectorPropertyAttribute : PropertyAttribute
    {
        /// <summary>
        /// Whether to apply the attribute to an entire array.
        /// </summary>
        public virtual bool AppliesToEntireArray { get { return true; } }

        /// <summary>
        /// Whether to apply the attribute to an element in an array.
        /// </summary>
        public virtual bool AppliesToArrayElements { get { return false; } }

        public static bool ShouldApply(PropertyAttribute attribute, bool isArray, bool isArrayElement)
        {
            if (isArray || isArrayElement) {
                InspectorPropertyAttribute cast = attribute as InspectorPropertyAttribute;
                if (cast != null) {
                    if (isArray && !cast.AppliesToEntireArray) {
                        return false;
                    }
                    if (isArrayElement && !cast.AppliesToArrayElements) {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}