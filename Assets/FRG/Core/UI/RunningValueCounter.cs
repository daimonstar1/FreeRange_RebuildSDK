using System;
using System.Collections.Generic;
using UnityEngine;

namespace FRG.Core {
    public class RunningValueCounter : MonoBehaviour {

        [SerializeField] protected int samplesCount = 30;
        //[SerializeField] protected bool useFixedUpdate = false;

        public float Value { get { return Median; } }

        public float Current { get { return values[writeIndex]; } }

        public float Average {
            get {
                float averageFps = 0;
                for(int i = 0;i < values.Length;i++)
                    averageFps += values[i];
                averageFps /= values.Length;
                return averageFps;
            }
        }

        public float Median { get { if ( valuesSorted == null || valuesSorted.Length == 0 ) return 0; return valuesSorted[valuesSorted.Length / 2]; } }

        float[] values; // for average and recording
        float[] valuesSorted; // for median
        int writeIndex = 0;

        protected virtual void Update() {
            //if(useFixedUpdate) return;
            DoUpdate();
        }

        //protected virtual void FixedUpdate() {
        //    if(!useFixedUpdate) return;
        //    DoUpdate();
        //}

        protected virtual void DoUpdate() {

            if(samplesCount > 500) {
                Debug.LogError("You have too large sample count, it must be an error in inspector field, please correct the value to 30 or something useful.");
                return;
            }

            if(values == null || values.Length != samplesCount) values = new float[samplesCount];
            if(valuesSorted == null || valuesSorted.Length != samplesCount) valuesSorted = new float[samplesCount];

            writeIndex++;
            if(writeIndex >= values.Length) {
                writeIndex = 0;
            }

            values[writeIndex] = RefreshValue();

            Array.Copy(values, valuesSorted, values.Length);
            Array.Sort(valuesSorted, Comparer<float>.Default);
        }

        protected virtual float RefreshValue() {
            return 0f;
        }

    }
}

