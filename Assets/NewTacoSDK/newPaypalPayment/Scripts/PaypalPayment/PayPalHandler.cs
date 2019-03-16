using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.Networking;
using GameTaco;
using System;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
namespace GameTacoSDK
{
    public class PayPalHandler :MonoBehaviour, IPayPalHandler
    {
        private IPayPalDeposit controller;
        private IStoreView viewer;
        private PayPalConfig payPalConfig;
        private event PayPalDepositCallback callback;
        private string num;

        public void init(IPayPalDeposit controller, IStoreView viewer, PayPalConfig payPalConfig, PayPalDepositCallback callback = null)
        {
            this.controller = controller;
            this.viewer = viewer;
            this.callback = callback;
            this.payPalConfig = payPalConfig;
            authenticated(payPalConfig.clientID, payPalConfig.secret);
        }
        #region authenticated
        private PayPalGetAccessTokenJsonResponse API_SuccessAuthenticatedResponse;
        private PayPalErrorJsonResponse API_ErrorAuthenticatedResponse;
        public void authenticated(string clientID, string secrect)
        {
           viewer.showLoading();
           viewer.disableButton(eNumComponentType.BUTTON_PAYPAL_DEPOSIT_NEXT);
           StartCoroutine(makeAuthenticatedAPIcall(clientID, secrect));
        }
        IEnumerator makeAuthenticatedAPIcall(string clientID, string secrect)
        {
            callHttpRequest_authenticated(clientID, secrect);
            yield return new WaitUntil(() => respone_authenticated != "");
            if (respone_authenticated.Equals("-1"))
                handleAuthenticatedErrorResponse(error_authenticated);
            else
                handleAuthenticatedSuccessResponse(respone_authenticated);
            viewer.hideLoading();

            /*
            WWWForm postData = new WWWForm();
            postData.AddField("grant_type", "client_credentials");

            string endpointURL = payPalConfig.isUsingSandbox() ?
                "https://api.sandbox.paypal.com/v1/oauth2/token" :
                "https://api.paypal.com/v1/oauth2/token";
            UnityWebRequest www = UnityWebRequest.Post(endpointURL, postData);
            www.SetRequestHeader("Accept", "application/json");
            www.SetRequestHeader("content-type", "application/x-www-form-urlencoded");
            www.SetRequestHeader("Accept-Language", "en_US");
            www.SetRequestHeader("Authorization", "Basic " + System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(clientID + ":" + secrect)));

            Debug.Log("Making call to: " + endpointURL);

            yield return www.SendWebRequest();
            viewer.hideLoading();
            if (www.error == null)
            {
                Debug.Log("WWW Ok! Full Text: " + www.downloadHandler.text);
                handleAuthenticatedSuccessResponse(www.downloadHandler.text);
            }
            else
            {
                Debug.Log("WWW Error: " + www.error);
                handleAuthenticatedErrorResponse(www.downloadHandler.text, www.error);
            }*/
        }
        private string respone_authenticated = "",error_authenticated="";
        private void callHttpRequest_authenticated(string clientID, string secrect)
        {
            respone_authenticated = "";
            error_authenticated = "";
            Thread thr = new Thread(new ThreadStart(()=> {
                try
                {
                    string endpointURL = payPalConfig.isUsingSandbox() ?
                        "https://api.sandbox.paypal.com/v1/oauth2/token" :
                        "https://api.paypal.com/v1/oauth2/token";

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endpointURL);
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.Headers.Add("Authorization", "Basic " + System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(clientID + ":" + secrect)));
                    request.Accept = "application/json";
                    request.Headers.Add("Accept-Language", "en_US");
                    request.Method = "POST ";

                    string postForm = "grant_type=client_credentials";
                    var data = Encoding.ASCII.GetBytes(postForm);

                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }

                    var response = request.GetResponse();

                    string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    Debug.LogError("callHttpRequest_authenticated=" + responseString);
                    respone_authenticated = responseString;
                }
                catch (Exception ex)
                {
                    Debug.Log("ex="+ex.Message);
                    respone_authenticated = "-1";
                    error_authenticated = ex.Message;
                }
            }));
            thr.Start();
        }

        private void handleAuthenticatedSuccessResponse(string responseText)
        {
            API_SuccessAuthenticatedResponse = null;
            API_SuccessAuthenticatedResponse = JsonUtility.FromJson<PayPalGetAccessTokenJsonResponse>(responseText);
            Debug.Log("parsed response");
            viewer.enableButton(eNumComponentType.BUTTON_PAYPAL_DEPOSIT_NEXT);

        }

        private void handleAuthenticatedErrorResponse(string errorText)
        {
            API_ErrorAuthenticatedResponse = JsonUtility.FromJson<PayPalErrorJsonResponse>(errorText);

            if (API_ErrorAuthenticatedResponse == null)
            {
                API_ErrorAuthenticatedResponse = new PayPalErrorJsonResponse();
                API_ErrorAuthenticatedResponse.message = errorText;
            }
            viewer.showDialog("error="+ errorText);
            Debug.Log("parsed response");

        }
        #endregion

        #region prepare payment
        private PayPalCreatePaymentJsonResponse API_SuccessPreparePaymentResponse;

        public void makePreparePaymentAPI(string num)
        {
            if (API_SuccessAuthenticatedResponse == null)
            {
                if (this.controller != null)
                    this.controller.authenticated();
                else
                    Debug.LogError("Failed to authenticate,Please try again!");
                return;
            }
            this.num = num;
            StartCoroutine(makePreparePaymentAPIcall(num));
        }

        void handlePreparePaymentSuccessResponse(string responseText)
        {
            API_SuccessPreparePaymentResponse = JsonUtility.FromJson<PayPalCreatePaymentJsonResponse>(responseText);
            viewer.activeButton(eNumComponentType.BUTTON_PAYPAL_DEPOSIT_CHECKOUT);
            viewer.disableInputField(eNumComponentType.INPUT_PAYPAL_AMOUNT);
            viewer.deactiveButton(eNumComponentType.BUTTON_PAYPAL_DEPOSIT_NEXT);
            viewer.showTextPaymentInfo(num);

        }

        void handlePreparePaymentErrorResponse(string errorText)
        {
            Debug.Log("error=" + errorText);
            viewer.showDialog("error="+ errorText);
        }
        private string respone_preparepayment, error_preparepayment;
        private void callHttpRequest_preparepayment(string num)
        {
            respone_preparepayment = "";
            error_preparepayment = "";
            string data_post = JsonUtility.ToJson(createRequest(num));
            Thread thr = new Thread(new ThreadStart(() => {
                try
                {
                    string endpointURL = payPalConfig.isUsingSandbox() ?
                    "https://api.sandbox.paypal.com/v1/payments/payment" :
                    "https://api.paypal.com/v1/payments/payment";
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endpointURL);
                    request.ContentType = "application/json";
                    request.Headers.Add("Authorization", "Bearer " + API_SuccessAuthenticatedResponse.access_token);
                    request.Accept = "application/json";
                    request.Method = "POST ";

                    string postForm = data_post;
                    var data = Encoding.ASCII.GetBytes(postForm);

                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }

                    var response = request.GetResponse();

                    string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    Debug.LogError("callHttpRequest_preparepayment=" + responseString);

                    respone_preparepayment = responseString;
                }
                catch (Exception ex)
                {
                    Debug.Log("ex=" + ex.Message);
                    respone_preparepayment = "-1";
                    error_preparepayment = ex.Message;
                }
            }));
            thr.Start();

            
        }
        IEnumerator makePreparePaymentAPIcall(string num)
        {
            viewer.showLoading();
            callHttpRequest_preparepayment(num);
            yield return new WaitUntil(() => respone_preparepayment != "");
            if (respone_preparepayment.Equals("-1"))
                handlePreparePaymentErrorResponse(error_preparepayment);
            else
                handlePreparePaymentSuccessResponse(respone_preparepayment);
            viewer.hideLoading();


            /*string endpointURL = payPalConfig.isUsingSandbox() ?
               "https://api.sandbox.paypal.com/v1/payments/payment" :
               "https://api.paypal.com/v1/payments/payment";

            UnityWebRequest www = UnityWebRequest.Put(endpointURL, JsonUtility.ToJson(createRequest(num)));
            www.method = UnityWebRequest.kHttpVerbPOST;
            www.SetRequestHeader("Accept", "application/json");
            www.SetRequestHeader("content-type", "application/json");
            www.SetRequestHeader("Authorization", "Bearer " + API_SuccessAuthenticatedResponse.access_token);
            yield return www.SendWebRequest();
            viewer.hideLoading();
            if (www.error == null)
            {
                Debug.Log("WWW Ok! Full Text: " + www.downloadHandler.text);
                handlePreparePaymentSuccessResponse(www.downloadHandler.text);

            }
            else
            {
                Debug.Log("WWW Error: " + www.error);
                Debug.Log("WWW Text: " + www.downloadHandler.text);

                handlePreparePaymentErrorResponse(www.downloadHandler.text, www.error);
            }*/
        }
        private PayPalCreatePaymentJsonRequest createRequest(string itemPrice)
        {
            TextAsset JSON_CreatePaymentRequest = Resources.Load("Misc/CreatePaymentRequestBody") as TextAsset;

            PayPalCreatePaymentJsonRequest request = JsonUtility.FromJson<PayPalCreatePaymentJsonRequest>(JSON_CreatePaymentRequest.text);

            request.transactions[0].amount.total = itemPrice;
            request.transactions[0].amount.currency = payPalConfig.currencyCode;
            request.transactions[0].description = "Deposit from TacoGame";
            request.transactions[0].invoice_number = System.Guid.NewGuid().ToString();
            request.transactions[0].item_list.items[0].name = "Deposit";
            request.transactions[0].item_list.items[0].description = "Deposit from TacoGame";
            request.transactions[0].item_list.items[0].price = itemPrice+"";
            request.transactions[0].item_list.items[0].currency = payPalConfig.currencyCode;

            return request;

        }

        #endregion

        #region checkout

        private PayPalCreatePaymentJsonResponse API_SuccessResponse;
        private PayPalErrorJsonResponse API_ErrorResponse;
        private PaymentListener newPaymentListener;
        private UniWebView webView;

        public void backPreviousPage()
        {
            if (webView != null)
            {
                if (webView.CanGoBack)
                    webView.GoBack();
                else
                {
                    CancelInvoke("checkForPurchaseStatusChange");
                    viewer.hideLoading();
                    Destroy(webView);
                    viewer.hideWebLayout();
                    Destroy(newPaymentListener);
                    viewer.destroyPaymentAPI();
                }
            }
        }
        private void createWebView(string url)
        {
            viewer.showWebLayout();
            webView=gameObject.AddComponent<UniWebView>();
            webView.Frame = new Rect(0, 150, Screen.width, Screen.height-150);
            webView.SetShowToolbar(false, false, true, true);
            webView.OnShouldClose += (view) => {
                webView = null;
                return true;
            };
            webView.OnPageStarted += startPage;
            webView.Load(url);
            webView.Show();
        }
        private void startPage(UniWebView webView, string url)
        {
            if (!url.Contains("sandbox.paypal.com") ||
                !url.Contains("paypal.com"))
            {
				Debug.LogError("Destroy webView");
                Destroy(webView);
                viewer.hideWebLayout();
                viewer.showLoading();

            }
        }
        public void checkout(string num)
        {
            string checkoutUrl = API_SuccessPreparePaymentResponse.links[1].href;
            if (Application.platform.Equals(RuntimePlatform.WebGLPlayer))
                Application.OpenURL(checkoutUrl);
            else
            {
            #if UNITY_EDITOR
               Application.OpenURL(checkoutUrl);
                createWebView(checkoutUrl);
#else
                createWebView(checkoutUrl);
#endif
            }

            newPaymentListener = gameObject.AddComponent<PaymentListener>();
            newPaymentListener.payPalConfig = payPalConfig;
            newPaymentListener.accessToken = API_SuccessAuthenticatedResponse.access_token;
            newPaymentListener.listeningInterval = 10f;
            newPaymentListener.payID = API_SuccessPreparePaymentResponse.id;

            InvokeRepeating("checkForPurchaseStatusChange",1f,1f);
        }
        void checkForPurchaseStatusChange()
        {
            if (newPaymentListener.listenerStatus == PaymentListener.ListenerState.SUCCESS)
                finishPayment(true);
            else if (newPaymentListener.listenerStatus == PaymentListener.ListenerState.FAILURE)
               finishPayment(false);
        }
        private void finishPayment(bool success)
        {
            CancelInvoke("checkForPurchaseStatusChange");
            viewer.hideLoading();
            
            if (success)
            {
                viewer.showLoading();
                StartCoroutine(sendPaymenInfo(newPaymentListener.payment_success_data));
               
            }
            else
            {
                callback?.Invoke(false, "", "");
                Destroy(newPaymentListener);
                viewer.showDialog("Opp!,Something wrong..Please try again");
            }
        }

