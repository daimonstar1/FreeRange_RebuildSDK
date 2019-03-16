namespace FRG.Core
{
    /// <summary>
    /// Exposes this field to the deep copier.
    /// </summary>
    public sealed class InspectorDeepCopyAttribute : InspectorPropertyAttribute
    {
        /// <summary>
        /// Includes this field in the deep copier.
        /// Defaults to true for <see cref="ManagedEntry"/> and false for everything else.
        /// </summary>
        public bool Include { get; set; }
        public bool CapsName { get; set; }
        public bool SkipSubfields { get; set; }

        /// <summary>
        /// Exposes this field to the deep copier.
        /// </summary>
        public InspectorDeepCopyAttribute()
        {
            Include = true;
        }
    }
}