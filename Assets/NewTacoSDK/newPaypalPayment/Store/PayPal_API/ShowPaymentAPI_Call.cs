using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading;
using System.Net;
using System.IO;
using System;

public class ShowPaymentAPI_Call : MonoBehaviour {

	public string payID;

	public string accessToken;

	//[HideInInspector]
	public PayPalShowPaymentJsonResponse API_SuccessResponse;

	//[HideInInspector]
    public PayPalConfig payPalConfig;
	// Use this for initialization
	void Start () {
		Debug.Log("calling coroutine");
		StartCoroutine (MakePayAPIcall ());
	}
	void handleSuccessResponse(string responseText) {

		//attempt to parse reponse text
		API_SuccessResponse = JsonUtility.FromJson<PayPalShowPaymentJsonResponse>(responseText);
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
        Thread thr = new Thread(new ThreadStart(() => {
            try
            {
                string baseEndpointURL = payPalConfig.isUsingSandbox() ?
                "https://api.sandbox.paypal.com/v1/payments/payment/" :
                "https://api.paypal.com/v1/payments/payment/";
                string endpointURL = baseEndpointURL + payID;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endpointURL);
                request.ContentType = "application/json";
                request.Headers.Add("Authorization", "Bearer " + accessToken);
                request.Accept = "application/json";
                request.Method = "GET ";

                var response = request.GetResponse();

                string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                Debug.LogError("callHttpRequest_makepayapi_show=" + responseString);
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

        /*string baseEndpointURL = payPalConfig.isUsingSandbox () ?
			"https://api.sandbox.paypal.com/v1/payments/payment/" :
			"https://api.paypal.com/v1/payments/payment/";

		string endpointURL = baseEndpointURL + payID;
        UnityWebRequest www = UnityWebRequest.Get(endpointURL);
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Accept", "application/json");
        //www.SetRequestHeader("content-type", "application/x-www-form-urlencoded");
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
		} */
    }
}
