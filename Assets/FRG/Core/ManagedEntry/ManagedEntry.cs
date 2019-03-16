using System;
using UnityEngine;

namespace FRG.Core
{
    /// <summary>
    /// A simple data class that is kept in a manager.
    /// </summary>
    [InspectorShowContents, Serializable]
    public abstract partial class ManagedEntry : UnityEngine.ScriptableObject
    {
        /// <summary>
        /// Whether a ManagedEntry is stable.
        /// </summary>
        /// <remarks>
        /// NOTE: Autocalculate results will not be returned from a GetAll.
        /// </remarks>
        [InspectorHide, SerializeField]
        StableOption _stableOption = StableOption.Autocalculate;

        public StableOption StableOption 
        {
            get {
                return _stableOption;
            }
        }

#if UNITY_EDITOR
        public static void SetStableOption(ManagedEntry managedEntry, StableOption stableOption)
        {
            managedEntry._stableOption = stableOption;
        }
#endif

        /// <summary>
        /// When false, this asset won't be built or returned from <see cref="EntryManager"/>.
        /// </summary>
        /// <remarks>Should be relatively cheap because it is checked a lot.</remarks>
        public virtual bool IsValid { get { return true; } }
    }
}
