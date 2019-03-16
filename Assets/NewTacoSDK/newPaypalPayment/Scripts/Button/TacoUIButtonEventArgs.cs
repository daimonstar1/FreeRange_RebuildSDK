using UnityEngine.UI;

namespace GameTacoSDK
{
	public interface ITacoUIButtonView
	{
		event TacoUIButtonHandler<Button> on_click;

		void initEvent ();
	}
	public class TacoUIButtonEventArgs
	{
		public eNumComponentType type;

		public TacoUIButtonEventArgs (eNumComponentType _type)
		{
			this.type = _type;
		}
	}
	public delegate void TacoUIButtonHandler<Button> (Button sender, TacoUIButtonEventArgs args);
}
