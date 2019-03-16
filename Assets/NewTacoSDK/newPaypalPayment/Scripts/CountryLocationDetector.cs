using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using GameTacoSDK;
public class CountryLocationDetector : SingletonMono<CountryLocationDetector>
{
    public delegate void CountryLocationDetectCallback(string rawdata, bool can_continute);
    private Transform canvas;
    private GameObject prefab_loading;
    private GameObject loading;
    private CountryLocationDetectCallback callback;
    void Awake()
    {
        prefab_loading = Resources.Load<GameObject>("Prefabs/loading");
    }
    private void showLoading()
    {
        if (loading != null)
        {
            loading.SetActive(true);
            return;
        }
        loading = Instantiate(prefab_loading, Vector3.zero, Quaternion.identity);
        if (canvas != null)
            loading.transform.parent = canvas;
        else
            loading.transform.parent = findRootCanvas();

        loading.name = "loading";
        loading.transform.position = Vector3.zero;
        loading.transform.localPosition = Vector3.zero;
        loading.transform.localScale = Vector3.one;
        View.SetAnchor(loading.GetComponent<RectTransform>(), AnchorPresets.StretchAll);
        View.SetPivot(loading.GetComponent<RectTransform>(), PivotPresets.MiddleCenter);
    }
    private void destroy()
    {
        if(loading!=null)
            Destroy(loading);
        Destroy(gameObject);
    }
    private void hideLoading()
    {
        loading.SetActive(false);
    }
    private Transform findRootCanvas()
    {
        if (canvas != null)
            return canvas;
        canvas = FindObjectOfType<Canvas>().transform;
        if (canvas == null)
            Debug.LogError("canvas is null");
        return canvas;
    }
    public void startDetectLocation(Transform canvas, CountryLocationDetectCallback callback)
    {
        this.canvas = canvas;
        this.callback = callback;
        StartCoroutine(checkIPLocation());
    }
    private IEnumerator checkIPLocation()
    {
        showLoading();
        UnityWebRequest www = UnityWebRequest.Get("http://ip-api.com/json");
        yield return www.Send();
        hideLoading();
        if (www.isNetworkError || www.responseCode == 500)
            DialogManager.Instance.showMessage(gameObject, "error=" + www.error,"",()=> {
                callback?.Invoke(www.error, false);
                destroy();
            }, this.canvas);
        else
        {
            JSONObject json = new JSONObject(www.downloadHandler.text);
            if (json == null)
            {
                DialogManager.Instance.showMessage(gameObject, "error=" + www.downloadHandler.text, "", () => {
                    callback?.Invoke(www.downloadHandler.text, false);
                    destroy();
                }, this.canvas);
            }
            else
            {
                string countryCode = json.GetField("countryCode").str;
                string country = json.GetField("country").str;
                string city = json.GetField("city").str;
                string message= "You are at " + city + "," + country ;
                bool can_continute = true;
                if (countryCode.ToLower().Trim().Equals("vn"))
                {
                    message = "Real Money transactions are not allowed in your current location";
                    can_continute = false;
                }
                DialogManager.Instance.showMessage(gameObject, message, "", () => {
                    callback?.Invoke(www.downloadHandler.text, can_continute);
                    destroy();
                }, this.canvas);
            }

        }
    }
}
