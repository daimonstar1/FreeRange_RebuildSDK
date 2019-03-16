using System;
using UnityEngine;

namespace FRG.Core {
    /// <summary>
    /// Common interface for scrollbars, page-pills, or some other UI-element which
    ///    controls how a display list shows its data
    /// </summary>
    public interface IScrollbar {
        /// <summary>
        /// Invoked when the user sets the current scroll percentage by interacting with
        ///    this scrollbar in some way
        /// </summary>
        Action<float> OnScrollPercentChanged { get; set; }

        /// <summary>
        /// The percent of the first viewable element in the list according to its
        ///    position in the data collection. Ranged from (0f - 1f)
        /// </summary>
        float ScrollPercent { get; set; }

        /// <summary>
        /// The percent of the data which is viewable by the collection at one time
        ///    ie: if the data-list has a length of 100 and ViewablePercentage == 0.2,
        ///    then 20 elements are viewable at one time. Ranges from (0f - 1f)
        /// </summary>
        float ViewablePercentage { get; set; }

        /// <summary>
        /// The scrollbar's gameObject; Implemented by UnityEngine.Component
        /// </summary>
        GameObject gameObject { get; }

        /// <summary>
        /// The scrollbar's transform; Implemented by UnityEngine.Component
        /// </summary>
        Transform transform { get; }
    }
}