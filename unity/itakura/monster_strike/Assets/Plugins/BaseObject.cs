using UnityEngine;
using System.Collections;

public class BaseObject : MonoInheritance
{
	public float GetTouchDistance()
	{
		if (!InputInterface.isClick()) {
			return System.Single.MaxValue;
		}
		return Vector3.Distance(transform.position, InputInterface.position);
	}

	[System.Obsolete("Use InputInterface.position")]
	public static Vector3 GetTouchPosition()
	{
		return InputInterface.position;
	}

}
