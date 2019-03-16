using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameTacoSDK
{
	public class TacoUIToggleView : Toggle,ITacoUIToggleView
	{
		#region ITacoUIToggleView implementation

		public event TacoUIToggleHandler<Toggle> check_changed;

		public void initEvent ()
		{
			onValueChanged.AddListener ((bool arg0) => _valueChanged (arg0));
		}

		#endregion

		public eNumComponentType type;

		private void _valueChanged (bool ischecked)
		{
			if (type == eNumComponentType.NONE) {
				Debug.LogError ("Your Toggle doesn't define a type of component yet, please assign the type of Toggle");
			} else
				check_changed.Invoke (this, new TacoUIToggleEventArgs (ischecked == true ? "1" : "0", type));
		}
		
	}
}
