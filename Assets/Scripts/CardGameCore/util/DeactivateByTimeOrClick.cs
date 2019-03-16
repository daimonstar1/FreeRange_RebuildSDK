using System;
using System.Collections;
using UnityEngine;

namespace FRG.Taco
{
    public class DeactivateByTimeOrClick : MonoBehaviour
    {
        [SerializeField] private GameObject target;
        [SerializeField] private float deactivateAfterSeconds;

        private void Start()
        {
            if (target == null)
            {
                throw new ArgumentException("Please set target.");
            }

            StartCoroutine(DeactivateAfterSeconds(deactivateAfterSeconds, gameObject));
        }


        private void OnMouseDown()
        {
            if (target != null)
            {
                StopAllCoroutines();
                target.SetActive(false);
            }
            
        }

        private IEnumerator DeactivateAfterSeconds(float seconds, GameObject target)
        {
            yield return new WaitForSeconds(seconds);
            target.SetActive(false);
        }
    }
}