#endregion


        public void destroy()
        {
            if (GetComponent<PaymentListener>() != null)
                Destroy(GetComponent<PaymentListener>());
            if (GetComponent<ShowPaymentAPI_Call>() != null)
                Destroy(GetComponent<ShowPaymentAPI_Call>());
            if (GetComponent<ExecutePaymentAPI_Call>() != null)
                Destroy(GetComponent<ExecutePaymentAPI_Call>());
            Destroy(this);
        }
        #region send the payment info to server
        private string respone_sendpaymentinfo = "", error_sendpaymentinfo = "";
        private void callHttpRequest_sendpaymentinfo(string paymentinfo)
        {
            respone_sendpaymentinfo = "";
            error_sendpaymentinfo = "";
            Thread thr = new Thread(new ThreadStart(() => {
                try
                {
                    paymentinfo = "{\"payment_info\":" + paymentinfo + "}";

                    string endpointURL = GameTaco.Constants.BaseUrl + "api/funds/executePaymentFromApp";

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endpointURL);
                    request.ContentType = "application/json";
                    request.Accept = "application/json";
                    request.Headers.Add("x-access-token", TacoManager.User.token);
                    request.Method = "POST ";

                    string postForm = paymentinfo;
                    var data = Encoding.ASCII.GetBytes(postForm);

                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }

                    var response = request.GetResponse();

                    string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    Debug.LogError("callHttpRequest_sendpaymentinfo=" + responseString);
                    respone_sendpaymentinfo = responseString;
                }
                catch (Exception ex)
                {
                    Debug.Log("ex=" + ex.Message);
                    respone_sendpaymentinfo = "-1";
                    error_sendpaymentinfo = ex.Message;
                }
            }));
            thr.Start();
        }

        IEnumerator sendPaymenInfo(string paymentinfo)
        {
            callHttpRequest_sendpaymentinfo(paymentinfo);
            yield return new WaitUntil(() => respone_sendpaymentinfo != "");
            if (respone_sendpaymentinfo.Equals("-1"))
            {
                Destroy(newPaymentListener);
                viewer.showDialog("Opp!,Cannot save your payment progress to server!");
                callback?.Invoke(false, error_sendpaymentinfo, paymentinfo);
            }
            else
            {
                Destroy(newPaymentListener);
                JSONObject json = new JSONObject(respone_sendpaymentinfo);
                if (json == null)
                {
                    Destroy(newPaymentListener);
                    viewer.showDialog("Opp!,Cannot save your payment progress to server!");
                    callback?.Invoke(false, respone_sendpaymentinfo, paymentinfo);
                }
                else
                {
                    bool success = json.GetField("success").b;
                    if (success)
                    {
                        viewer.showDialog("Deposit successfully!");
                        callback?.Invoke(true, json.GetField("msg").str, paymentinfo);
                    }
                    else
                    {
                        viewer.showDialog(json.GetField("msg").str);
                        callback?.Invoke(false, json.GetField("msg").str, paymentinfo);
                    }
                }
            }
            viewer.hideLoading();

            /*paymentinfo = "{\"payment_info\":" + paymentinfo + "}";
            Debug.LogError("data=" + paymentinfo);
            UnityWebRequest www = UnityWebRequest.Put(Constants.BaseUrl + "api/funds/executePaymentFromApp", paymentinfo);
            www.method = UnityWebRequest.kHttpVerbPOST;
            www.SetRequestHeader("x-access-token", TacoManager.User.token);
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "application/json");
            yield return www.SendWebRequest();
            viewer.hideLoading();
            if (www.isNetworkError || www.responseCode == 500)
            {
                Destroy(newPaymentListener);
                viewer.showDialog("Opp!,Cannot save your payment progress to server!");
                callback?.Invoke(false, www.downloadHandler.text==null?"" : www.downloadHandler.text, paymentinfo);
            }
            else
            {
                Destroy(newPaymentListener);
                JSONObject json = new JSONObject(www.downloadHandler.text);
                if (json == null)
                {
                    Destroy(newPaymentListener);
                    viewer.showDialog("Opp!,Cannot save your payment progress to server!");
                    callback?.Invoke(false, www.downloadHandler.text == null ? "" : www.downloadHandler.text, paymentinfo);
                }
                else
                {
                    bool success = json.GetField("success").b;
                    if (success)
                    {
                        viewer.showDialog("Deposit successfully!");
                        callback?.Invoke(true, json.GetField("msg").str, paymentinfo);
                    }
                    else
                    {
                        viewer.showDialog(json.GetField("msg").str);
                        callback?.Invoke(false, json.GetField("msg").str, paymentinfo);
                    }
                }
               
            }*/
        }
        #endregion
    }
}

