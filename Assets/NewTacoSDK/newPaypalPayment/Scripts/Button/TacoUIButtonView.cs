using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameTacoSDK
{
	public class TacoUIButtonView : Button,ITacoUIButtonView
	{
        protected override void Start()
        {
            base.Start();
            initEvent();
        }
		public eNumComponentType type = eNumComponentType.NONE;

		#region ITacoUIButtonView implementation

		public event TacoUIButtonHandler<Button> on_click;

		public void initEvent ()
		{
			onClick.AddListener (() => _onClick ());
		}

		#endregion

		private void _onClick ()
		{
			if (type == eNumComponentType.NONE) {
				Debug.LogError ("Your button doesn't define a type of component yet, please assign the type of button");
			} else {
				on_click.Invoke (this, new TacoUIButtonEventArgs (type));
			}
		}

	}
}
