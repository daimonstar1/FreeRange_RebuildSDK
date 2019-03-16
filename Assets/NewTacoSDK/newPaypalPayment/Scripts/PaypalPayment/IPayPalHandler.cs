using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameTacoSDK
{
    public interface IPayPalHandler
    {
        void init(IPayPalDeposit sender, IStoreView receiver, PayPalConfig payPalConfig, PayPalDepositCallback callback=null);
        void authenticated(string clientID,string secrect);
        void makePreparePaymentAPI(string num);
        void checkout(string num);
        void destroy();
        void backPreviousPage();
    }
}
