#define FRG_PROFILE_EDITOR_PROGRESS

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace FRG.SharedCore
{
    /// <summary>
    /// Shows editor progress bar and adds profiler samples. No-ops when not in editor or not profiling.
    /// </summary>
    /// <remarks>Can turn profiling on/off by commenting out the FRG_PROFILE_EDITOR_PROGRESS symbol in this file.</remarks>
    /// <example>
    /// 
    /// // RunWithCancel is optional, but otherwise there is no cancel button
    /// bool notCanceled = RunWithCancel("Loading Game", () => {
    ///     
    ///     // Showing 0% progress and "Loading Game"
    ///     
    ///     using (EditorProgress.PushSection("Initializing Somatic Overlay", .5f)
    ///     {
    ///        // Showing 0% progress and "Initializing Somatic Overlay"
    ///        
    ///        using (EditorProgress.PushSection("Reticulating Splines", .4f) {
    ///           
    ///           // Showing 0% progress and "Reticulating Splines"
    ///           
    ///           // Will apply 50% * 40% = 20% progress when it leaves the inner block
    ///        }
    ///        
    ///        // Showing 0% progress and "Reticulating Splines" (does not update until next push)
    ///        
    ///        using (EditorProgress.PushSection("Loading Assets", .4f) {
    ///           
    ///           // Showing 20% progress and "Loading Assets" / ""
    ///        
    ///           int length = 40;
    ///           for (int i = 0; i &lt; length; ++i) {
    ///               
    ///               string X = (i + 1).ToString();
    ///               string path = "Asset/MyFile" + X;
    ///               using (EditorProgress.PushIteration(path, i, length) {
    ///                  
    ///                  // Showing from 20% to 39.5% progress and "Reticulating Splines [X / 40]" with info text "MyFileX"
    ///               }
    ///           }
    ///           
    ///           // Showing from 39.5% progress and "Reticulating Splines [40 / 40]" with info text "MyFile40" (does not update until next push)
    ///           
    ///           // Will apply 50% * 40% = 20% more progress when it leaves this block
    ///        }
    ///
    ///        // Will never apply more than 50% total progress when it leaves the outer block
    ///     }
    /// });
    /// 
    /// // Automatically hides the progress dialog when it leaves the last block, in this case, the RunWithCancel
    /// 
    /// </example>
    public static class EditorProgress
    {
        private static bool _supportCancellation;

#if UNITY_EDITOR
        [UnityEditor.MenuItem("FRG/Editor/Hide Progress Bar", priority = 49)]
        private static void HideProgress()
        {
            UnityEditor.EditorUtility.ClearProgressBar();
            _supportCancellation = false;
        }
#endif // UNITY_EDITOR

        /// <summary>
        /// Shows a progress dialog.
        /// </summary>
        /// <param name="title">The title to display.</param>
        /// <param name="progressIncrement">The amount of time that will pass when this block is complete.</param>
        /// <returns>An object meant to be put into a using block.</returns>
        public static ProgressState PushSection(string title, float progressIncrement)
        {
            return PushSection(title, "", progressIncrement);
        }

        /// <summary>
        /// Shows a progress dialog.
        /// </summary>
        /// <param name="title">The title to display.</param>
        /// <param name="info">The info display at the bottom of the dialog.</param>
        /// <param name="progressIncrement">The amount of time that will pass when the using block is complete.</param>
        /// <returns>An object meant to be put into a using block.</returns>
        public static ProgressState PushSection(string title, string info, float progressIncrement)
        {
#if UNITY_EDITOR
            Display(title, info, int.MinValue);
#endif

            return PushInternal(title, progressIncrement);
        }

        /// <summary>
        /// Specifies a loop iteration of some sort, with indices.
        /// </summary>
        /// <param name="info">The info message to display.</param>
        /// <param name="displayIndex">The index to display for current progress (will have 1 added).</param>
        /// <param name="displayCount">The amount of items to display for current progress.</param>
        /// <returns>An object meant to be put into a using block.</returns>
        public static ProgressState PushIteration(string info, int index, int count)
        {
            return PushIteration(info, index, count, false);
        }

        /// <summary>
        /// Specifies a loop iteration of some sort, with indices.
        /// </summary>
        /// <param name="info">The info message to display.</param>
        /// <param name="displayIndex">The index to display for current progress (will have 1 added).</param>
        /// <param name="displayCount">The amount of items to display for current progress.</param>
        /// <param name="alwaysShow">
        /// Forbid the progress meter from skipping this display if there are a lot of very fast updates.
        /// Specify false (default) to better determine which item is actually hanging.
        /// </param>
        /// <returns>An object meant to be put into a using block.</returns>
        public static ProgressState PushIteration(string info, int index, int count, bool alwaysShow)
        {
            float progressIncrement = 0;

#if UNITY_EDITOR
            ProgressRecord top = EditorState.Top;
            float nextProgress = top.ProgressMin + Mathf.Clamp01(index / (float)(count - 1)) * (top.ProgressMax - top.ProgressMin);
            progressIncrement = Mathf.Clamp(nextProgress - EditorState._currentProgress, 0, top.ProgressMultiplier);
#endif

            return PushIteration(info, progressIncrement, index, count, alwaysShow);
        }

        /// <summary>
        /// Specifies a loop iteration of some sort, with indices.
        /// </summary>
        /// <param name="info">The info message to display.</param>
        /// <param name="progressIncrement">The amount to increment progress (other overloads compute this).</param>
        /// <param name="displayIndex">The index to display for current progress (will have 1 added).</param>
        /// <param name="displayCount">The amount of items to display for current progress.</param>
        /// <returns>An object meant to be put into a using block.</returns>
        public static ProgressState PushIteration(string info, float progressIncrement, int displayIndex, int displayCount)
        {
            return PushIteration(info, progressIncrement, displayIndex, displayCount, false);
        }

        /// <summary>
        /// Specifies a loop iteration of some sort, with indices.
        /// </summary>
        /// <param name="info">The info message to display.</param>
        /// <param name="progressIncrement">The amount to increment progress (other overloads compute this).</param>
        /// <param name="displayIndex">The index to display for current progress (will have 1 added).</param>
        /// <param name="displayCount">The amount of items to display for current progress.</param>
        /// <param name="alwaysShow">
        /// Forbid the progress meter from skipping this display if there are a lot of very fast updates.
        /// Specify false (default) to better determine which item is actually hanging.
        /// </param>
        /// <returns>An object meant to be put into a using block.</returns>
        public static ProgressState PushIteration(string info, float progressIncrement, int displayIndex, int displayCount, bool alwaysShow)
        {
#if UNITY_EDITOR
            DisplayIteration(info, displayIndex, displayCount, alwaysShow);
#endif // UNITY_EDITOR

            return PushInternal("", progressIncrement);
        }

        /// <summary>
        /// Specifies an inner loop iteration or step of some sort.
        /// </summary>
        /// <param name="info">The info message to display.</param>
        /// <param name="progressIncrement">The amount to increment progress (other overloads compute this).</param>
        /// <returns>An object meant to be put into a using block.</returns>
        public static ProgressState PushRawIteration(string info, float progressIncrement)
        {
            return PushRawIteration(info, progressIncrement, false);
        }

        /// <summary>
        /// Specifies an inner loop iteration or step of some sort.
        /// </summary>
        /// <param name="info">The info message to display.</param>
        /// <param name="progressIncrement">The amount to increment progress (other overloads compute this).</param>
        /// <param name="alwaysShow">
        /// Forbid the progress meter from skipping this display if there are a lot of very fast updates.
        /// Specify false (default) to better determine which item is actually hanging.
        /// </param>
        /// <returns>An object meant to be put into a using block.</returns>
        public static ProgressState PushRawIteration(string info, float progressIncrement, bool alwaysShow)
        {
#if UNITY_EDITOR
            DisplayIteration(info, alwaysShow);
#endif

            return PushInternal("", progressIncrement);
        }

#if UNITY_EDITOR
        private static void DisplayIteration(string info, int displayIndex, int displayCount, bool alwaysShow)
        {
            // Don't display iterations in batch mode
            if (UnityEditorInternal.InternalEditorUtility.inBatchMode) {
                return;
            }

            if (alwaysShow ||
                /* Make sure at correct depth and enough time has passed. */
                (EditorState._lastDisplayedDepth != EditorState.StackDepth || EditorState.CurrentTime >= EditorState._lastDisplayedTimestamp + EditorState.SkipIterationTimeIncrement) ||
                /* Always show first and last. */
                (displayIndex == 0 || displayIndex == displayCount - 1)) {
                string displayTitle = string.Format("{0} ({1}/{2})", EditorState.Top.IterationTitle, (displayIndex + 1).ToString(), displayCount.ToString());

                Display(displayTitle, info, displayIndex);
            }
        }

        private static void DisplayIteration(string info, bool alwaysShow)
        {
            // Don't display iterations in batch mode
            if (UnityEditorInternal.InternalEditorUtility.inBatchMode) {
                return;
            }

            if (alwaysShow ||
                /* Make sure at correct depth and enough time has passed. */
                (EditorState._lastDisplayedDepth != EditorState.StackDepth || EditorState.CurrentTime >= EditorState._lastDisplayedTimestamp + EditorState.SkipIterationTimeIncrement)) {
                Display("", info, int.MinValue);
            }
        }

        private static void Display(string displayTitle, string displayInfo, int displayIndex)
        {
            ProgressRecord parent = EditorState.Top;

            if (string.IsNullOrEmpty(displayTitle)) { displayTitle = parent.IterationTitle; }

            displayInfo = displayInfo ?? "";
            float displayProgress = EditorState._currentProgress;

            EditorState._lastDisplayedDepth = EditorState.StackDepth;
            EditorState._lastDisplayedTimestamp = EditorState.CurrentTime;

            if (UnityEditorInternal.InternalEditorUtility.inBatchMode) {
                if (EditorState.StackDepth <= 3) {
                    Debug.Log("EditorProgress: " + displayTitle);
                }
            }
            else if (_supportCancellation) {
                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(displayTitle, displayInfo, displayProgress)) {
                    throw new OperationCanceledException("Cancelled EditorProgress operation.");
                }
            }
            else {
                UnityEditor.EditorUtility.DisplayProgressBar(displayTitle, displayInfo, displayProgress);
            }
        }
#endif // UNITY_EDITOR

        private static ProgressState PushInternal(string iterationTitle, float progressIncrement)
        {
#if UNITY_EDITOR
            ProgressRecord parent = EditorState.Top;

            if (string.IsNullOrEmpty(iterationTitle)) { iterationTitle = parent.IterationTitle; }
            float progressMin = Mathf.Clamp01(EditorState._currentProgress);
            float progressMultiplier = parent.ProgressMultiplier * Mathf.Clamp01(progressIncrement);
            float progressMax = Math.Min(progressMin + progressMultiplier, parent.ProgressMax);
            ProgressRecord progress = new ProgressRecord(iterationTitle, progressMin, progressMax, progressMultiplier);

            if (EditorState._stack == null) { EditorState._stack = new Stack<ProgressRecord>(); }
            EditorState._stack.Push(progress);
#endif // UNITY_EDITOR

#if FRG_PROFILE_EDITOR_PROGRESS
            Profiler.BeginSample(iterationTitle);
#endif

            return new ProgressState();
        }

        /// <summary>
        /// Runs a delegate, supporting cancellation by automatically throwing <see cref="OperationCanceledException"/> in the Push methods of this class if called within the delegate.
        /// </summary>
        /// <returns>false if cancelled.</returns>
        /// <remarks>Will propagate cancellation if within another cancelled block.</remarks>
        public static bool RunWithCancel(string title, Action action)
        {
            bool hasParentCancellation = _supportCancellation;
            _supportCancellation = true;

#if UNITY_EDITOR
            int undoGroupIndex = UnityEditor.Undo.GetCurrentGroup();
#endif

            try {
                using (PushSection(title, 1.0f)) {
                    action();
                }
                return true;
            }
            catch (OperationCanceledException) {
                if (hasParentCancellation) {
                    throw;
                }

                return false;
            }
            finally {
                _supportCancellation = hasParentCancellation;

#if UNITY_EDITOR
                UnityEditor.Undo.CollapseUndoOperations(undoGroupIndex);
#endif
            }
        }

        public struct ProgressState : IDisposable
        {
#if UNITY_EDITOR
            bool _isDisposed;
#endif

            public void Dispose()
            {
#if UNITY_EDITOR
                // Imperfect guard because this is a struct
                if (_isDisposed) { return; }
                _isDisposed = true;

                if (EditorState.IsActive) {
                    ProgressRecord previousProgress = EditorState._stack.Pop();

                    EditorState._currentProgress = previousProgress.ProgressMax;

                    if (!EditorState.IsActive) {
                        EditorState._currentProgress = 0;
                        UnityEditor.EditorUtility.ClearProgressBar();
                    }

                    if (!EditorState.IsActive || EditorState.StackDepth != EditorState._lastDisplayedDepth) {
                        EditorState._lastDisplayedDepth = int.MinValue;
                        EditorState._lastDisplayedTimestamp = TimeSpan.MinValue;
                    }
                }
                else {
                    UnityEngine.Debug.LogError("Unexpected extra dispose call in EditorProgress.ProgressState.");

                    // Just in case
                    UnityEditor.EditorUtility.ClearProgressBar();
                }
#endif // UNITY_EDITOR

#if FRG_PROFILE_EDITOR_PROGRESS
                Profiler.EndSample();
#endif
            }
        }

#if UNITY_EDITOR
        private struct ProgressRecord
        {
            public readonly string IterationTitle;
            public readonly float ProgressMin;
            public readonly float ProgressMax;
            public readonly float ProgressMultiplier;

            public ProgressRecord(string iterationTitle, float progressMin, float progressMax, float progressMultiplier)
            {
                IterationTitle = iterationTitle ?? "";
                ProgressMin = progressMin;
                ProgressMax = progressMax;
                ProgressMultiplier = progressMultiplier;
            }
        }

        private static class EditorState
        {
            public const int IterationFramesPerSecond = 100;
            public static readonly TimeSpan SkipIterationTimeIncrement = TimeSpan.FromSeconds(1.0 / IterationFramesPerSecond);

            public static Stack<ProgressRecord> _stack;
            public static float _currentProgress = 0;
            public static int _lastDisplayedDepth = int.MinValue;
            public static TimeSpan _lastDisplayedTimestamp = TimeSpan.MinValue;

            public static ProgressRecord Top {
                get {
                    if (IsActive) {
                        return _stack.Peek();
                    }
                    return new ProgressRecord("", 0.0f, 1.0f, 1.0f);
                }
            }

            public static TimeSpan CurrentTime {
                get {
                    return TimeSpan.FromSeconds(UnityEditor.EditorApplication.timeSinceStartup);
                }
            }

            public static bool IsActive {
                get { return (StackDepth != 0); }
            }

            public static int StackDepth {
                get {
                    return (_stack != null) ? _stack.Count : 0;
                }
            }
        }
#endif
    }
}
