using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GameTaco
{
	public class WebViewController : MonoBehaviour {

	// Use this for initialization
	 public static UniWebView webView;
	void Start () {
		// CreateWebView();
	}
	public static void CreateWebView() {
		var webViewGameObject = GameObject.Find("Controller");
		webView = webViewGameObject.AddComponent<UniWebView>();
		webView.Frame = new Rect(0, 0, Screen.width, Screen.height);
		webView.SetShowToolbar(true,false,true,true);
		webView.OnShouldClose += (view) => {
				webView = null;
				return true;
		};
    }
	public static void CloseWebView() {
        Destroy(webView);
        webView = null;
    }

  public static void OnDestroy() {
        CloseWebView();
    }
	// Update is called once per frame
	void Update () {
		
		}
	}
}