using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GameTacoSDK
{
    public interface IDialogView
    {
        void showMessage(IDialogController controller,string message, string title = "",Transform canvas=null);
        void destroy();
    }
}