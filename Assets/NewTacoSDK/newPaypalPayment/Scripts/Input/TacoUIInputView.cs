using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameTacoSDK
{
	public class TacoUIInputView : InputField,ITacoUIInputView
	{
		#region ITacoUIInputView implementation

		public event TacoUIInputHandler<InputField> data_changed;
        protected override void Start()
        {
            base.Start();
            initEvent();
        }
        public void initEvent ()
		{
			onValueChanged.AddListener (delegate {
				_valueChanged ();
			});
		}

		#endregion

		public eNumComponentType type = eNumComponentType.NONE;

		private void _valueChanged ()
		{
			if (type == eNumComponentType.NONE) {
				Debug.LogError ("Your Inputfield doesn't define a type of component yet, please assign the type of Inputfield");
			} else
				data_changed.Invoke (this, new TacoUIInputEventArgs (text, type));
		}


	}
}
