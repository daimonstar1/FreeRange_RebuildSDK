using System;

namespace FRG.Core
{
    /// <summary>
    /// An attribute that specifies where to place a singleton.
    /// Used with the <see cref="ServiceLocator"/> class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class ServiceOptionsAttribute : Attribute
    {
        /// <summary>
        /// The name of the gameobject or folder to place this singleton within.
        /// Only used for <see cref="ServiceLocator.ResolveRuntime{T}"/> and <see cref="ServiceLocator.ResolveAsset{T}"/>.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Specifes whether this singleton should be only allowed in the editor.
        /// </summary>
        public bool IsEditorOnly { get; set; }
        
        public ServiceOptionsAttribute()
        {
        }
    }
}
