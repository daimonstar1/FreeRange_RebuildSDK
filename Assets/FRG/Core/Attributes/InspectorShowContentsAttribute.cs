using System;

namespace FRG.Core
{
    /// <summary>
    /// Specifies whether to show the contents of a field. Only valid on <see cref="UnityEngine.Object"/> references.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class InspectorShowContentsAttribute : InspectorPropertyAttribute
    {
        /// <summary>
        /// Whether to show contents. Defaults to true.
        /// </summary>
        public bool ShowContents { get; set; }

        /// <summary>
        /// The maximum number of indents before preventing Showcontents. Defaults to 8.
        /// </summary>
        /// <remarks>This value only applies to the current field, not its children.</remarks>
        public int MaxIndent { get; set; }

        /// <summary>
        /// If provided, invokes to figure out what type to restrict the field to.
        /// </summary>
        public string TypeMethod { get; set; }

        /// <summary>
        /// Suppress the bars and warning text.
        /// </summary>
        public bool SuppressWarning { get; set; }

        public override bool AppliesToEntireArray { get { return false; } }
        public override bool AppliesToArrayElements { get { return true; } }

        /// <summary>
        /// Shows the contents of an object reference.
        /// </summary>
        public InspectorShowContentsAttribute()
        {
            ShowContents = true;
            MaxIndent = 8;
        }
    }
}
