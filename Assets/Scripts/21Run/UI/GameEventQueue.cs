using System.Collections;
using System.Collections.Generic;
using FRG.Taco.Run21;
using UnityEngine;

namespace FRG.Taco
{
    /// <summary>
    /// Used to enqueue game events, so they can be processed in order of arrival by <see cref="PopupManager"/> 
    /// </summary>
    public class GameEventQueue : MonoBehaviour
    {
        private List<GameEvent> enqueuedEvents = new List<GameEvent>();
        private Coroutine eventBeingProcessed;

        [SerializeField] PopupManager _popupManager;

        /// <summary>
        /// Add event to queue for execution.
        /// </summary>
        /// <param name="gameEvent"></param>
        public void EnqueueEvent(GameEvent gameEvent)
        {
            enqueuedEvents.Add(gameEvent);

            if (eventBeingProcessed == null)
            {
                ProcessNextEvent();
            }
        }

        /// <summary>
        /// When finished will triggering processing of next event in queue.
        /// </summary>
        public void ProcessNextEvent()
        {
            if (enqueuedEvents.Count == 0 || eventBeingProcessed != null)
            {
                return;
            }

            eventBeingProcessed = StartCoroutine(GetProcessNextEventCoroutine());
        }

        private IEnumerator GetProcessNextEventCoroutine()
        {
            if (enqueuedEvents.Count == 0 || eventBeingProcessed != null)
            {
                yield return null;
            }
            else
            {
                GameEvent _event = enqueuedEvents[0];
                enqueuedEvents.RemoveAt(0);

                eventBeingProcessed = _popupManager.TogglePopupsForEvent(_event, () =>
                {
                    eventBeingProcessed = null;
                    ProcessNextEvent();
                });

                yield return null;
            }
        }


        public bool AreAllEventsProcessed()
        {
            return enqueuedEvents.Count == 0 && eventBeingProcessed == null;
        }
    }
}