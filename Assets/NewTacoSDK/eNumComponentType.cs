using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameTacoSDK
{
	/// <summary>
	/// To identify type of The taco view component
	/// </summary>
	public enum eNumComponentType
	{
		NONE,
        OBJECT_LOADING,
        OBJECT_CANVAS,
        OBJECT_TOPBAR,
        INPUT_PAYPAL_AMOUNT,
        BUTTON_PAYPAL_DEPOSIT_NEXT,
        BUTTON_PAYPAL_DEPOSIT_CHECKOUT,
        BUTTON_PAYPAL_DEPOSIT_WEB_BACK,
        BUTTON_PAYPAL_DEPOSIT_OK,
        BUTTON_PAYPAL_DISMISS,
        BUTTON_DIALOG_OK
    }
}
