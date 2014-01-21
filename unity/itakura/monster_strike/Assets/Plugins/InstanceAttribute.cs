using UnityEngine;
using System.Collections;

public class InstanceAttribute<T> : BaseObject
	where T : MonoInheritance
{
	protected static T classInstance;

	public static T instance {
		get {
			SetInstance();
			return classInstance;
		}
	}
	
	public static void InitInstance()
	{
		SetInstance();
	}
	
	private static void SetInstance()
	{
		if (classInstance == null) {
			classInstance = FindObjectOfType (typeof(T)) as T;
			if (classInstance == null) {
				Debug.LogWarning(typeof(T) + " is nothing");
			}
		}
	}
}
