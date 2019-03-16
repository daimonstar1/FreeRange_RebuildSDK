using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GameTacoSDK;

namespace GameTaco {
public class MakeDepositScript : MonoBehaviour, IPointerClickHandler {

	public GameObject TacoBlockingCanvas;
	public GameObject ButtonDepoistFundError;
	public bool IsImage;

	// Use this for initialization
	void Start () {
		//TacoBlockingCanvas = GameObject.Find(GameTaco.CanvasNames.TacoBlockingCanvas);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if(IsImage)
			GameTaco.BalanceManager.Instance.Init(0);
	}

	public void OnDepositCash()
	{
		GameTaco.BalanceManager.Instance.Init(0);
		TacoBlockingCanvas.SetActive(true);
	}
	public void OnPurchaseToken()
	{
		GameTaco.BalanceManager.Instance.Init(1);
		TacoBlockingCanvas.SetActive(true);
	}

	public void OnDepositCashWhenNotEnoughFund()
	{
		 GameObject FundErrorJoin = GameObject.Find("TacoGame/TacoCommonCanvas/PanelList/FundErrorJoin");
		// GameObject BalancePanelHeader = GameObject.Find("TacoGame/TacoCommonCanvas/BalancePanel/Container/Header");
		// GameObject BalancePanelMoney = GameObject.Find("TacoGame/TacoCommonCanvas/BalancePanel/Container/Money");
		// GameObject BalancePanelFunds = GameObject.Find("TacoGame/TacoCommonCanvas/BalancePanel/Container/Funds");
		 
		// if(BalancePanelFunds != null)
		// {
		// 	// BalancePanelHeader.SetActive(false);
		// 	// BalancePanelMoney.SetActive(false);
		// 	// BalancePanelFunds.SetActive(false);
		// }
		// TournamentManager.Instance.OpenModalDepositDisablePanel();
		PayPalDeposit.instance.Init(FundErrorJoin, FundErrorJoin.transform, (bool save_payment_info_status, string save_payment_info, string paymentinfo) => {
			if (save_payment_info_status)
			{
				BalanceManager.Instance.LoadCurrentBalance();
				if(FundErrorJoin != null)
				{
					//FundErrorJoin.SetActive(false);
				}
			}
				//LoadCurrentBalance();
		});

	}


}

}