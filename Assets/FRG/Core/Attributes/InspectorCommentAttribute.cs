using System;

namespace FRG.Core
{

    /// <summary>
    /// The type of comment icon.
    /// </summary>
    public enum InspectorCommentIcon
    {

        /// <summary>
        /// Specifies no icon.
        /// </summary>
        None,

        /// <summary>
        /// Specifies an info icon.
        /// </summary>
        Info,

        /// <summary>
        /// Specifies a warning icon.
        /// </summary>
        Warning,

        /// <summary>
        /// Specifies an error icon.
        /// </summary>
        Error
    }

    /// <summary>
    /// An attribute that adds a comment to the inspector in a little help box.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class InspectorCommentAttribute : InspectorPropertyAttribute
    {
        /// <summary>
        /// The comment to add.
        /// </summary>
        public string CommentText { get; set; }

        /// <summary>
        /// The method to call that determines what message to write.
        /// If there is no message, the box will not display.
        /// </summary>
        public string CommentTextMethodName { get; set; }

        /// <summary>
        /// The style of the icon to show.
        /// </summary>
        public InspectorCommentIcon Icon { get; set; }

        /// <summary>
        /// The method to call that chooses whether to show the message.
        /// </summary>
        public string ConditionMethodName { get; set; }

        public bool IgnoreDisabledState { get; set; }

        public override bool AppliesToEntireArray { get { return true; } }
        public override bool AppliesToArrayElements { get { return false; } }

        /// <summary>
        /// Specifies a comment box to attach above a field in the inspector.
        /// </summary>
        public InspectorCommentAttribute()
        {
            Icon = InspectorCommentIcon.Info;
        }
    }
}