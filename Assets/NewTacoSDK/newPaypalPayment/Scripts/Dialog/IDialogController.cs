using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GameTacoSDK
{
    public interface IDialogController
    {
        void hideDialog();
        void showMessage(GameObject caller, string message, string title = "", System.Action hideCallback = null, Transform canvas = null);
    }
}
