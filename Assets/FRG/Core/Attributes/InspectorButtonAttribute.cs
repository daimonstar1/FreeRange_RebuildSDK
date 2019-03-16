using System;
using UnityEngine;

namespace FRG.Core
{

    /// <summary>
    /// Base class for attributes that extend the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class InspectorButtonAttribute : InspectorPropertyAttribute
    {
        /// <summary>
        /// The text to show on the button. Defaults to the (demangled) method name.
        /// </summary>
        public string ButtonText { get; private set; }

        /// <summary>
        /// The name of the method to call upon pressing the button.
        /// </summary>
        public string MethodName { get; private set; }

        /// <summary>
        /// The optional tooltip text to include for the button.
        /// </summary>
        public string Tooltip { get; set; }

        /// <summary>
        /// Whether to treat the object as dirty after calling. (Defaults to true.)
        /// </summary>
        public bool DirtyObject { get; set; }

        public string ValidationMethodName { get; set; }


        private GUIContent content;
        public GUIContent Content
        {
            get
            {
                if (content == null)
                {
                    content = new GUIContent(ButtonText, Tooltip);
                }
                return content;
            }
        }


        public override bool AppliesToEntireArray { get { return true; } }
        public override bool AppliesToArrayElements { get { return false; } }

        /// <summary>
        /// Creates a new attribute.
        /// </summary>
        /// <param name="methodName">The name of the method to call.</param>
        public InspectorButtonAttribute(string methodName)
            : this(methodName, ReflectionUtil.GetInspectorDisplayName(methodName))

        {
        }

        /// <summary>
        /// Creates a new attribute.
        /// </summary>
        /// <param name="methodName">The name of the method to call.</param>
        /// <param name="buttonText">The text of the button.</param>
        public InspectorButtonAttribute(string methodName, string buttonText)
        {
            this.MethodName = methodName;
            this.ButtonText = buttonText;
            this.DirtyObject = true;
        }
    }
}