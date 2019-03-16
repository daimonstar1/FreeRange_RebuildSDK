using System;
using UnityEngine;

namespace FRG.Core
{
    public class FpsCounter : MonoBehaviour
    {
        private static FpsCounter _instance;

        public static FpsCounter instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("FpsCounter");
                    var fpsCounter = go.AddComponent<FpsCounter>();
                    DontDestroyOnLoad(go);
                    _instance = fpsCounter;
                }

                return _instance;
            }
        }

        [SerializeField] public int fpsSamplesCount = 30;

        private float[] fpsValues; // for average and recording
        private float[] fpsValuesSorted; // for median
        private float[] copyHelperArray;
        private int fpsValuesWriteIndex = 0;

        public float Fps {
            get { return MedianFps; }
        }

        public float CurrentFps {
            get { return fpsValues[fpsValuesWriteIndex]; }
        }

        public float AverageFps {
            get {
                float averageFps = 0;
                for (int i = 0; i < fpsValues.Length; i++)
                    averageFps += fpsValues[i];
                averageFps /= fpsValues.Length;
                return averageFps;
            }
        }

        public float MedianFps {
            get { return fpsValuesSorted[fpsValuesSorted.Length / 2]; }
        }

        void Awake() {
            // in case we want to put the scrip manually in scene
            if (_instance == null)
                _instance = this;

            if (fpsSamplesCount > 500) {
                Debug.LogError("You have too large sample count, it must be an error in inspector field, please correct the value to 30 or something useful.");
                return;
            }

            fpsValues = new float[fpsSamplesCount];
            fpsValuesSorted = new float[fpsSamplesCount];
            copyHelperArray = new float[fpsSamplesCount];
        }

        void Update() {
            fpsValuesWriteIndex++;
            if (fpsValuesWriteIndex >= fpsValues.Length) {
                fpsValuesWriteIndex = 0;
            }

            float oldestValue = fpsValues[fpsValuesWriteIndex];
            float newValue = 1f / Time.deltaTime;
            fpsValues[fpsValuesWriteIndex] = newValue;

            RemoveInsertValuesIntoSortedArray(fpsValuesSorted, oldestValue, newValue);

            // cross checking our new algorithm
            //float[] fpsValuesSorted2 = new float[fpsValuesSorted.Length];
            //Array.Copy(fpsValues, fpsValuesSorted2, fpsValues.Length);
            //Array.Sort(fpsValuesSorted2);
            //for (int i = 0; i < fpsValuesSorted.Length; i++)
            //{
            //    if (!Mathf.Approximately(fpsValuesSorted[i], fpsValuesSorted2[i]))
            //    {
            //        Debug.LogError("FpsCounter error");
            //    }
            //}
        }

        /// <summary>
        /// Removes the value from the sorted array and adds a new value afterwards.
        /// Find indexes to remove and index to where to add in. Our array is then split into 3 parts, A+B+C, where the seperators 
        /// are index to remove and index to add. A part can stay, so copy B and C blocks to helper array, removing the extra 
        /// space where we remove the value and add extra space where we add the value, then copy averything back to original array.
        /// </summary>
        private void RemoveInsertValuesIntoSortedArray(float[] array, float valToRemove, float valToAdd)
        {
            // for debug
            //Array.Clear(helperArray, 0, helperArray.Length);

            // points to smaller of 2 indexes where the velue should be, -1 for first value
            int indexOfValToAdd;
            int indexOfValToRemove;
            FindIndexOfElements(array, valToRemove, valToAdd, out indexOfValToRemove, out indexOfValToAdd);

            // new value just so happens to be inserted before or after value we're removing, just apply new value to index
            // A + rem/add + B
            if (indexOfValToAdd == indexOfValToRemove || indexOfValToAdd - 1 == indexOfValToRemove)
            {
                array[indexOfValToRemove] = valToAdd;
                return;
            }
            // add val + B + rem val + C
            else if (indexOfValToAdd == 0)
            {
                int blockBstart = 0;
                int blockCstart = indexOfValToRemove + 1;

                // copy B to helper array
                Array.Copy(array, blockBstart, copyHelperArray, blockBstart + 1, indexOfValToRemove);
                // copy C to helper array
                Array.Copy(array, blockCstart, copyHelperArray, blockCstart, array.Length - blockCstart);
                // apply new value
                copyHelperArray[0] = valToAdd;
                // copy B+val+C back to original without old val
                Array.Copy(copyHelperArray, 0, array, 0, array.Length);
            }
            // A + rem val + B + add val
            else if (indexOfValToAdd == array.Length)
            {
                int blockBstart = indexOfValToRemove + 1;

                // copy B to helper array
                Array.Copy(array, blockBstart, copyHelperArray, blockBstart - 1, array.Length - blockBstart);
                // apply new value
                copyHelperArray[indexOfValToAdd - 1] = valToAdd;
                // copy B+val back to original without old val
                Array.Copy(copyHelperArray, blockBstart - 1, array, indexOfValToRemove, array.Length - indexOfValToRemove);
            }
            // A + rem val + B + add val + C
            else if (indexOfValToRemove < indexOfValToAdd)
            {
                int blockBstart = indexOfValToRemove + 1;
                int blockCstart = indexOfValToAdd;

                // copy B to helper array
                Array.Copy(array, blockBstart, copyHelperArray, blockBstart - 1, indexOfValToAdd - blockBstart);
                // copy C to helper array
                Array.Copy(array, blockCstart, copyHelperArray, blockCstart, array.Length - blockCstart);
                // apply new value
                copyHelperArray[indexOfValToAdd - 1] = valToAdd;
                // copy B+val+C back to original without old val
                Array.Copy(copyHelperArray, blockBstart - 1, array, indexOfValToRemove, array.Length - indexOfValToRemove);
            }
            // A + add val + B + rem val + C
            else
            {
                int blockBstart = indexOfValToAdd;
                int blockCstart = indexOfValToRemove + 1;

                // copy B to helper array
                Array.Copy(array, blockBstart, copyHelperArray, blockBstart + 1, indexOfValToRemove - blockBstart);
                // copy C to helper array
                Array.Copy(array, blockCstart, copyHelperArray, blockCstart, array.Length - blockCstart);
                // apply new value
                copyHelperArray[indexOfValToAdd] = valToAdd;
                // copy B+val+C back to original without old val
                Array.Copy(copyHelperArray, indexOfValToAdd, array, indexOfValToAdd, array.Length - indexOfValToAdd);
            }
        }

        private void FindIndexOfElements(float[] array, float valToRemove, float valToAdd, out int indexOfValToRemove, out int indexOfValToAdd)
        {
            indexOfValToRemove = -1;
            indexOfValToAdd = 0;

            // loop but skip last index we 're already assuming it's there
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == valToRemove)
                {
                    indexOfValToRemove = i;
                }

                if (valToAdd >= array[i])
                {
                    indexOfValToAdd = i + 1;
                }
            }
        }
    }
}