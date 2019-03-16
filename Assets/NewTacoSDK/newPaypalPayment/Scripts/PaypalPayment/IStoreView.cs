using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GameTacoSDK
{
    public interface IStoreView
    {
        void showLoading();
        void hideLoading();
        void showStoreView();
        void setController(IPayPalDeposit controller);
        void destroy();
        void disableButton(eNumComponentType type);
        void activeButton(eNumComponentType type);
        void deactiveButton(eNumComponentType type);
        void enableButton(eNumComponentType type);
        void disableInputField(eNumComponentType type);
        void enableInputField(eNumComponentType type);
        void showDialog(string message);
        void hideDialog();
        void showWebLayout();
        void hideWebLayout();
        void showPaymentLayout();
        void hidePaymentLayout();
        void hideInput();
        void showTextPaymentInfo(string num);
        void destroyPaymentAPI();
        void resetInput();

    }
}
