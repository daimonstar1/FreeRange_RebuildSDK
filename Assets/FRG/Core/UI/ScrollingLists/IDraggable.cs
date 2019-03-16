

using System;
using UnityEngine;

namespace FRG.Core.UI
{
    public interface IDraggable
    {
        /// <summary>
        /// Get/Set whether dragging is enabled on this object
        /// </summary>
        bool DraggingEnabled { get; set; }

        /// <summary>
        /// Action to be invoked upon the first frame of dragging
        ///   Arg0: starting local mouse position with regards to the object being dragged
        /// </summary>
        Action<Vector3> DragBegin { get; set; }

        /// <summary>
        /// Action to be invoked upon every frame while the object has been dragged.
        ///   Arg0: starting local mouse position with regards to the calling object
        ///   Arg1: current local mouse position with regards to the calling object
        ///   Arg2: local-position-delta since the previous frame
        /// </summary>
        Action<Vector3, Vector3, Vector3> DragUpdate { get; set; }

        /// <summary>
        /// Action invoked upon releasing the mouse-button after dragging
        /// </summary>
        Action DragRelease { get; set; }
    }
}