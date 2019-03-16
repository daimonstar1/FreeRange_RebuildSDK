using System;

namespace FRG.Core
{

    /// <summary>
    /// Hides this field in the inspector. (Works on enum fields as well, but methods are not invoked.)
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public sealed class InspectorHideAttribute : InspectorPropertyAttribute
    {
        /// <summary>
        /// The name of the optional condition to use for when to not make it hidden.
        /// </summary>
        /// <remarks>Ignored for enum fields.</remarks>
        public string ExemptionMethodName { get; private set; }

        public override bool AppliesToEntireArray { get { return true; } }
        public override bool AppliesToArrayElements { get { return true; } }

        /// <summary>
        /// Hides this field in the inspector.
        /// </summary>
        public InspectorHideAttribute()
        {
        }
        
        /// <summary>
        /// Hides this field in the inspector depending on the specified exemption methods
        /// </summary>
        /// <param name="exemptionMethodNames">The name of the optional condition to use for when to not make it hidden.</param>
        public InspectorHideAttribute(string exemptionMethodName)
        {
            ExemptionMethodName = exemptionMethodName;
        }
    }
}