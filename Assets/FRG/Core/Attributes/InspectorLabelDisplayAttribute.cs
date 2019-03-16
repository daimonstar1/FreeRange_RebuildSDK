using System;

namespace FRG.Core
{
    /// <summary>
    /// An attribute that shows or hides the label of an inspector field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class InspectorLabelDisplayAttribute : InspectorPropertyAttribute
    {
        /// <summary>
        /// Whether to show the label of the field (not its elements). When the attribute is not specified, the default is true.
        /// </summary>
        public bool ShowLabel { get; private set; }

        /// <summary>
        /// The amount of indent to add to the child fields or (for lists) list elements. Defaults to 1 if label is shown, else 0.
        /// </summary>
        public int ChildIndent { get; set; }

        /// <summary>
        /// [Lists only.] Whether to show the labels of the field's list elements. When the attribute is not specified, the default is true for simple objects and false for compound objects.
        /// </summary>
        public bool ShowListElementLabels { get; private set; }

        /// <summary>
        /// [Lists only.] The amount of indent to add to the list elements' child fields. Defaults to 1 if list elements' labels are shown, else 0.
        /// </summary>
        public int ListElementChildIndent { get; set; }

        /// <summary>
        /// Specifies the proxy list field to use as the label of this field.
        /// </summary>
        public string FieldToUseAsLabel { get; set; }

        /// <summary>
        /// Specifies the proxy list field to use as the label of this field.
        /// </summary>
        public string FieldToUseAsListElementLabel { get; set; }

        /// <summary>
        /// Specifies a boolean value to use that can disable this field.
        /// </summary>
        public string FieldToUseAsBooleanToggle { get; set; }

        public override bool AppliesToArrayElements
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Shows or hides the label of an inspector field. May be ignored.
        /// </summary>
        /// <param name="showLabel">Whether to show the label of the field (not its elements). When the attribute is not specified, the default is true.</param>
        /// <param name="showListElementLabel">Whether to show the labels of the field's list elements. Only applicable to lists. When the attribute is not specified, the default is false.</param>
        public InspectorLabelDisplayAttribute(bool showLabel, bool showListElementLabel = true)
        {
            ShowLabel = showLabel;
            ChildIndent = showLabel ? 1 : 0;
            ShowListElementLabels = showListElementLabel;
            ListElementChildIndent = showListElementLabel ? 1 : 0;
        }

        public bool GetShowLabel(bool isListElement)
        {
            return isListElement ? ShowListElementLabels : ShowLabel;
        }

        public int GetChildFieldIndent(bool isListElement)
        {
            return isListElement ? ListElementChildIndent : ChildIndent;
        }
    }
}
