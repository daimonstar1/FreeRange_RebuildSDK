using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FRG.Core
{
    /// <summary>
    /// An indirect lightweight reference to an asset. (Wraps a name.)
    /// </summary>
    [Serializable]
    [RequireType(typeof(UnityEngine.Object), order = int.MaxValue)]
    [StructLayout(LayoutKind.Auto)]
    public struct AssetManagerRef : IEquatable<AssetManagerRef>
    {
        /// <summary>
        /// Prefix for a main prefab (GameObject)
        /// </summary>
        public const string UniquePrefix = "zx_";
        public const int UuidLength = 32;
        public const char SeparatorChar = '+';

        [SerializeField]
        private string uniqueId;

        public string UniqueId { get { return uniqueId ?? ""; } } 

        /// <summary>
        /// Determines if this entry is not empty.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(uniqueId);
            }
        }

        /// <summary>
        /// Creates a new ref with all components.
        /// </summary>
        public AssetManagerRef(string uniqueId)
        {
            if (string.IsNullOrEmpty(uniqueId)) {
                this.uniqueId = "";
            }
            else if (!IsValidUniqueId(uniqueId)) {
                throw new ArgumentException("Invalid AssetManagerRef. You should not be creating these manually.", "uniqueId");
            }
            else {
                this.uniqueId = uniqueId;
            }
        }

        /// <summary>
        /// Compares two refs for equality.
        /// </summary>
        /// <param name="left">A ref.</param>
        /// <param name="right">A ref.</param>
        /// <returns>true if equal, else false.</returns>
        public static bool operator ==(AssetManagerRef left, AssetManagerRef right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two refs for inequality.
        /// </summary>
        /// <param name="left">A ref.</param>
        /// <param name="right">A ref.</param>
        /// <returns>true if not equal, else false.</returns>
        public static bool operator !=(AssetManagerRef left, AssetManagerRef right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Compares against a ref for equality.
        /// </summary>
        /// <param name="value">A ref.</param>
        /// <returns>true if equal, else false.</returns>
        public bool Equals(AssetManagerRef value)
        {
            return string.Equals(uniqueId ?? "", value.uniqueId ?? "", StringComparison.Ordinal);
        }

        /// <summary>
        /// Compares against an object for equality.
        /// </summary>
        /// <param name="obj">An object.</param>
        /// <returns>true if equal, else false.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is AssetManagerRef))
                return false;
            return this == (AssetManagerRef)obj;
        }

        /// <summary>
        /// Gets a hash code for this object.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            if (!IsValid) { return 0; }

            return uniqueId.GetHashCode();
        }

        /// <summary>
        /// Serializes the ref to string.
        /// </summary>
        /// <returns>The serialized ref.</returns>
        public string Serialize()
        {
            return uniqueId ?? "";
        }

        /// <summary>
        /// Deserializes a ref from string.
        /// </summary>
        /// <param name="serializedRef">The serialized ref.</param>
        /// <returns>The deserialized ref, or an empty ref if it cannot be deserialized.</returns>
        //public static AssetManagerRef Deserialize(string serializedRef)
        //{
        //    if (string.IsNullOrEmpty(serializedRef)) {
        //        return new AssetManagerRef();
        //    }
        //    else if (!IsValidUniqueId(serializedRef)) {
        //        throw new NetworkSerializationException("Serialized AssetManagerRef does not have valid content.");
        //    }
        //    else {
        //        return new AssetManagerRef(serializedRef);
        //    }
        //}
        
        /// <summary>
        /// Converts to a string representation of the type.
        /// </summary>
        /// <returns>A string representation of the type.</returns>
        public override string ToString()
        {
            return "AssetManagerRef(" + Serialize() + ")";
        }


        public static bool IsValidUniqueId(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            return Statics.UniqueIdRegex.IsMatch(text);
        }

        private static class Statics
        {
            public static readonly Regex UniqueIdRegex = new Regex(@"^zx_[a-z0-9]{32}(?:\+[0-9]+)?$", RegexOptions.CultureInvariant);
        }
    }
}
