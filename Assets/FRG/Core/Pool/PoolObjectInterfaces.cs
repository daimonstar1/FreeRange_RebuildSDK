using System;

namespace FRG.Core
{
    /// <summary>
    /// An object that needs to be cleaned up before it is saved.
    /// </summary>
    public interface IPresaveCleanupHandler
    {
        void OnPresaveCleanup();
    }
    
    public interface IClickable
    {
        void SetClickEvent(Action evt, UnityEngine.EventSystems.PointerEventData.InputButton button_);
    }
}
