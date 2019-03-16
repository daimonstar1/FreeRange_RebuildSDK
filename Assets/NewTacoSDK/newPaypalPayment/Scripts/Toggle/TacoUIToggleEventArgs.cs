using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameTacoSDK
{
	public interface ITacoUIToggleView
	{
		event TacoUIToggleHandler<Toggle> check_changed;

		void initEvent ();
	}

	public class TacoUIToggleEventArgs
	{
		public string is_checked;
		public eNumComponentType type;

		public TacoUIToggleEventArgs (string _is_checked, eNumComponentType _type)
		{
			this.is_checked = _is_checked;
			this.type = _type;
		}
	}
	public delegate void TacoUIToggleHandler<Toggle> (Toggle sender, TacoUIToggleEventArgs args);
}
