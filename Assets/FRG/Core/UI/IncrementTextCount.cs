using UnityEngine;
using UnityEngine.UI;

namespace FRG.Core.UI
{
    [AddComponentMenu(""), DisallowMultipleComponent]
    public class IncrementTextCount : MonoBehaviour
	{
		[SerializeField]
		protected float timeBetweenIncrements = 0f;
		[SerializeField]
		protected float incrementPercentage = 1f;
		[SerializeField]
		protected int minimumTotalIncrements = 10;
#if UNITY_4_5
		[SerializeField]
		protected TextMesh uitext;
#else
		[SerializeField]
		protected Text uiText;
#endif	
		protected int count;
		protected int countShown;

		protected float timeCalled;

		protected int minimum = -1;

		protected int increment
		{
			get
			{
				int _increment = (int)( count * ( incrementPercentage / 100f ) );

				if( _increment > minimum ) // if increment is greater than minimum, use minimum
				{
					_increment = minimum;
				}

				if( _increment < 1 ) // if increment is less than one, use one
				{
					return 1;
				}
				
				return _increment;
			}
		}

		protected void LateUpdate()
		{
#if UNITY_EDITOR
			if( uiText == null )
			{
				return;
			}
#endif
			int newCount = 0;
			System.Int32.TryParse( uiText.text, out newCount );

			if( count != newCount )
			{
				SetCount( newCount );
			}

			if( countShown != count )
			{
				if( Time.time > timeCalled + timeBetweenIncrements )
				{
					if( countShown < count )
					{
						countShown += increment;

						if( countShown > count )
						{
							countShown = count;
						}
					}
					else
					{
						countShown -= increment;
							
						if( countShown < count )
						{
							countShown = count;
						}	
					}
					timeCalled = Time.time;
				}
				uiText.text = ( countShown ).ToString();
			}
		}

		protected void SetCount( int newCount )
		{
#if UNITY_EDITOR
			if( uiText == null )
			{
				Debug.LogError( name + " has no Text script." );
				return;
			}
#endif
			count = newCount;
			uiText.text = countShown.ToString();
			timeCalled = Time.time;
			minimum = Mathf.Abs( count - countShown ) / minimumTotalIncrements;
		}

#if UNITY_EDITOR
		public bool SetText()
		{
			if( uiText == null )
			{
#if UNITY_4_5
				uiText = transform.GetComponent<TextMesh>();
#else
				uiText = transform.GetComponent<Text>();
#endif
				return true;
			}

			return false;
		}
#endif
	}
}