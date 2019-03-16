using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace GameTacoSDK
{
    public class StoreView : View, IStoreView
    {
        
        private GameObject prefab_loading;
        private GameObject prefab_store;
        private GameObject loading;
        private GameObject store;
        private GameObject payment_layout;
        private GameObject dialog_layout;
        private GameObject web_layout;
        private GameObject tex_payment_info;
        private IPayPalDeposit controller;
        void Awake()
        {
            prefab_loading = Resources.Load<GameObject>("Prefabs/loading");
            prefab_store = Resources.Load<GameObject>("Prefabs/store");
        }
        public void hideLoading()
        {
            loading.SetActive(false);
        }
        public void showLoading()
        {
            if (loading != null)
            {
                loading.SetActive(true);
                return;
            }
            loading = Instantiate(prefab_loading, Vector3.zero, Quaternion.identity);
            if (controller != null && controller.getCanvas() != null)
                loading.transform.parent = controller.getCanvas();
            else
                loading.transform.parent = findRootCanvas();

            loading.name = "loading";
            loading.transform.position = Vector3.zero;
            loading.transform.localPosition = Vector3.zero;
            loading.transform.localScale =Vector3.one;
            SetAnchor(loading.GetComponent<RectTransform>(), AnchorPresets.StretchAll);
            SetPivot(loading.GetComponent<RectTransform>(), PivotPresets.MiddleCenter);
        }

        public void setController(IPayPalDeposit controller)
        {
            this.controller = controller;
        }

        public void showStoreView()
        {
            StartCoroutine(startRenderStoreView());
            
        }
        private IEnumerator startRenderStoreView()
        {
            if (store != null)
            {
                store.SetActive(true);
                yield return null;
            }
            else
            {
                store = Instantiate(prefab_store, Vector3.zero, Quaternion.identity);
                if (controller != null && controller.getCanvas() != null)
                    store.transform.parent = controller.getCanvas();
                else
                    store.transform.parent = findRootCanvas();
                store.transform.position = Vector3.zero;
                store.transform.localPosition = Vector3.zero;
                store.transform.localScale = Vector3.one;
                store.name = "store";
                payment_layout = store.transform.Find("payment_layout").gameObject;
                dialog_layout = store.transform.Find("dialog_layout").gameObject;
                web_layout = store.transform.Find("web_layout").gameObject;
                tex_payment_info = payment_layout.transform.Find("tex").gameObject;
                addButton(eNumComponentType.BUTTON_PAYPAL_DEPOSIT_NEXT, payment_layout.transform.Find("next").GetComponent<TacoUIButtonView>());
                addButton(eNumComponentType.BUTTON_PAYPAL_DEPOSIT_CHECKOUT, payment_layout.transform.Find("checkout").GetComponent<TacoUIButtonView>());
                addInputField(eNumComponentType.INPUT_PAYPAL_AMOUNT, payment_layout.transform.Find("input").GetComponent<TacoUIInputView>());
                SetAnchor(store.GetComponent<RectTransform>(), AnchorPresets.StretchAll);
                SetPivot(store.GetComponent<RectTransform>(), PivotPresets.MiddleCenter);
                showPaymentLayout();
                if (controller != null)
                    controller.addPayPalConfig(store.GetComponent<PayPalConfig>());
                yield return new WaitForEndOfFrame();
                controller.finishRender();
            }
        }

        public void destroy()
        {
            Destroy(loading);
            Destroy(store);
            Destroy(this);
        }

        public void disableButton(eNumComponentType type)
        {
            getButton(type).enabled = false;
        }

        public void enableButton(eNumComponentType type)
        {
            getButton(type).enabled = true;
        }

        public void activeButton(eNumComponentType type)
        {
            getButton(type).gameObject.SetActive(true);
        }

        public void deactiveButton(eNumComponentType type)
        {
            getButton(type).gameObject.SetActive(false);
        }

        public void disableInputField(eNumComponentType type)
        {
            getInputField(type).enabled = false;
        }

        public void enableInputField(eNumComponentType type)
        {
            getInputField(type).enabled = true;
        }

        public void showDialog(string message)
        {
            dialog_layout.SetActive(true);
            dialog_layout.transform.Find("message").GetComponent<Text>().text = message;
            hideWebLayout();
            hidePaymentLayout();
        }

        public void hideDialog()
        {
            dialog_layout.SetActive(false);
        }

        public void showWebLayout()
        {
            web_layout.SetActive(true);
        }

        public void hideWebLayout()
        {
            web_layout.SetActive(false);
        }

        public void showPaymentLayout()
        {
            payment_layout.SetActive(true);
        }

        public void hidePaymentLayout()
        {
            payment_layout.SetActive(false);
        }

        public void hideInput()
        {
            getInputField(eNumComponentType.INPUT_PAYPAL_AMOUNT).gameObject.SetActive(false);
        }

        public void showTextPaymentInfo(string num)
        {
            hideInput();
            tex_payment_info.gameObject.SetActive(true);
            tex_payment_info.GetComponent<Text>().text = "Deposit $" + num + ".00 to your account.";
        }

        public void destroyPaymentAPI()
        {
            if (gameObject.GetComponent<ShowPaymentAPI_Call>() != null)
                Destroy(gameObject.GetComponent<ShowPaymentAPI_Call>());
            if (gameObject.GetComponent<ExecutePaymentAPI_Call>() != null)
                Destroy(gameObject.GetComponent<ExecutePaymentAPI_Call>());
        }

        public void resetInput()
        {
            getInputField(eNumComponentType.INPUT_PAYPAL_AMOUNT).text = "";
        }
    }
}

