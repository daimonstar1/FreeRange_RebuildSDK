using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
namespace GameTacoSDK
{
	public sealed class TacoUICanvas : MonoBehaviour
	{

		void Awake()
        {
            DontDestroyOnLoad(gameObject);
            checkComponent();
        }
        /// <summary>
        /// Ensure the Camera and EventSystem exist on scene!
        /// </summary>
        private void checkComponent()
        {
            if (Camera.main == null)
                Debug.LogError("Please add at least a Camera into the scene!");
            if (EventSystem.current==null)
                Debug.LogError("Please add at least an EventSystem into the scene!");
        }
	}
}
