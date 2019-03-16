using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading;
using System.Net;
using System.IO;
using System;
using System.Text;

public class ExecutePaymentAPI_Call : MonoBehaviour {

	public string paymentID;

	public string payerID;

	public string accessToken;

	//[HideInInspector]
	public PayPalExecutePaymentJsonResponse API_SuccessResponse;
    public PayPalConfig payPalConfig;
    public string respone_success;
	// Use this for initialization
	void Start () {
		Debug.Log("calling coroutine");
		StartCoroutine (MakePayAPIcall ());
	}

	void handleSuccessResponse(string responseText) {

		//attempt to parse reponse text
		API_SuccessResponse = JsonUtility.FromJson<PayPalExecutePaymentJsonResponse>(responseText);
        respone_success = responseText;

        Debug.Log ("parsed response");

	}

	void handleErrorResponse(string errorText) {

		//attempt to parse error response 
		Debug.Log ("error="+ errorText);

	}
    private string respone_makepayapi, error_makepayapi;
    private void callHttpRequest_makepayapi()
    {
        respone_makepayapi = "";
        error_makepayapi = "";
        PayPalExecutePaymentJsonRequest request1 = new PayPalExecutePaymentJsonRequest();
        request1.payer_id = payerID;
        string data_post = JsonUtility.ToJson(request1);
        Thread thr = new Thread(new ThreadStart(() => {
            try
            {
                string baseEndpointURL = payPalConfig.isUsingSandbox() ?
                "https://api.sandbox.paypal.com/v1/payments/payment/" :
                "https://api.paypal.com/v1/payments/payment/";

                string endpointURL = baseEndpointURL + paymentID + "/execute";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endpointURL);
                request.ContentType = "application/json";
                request.Headers.Add("Authorization", "Bearer " + accessToken);
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
                Debug.LogError("callHttpRequest_makepayapi_ex=" + responseString);
                respone_makepayapi = responseString;
            }
            catch (Exception ex)
            {
                Debug.Log("ex=" + ex.Message);
                respone_makepayapi = "-1";
                error_makepayapi = ex.Message;
            }
        }));
        thr.Start();
    }
    IEnumerator MakePayAPIcall() {

        callHttpRequest_makepayapi();
        yield return new WaitUntil(() => respone_makepayapi != "");
        if (respone_makepayapi.Equals("-1"))
            handleErrorResponse(error_makepayapi);
        else
            handleSuccessResponse(respone_makepayapi);

/*
        PayPalExecutePaymentJsonRequest request = new PayPalExecutePaymentJsonRequest ();
		request.payer_id = payerID;

		string baseEndpointURL = payPalConfig.isUsingSandbox () ?
			"https://api.sandbox.paypal.com/v1/payments/payment/" :
			"https://api.paypal.com/v1/payments/payment/";

		string endpointURL = baseEndpointURL + paymentID + "/execute";
        UnityWebRequest www = UnityWebRequest.Put(endpointURL, JsonUtility.ToJson(request));
        www.method = UnityWebRequest.kHttpVerbPOST;
        www.SetRequestHeader("Accept", "application/json");
        www.SetRequestHeader("content-type", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + accessToken);

        Debug.Log("Making call to: " + endpointURL);

		yield return www.SendWebRequest();

		//if ok response
		if (www.error == null) {
			Debug.Log("WWW Ok! Full Text: " + www.downloadHandler.text);

			handleSuccessResponse (www.downloadHandler.text);

		} else {
			Debug.Log("WWW Error: "+ www.error);
			Debug.Log("WWW Text: "+ www.downloadHandler.text);

			handleErrorResponse (www.downloadHandler.text, www.error);
		}   */ 
	}
}
