using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace GameTacoSDK
{
    public class DialogManager : SingletonMono<DialogManager>, IDialogController
    {
        private IDialogView dialogView;
        private GameObject caller;
        private Transform canvas;
        private Action hideCallback;
        public void hideDialog()
        {
            hideCallback?.Invoke();
            dialogView.destroy();
            if (caller.GetComponent<DialogManager>() != null)
                Destroy(caller.GetComponent<DialogManager>());
            Destroy(gameObject);
        }

        public void showMessage(GameObject caller, string message, string title = "", Action hideCallback = null, Transform canvas = null)
        {
            this.caller = caller;
            this.canvas = canvas;
            this.hideCallback = hideCallback;
            dialogView = caller.AddComponent<DialogView>();
            caller.GetComponent<DialogView>()._buttonClicked += buttonClicked;
            dialogView.showMessage(this, message, title,canvas);
        }
        private void buttonClicked(Button sender, TacoUIButtonEventArgs args)
        {
            switch (args.type)
            {
                case eNumComponentType.BUTTON_DIALOG_OK:
                    hideDialog();
                    break;
            }
        }
    }
}
