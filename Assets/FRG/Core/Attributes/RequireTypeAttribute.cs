using System;

namespace FRG.Core
{
    /// <summary>
    /// Specifies what type to use with a reference field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class RequireTypeAttribute : InspectorPropertyAttribute
    {
        /// <summary>
        /// The type specified. Either this is provided or TypeName.
        /// </summary>
        public Type SpecifiedType { get; set; }

        /// <summary>
        /// Whether to hide the contents of the attached field in the inspector.
        /// </summary>
        public bool HideContents { get; set; }

        public override bool AppliesToEntireArray { get { return false; } }
        public override bool AppliesToArrayElements { get { return true; } }

        /// <summary>
        /// Requires the use of the specified type or subclasses.
        /// </summary>
        /// <param name="type">The type that is to be required.</param>
        public RequireTypeAttribute(Type type)
        {
            SpecifiedType = type;
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// Note: When using this attribute, do not grab the attribute directly from the field,
        /// but only through InspectorFieldStorage. You may need to look up the hierarchy to find the appropriate RequiredType.
        /// </summary>
        /// <param name="attrib"></param>
        /// <returns></returns>
        public static Type GetRequiredType(RequireTypeAttribute attrib)
        {
            if (attrib == null)
            {
                return null;
            }
            if (attrib.SpecifiedType != null)
            {
                return attrib.SpecifiedType;
            }
            return null;
        }
#endif
    }
}
