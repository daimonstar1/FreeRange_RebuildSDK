using System;
using UnityEngine;
using UnityEngine.UI;
namespace GameTacoSDK
{
    public delegate void PayPalDepositCallback(bool save_payment_info_status, string save_payment_info, string paymentinfo);
    public class PayPalDeposit : Singleton<PayPalDeposit>, IPayPalDeposit
    {
        private IStoreView storeView;
        private IPayPalHandler payPalHandler;
        private PayPalConfig payPalConfig;
        private GameObject caller;
        private Transform canvas;
        private string amount;
        private event PayPalDepositCallback callback;
        public void addPayPalConfig(PayPalConfig payPalConfig)
        {
            this.payPalConfig = payPalConfig;
        }

        public void authenticated()
        {
            if (payPalConfig == null|| payPalHandler==null)
            {
                if (storeView != null)
                    storeView.destroy();
                if (caller != null)
                    Init(caller);
                else
                    Debug.LogError("Please call Init first!");
                return;
            }
            payPalHandler.authenticated(payPalConfig.clientID, payPalConfig.secret);
        }

        public void checkout()
        {
            payPalHandler.checkout(amount);
        }

        public void finishRender()
        {
            storeView.showLoading();
            payPalHandler = caller.AddComponent<PayPalHandler>();
            payPalHandler.init(this, storeView,payPalConfig, callback);
        }
        private void buttonClicked(Button sender, TacoUIButtonEventArgs args)
        {
            switch (args.type)
            {
                case eNumComponentType.BUTTON_PAYPAL_DEPOSIT_NEXT:
                    makePayment();
                    break;
                case eNumComponentType.BUTTON_PAYPAL_DEPOSIT_CHECKOUT:
                    checkout();
                    break;
                case eNumComponentType.BUTTON_PAYPAL_DISMISS:
                    destroy();
                    break;
                case eNumComponentType.BUTTON_PAYPAL_DEPOSIT_OK:
                    destroy();
                    break;
                case eNumComponentType.BUTTON_PAYPAL_DEPOSIT_WEB_BACK:
                    backPreviousPage();
                    break;
            }
        }
        private void inputChanged(InputField sender, TacoUIInputEventArgs args)
        {
            switch (args.type)
            {
                case eNumComponentType.INPUT_PAYPAL_AMOUNT:
                    amount = args._text;
                    break;
            }
        }
        public void Init(GameObject caller,Transform canvas=null, PayPalDepositCallback callback=null)
        {
            this.caller = caller;
            this.canvas = canvas;
            this.callback = callback;
            storeView = caller.AddComponent<StoreView>();
            storeView.setController(this);
            caller.GetComponent<StoreView>()._buttonClicked+= buttonClicked;
            caller.GetComponent<StoreView>()._textChanged += inputChanged;
            storeView.showStoreView();
        }

        public void makePayment()
        {
            if (payPalConfig == null || payPalHandler == null)
            {
                if (storeView != null)
                    storeView.destroy();
                if (caller != null)
                    Init(caller);
                else
                    Debug.LogError("Please call Init first!");
                return;
            }
            if (amount.Equals(""))
                return;
            try
            {
                int num = int.Parse(amount);
				if(num>0)
					payPalHandler.makePreparePaymentAPI(num+"");
				else
					storeView.resetInput();
            }
            catch (Exception ex) {
                storeView.resetInput();
            } 
        }

        public void tryagain()
        {
            authenticated();
        }

        public void destroy()
        {
            storeView.destroy();
            payPalHandler.destroy();
        }

        public Transform getCanvas()
        {
            return this.canvas;
        }
        private void backPreviousPage()
        {
            payPalHandler.backPreviousPage();
        }
    }
}

