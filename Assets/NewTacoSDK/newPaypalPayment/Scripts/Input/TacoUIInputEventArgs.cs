using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameTacoSDK
{
	public interface ITacoUIInputView
	{
		event TacoUIInputHandler<InputField> data_changed;

		void initEvent ();
	}

	public class TacoUIInputEventArgs
	{
		public string _text;
		public eNumComponentType type;

		public TacoUIInputEventArgs (string _text, eNumComponentType _type)
		{
			this._text = _text;
			this.type = _type;
		}
	}
	public delegate void TacoUIInputHandler<InputField> (InputField sender, TacoUIInputEventArgs args);
}
