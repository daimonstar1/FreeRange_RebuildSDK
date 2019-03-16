using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace GameTacoSDK
{
    public class DialogView : View, IDialogView
    {
        private GameObject prefab_dialog;
        private GameObject dialog;
        private IDialogController controller;
        private Text tv_message;
        private Text tv_title;
        void Awake()
        {
            prefab_dialog = Resources.Load<GameObject>("Prefabs/dialog");
        }
        public void destroy()
        {
            Destroy(dialog);
            Destroy(gameObject);
        }
        public void showMessage(IDialogController controller,string message, string title = "", Transform canvas = null)
        {
            this.controller = controller;
            StartCoroutine(startRenderView(message,title,canvas));
        }
        private IEnumerator startRenderView(string message,string title="", Transform canvas = null)
        {
            if (dialog != null)
            {
                dialog.SetActive(true);
                yield return null;
            }
            else
            {
                dialog = Instantiate(prefab_dialog, Vector3.zero, Quaternion.identity);
                if (canvas != null)
                    dialog.transform.parent = canvas;
                else
                    dialog.transform.parent = findRootCanvas();
                dialog.transform.position = Vector3.zero;
                dialog.transform.localPosition = Vector3.zero;
                dialog.transform.localScale = Vector3.one;
                dialog.name = "dialog";
                tv_message = dialog.transform.GetChild(0).Find("message").GetComponent<Text>();
                tv_message.text = message;
                tv_title = dialog.transform.GetChild(0).Find("title").GetComponent<Text>();
                if (!title.Equals(""))
                    tv_title.text = title;
                addButton(eNumComponentType.BUTTON_DIALOG_OK, dialog.transform.GetChild(0).Find("ok").GetComponent<TacoUIButtonView>());
                SetAnchor(dialog.GetComponent<RectTransform>(), AnchorPresets.StretchAll);
                SetPivot(dialog.GetComponent<RectTransform>(), PivotPresets.MiddleCenter);
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
