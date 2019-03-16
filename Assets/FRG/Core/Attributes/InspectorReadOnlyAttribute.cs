using System;

namespace FRG.Core
{
    /// <summary>
    /// Marks a field as read only.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public sealed class InspectorReadOnlyAttribute : InspectorPropertyAttribute
    {
        /// <summary>
        /// The name of the optional condition to use for when to not make it read only.
        /// </summary>
        public string ExemptionMethodName { get; private set; }

        public override bool AppliesToEntireArray { get { return true; } }
        public override bool AppliesToArrayElements { get { return true; } }

        /// <summary>
        /// Marks a field as read only.
        /// </summary>
        public InspectorReadOnlyAttribute() { }

        /// <summary>
        /// Marks a field as read only, unless a method with the specified name returns true.
        /// </summary>
        /// <param name="exemptionMethodName">The name of the method on the containing object, that takes no parameters and returns a boolean.</param>
        public InspectorReadOnlyAttribute(string exemptionMethodName)
        {
            ExemptionMethodName = exemptionMethodName;
        }
    }
}