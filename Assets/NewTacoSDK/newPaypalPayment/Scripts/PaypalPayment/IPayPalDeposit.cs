using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GameTacoSDK
{
    public interface IPayPalDeposit
    {
        void Init(GameObject caller,Transform canvas=null, PayPalDepositCallback callback=null);
        void authenticated();
        void makePayment();
        void checkout();
        void finishRender();
        void addPayPalConfig(PayPalConfig payPalConfig);
        void tryagain();
        void destroy();
        Transform getCanvas();
    }
}
