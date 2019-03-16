using UnityEngine;
using System;

namespace FRG.Core
{
    /// <summary>
    /// An exception for when required assets are not found in the asset manager.
    /// </summary>
    public class AssetNotFoundException : Exception
    {
        public AssetNotFoundException()
            : base()
        {
        }

        public AssetNotFoundException(string message)
            : base(message)
        {
        }

        public AssetNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}