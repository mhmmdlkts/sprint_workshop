using UnityEngine;
using System.Collections;

public class InputInterface : MonoBehaviour {
	public int maxTouch = 10;
	private TouchPhase[] touchPhase;
	private Vector3[] touchPosition;
	private static bool activating = true;

	public void Awake(){
		touchPhase = new TouchPhase[maxTouch];
		touchPosition = new Vector3[maxTouch];
	}

	public void Update()
	{
#if UNITY_IPHONE || UNITY_ANDROID
			// Todo : Input.touchCount > maxTouch
			for (int i = 0; i < Input.touchCount; ++i) {
				Touch t = Input.GetTouch(i);
				touchPhase[i] = t.phase;
				touchPosition[i] = t.position;
			}
#elif UNITY_EDITOR
			touchPhase[0] = TouchPhase.Moved;
			touchPosition[0] = Input.mousePosition;
#endif
	}
	public void OnGUI()
	{
		// Debug
		if(!Debug.isDebugBuild) return;
		for(int i=0;i<maxTouch;i++) {
			Rect r = new Rect(touchPosition[i].x,Screen.height - touchPosition[i].y,10,10);
			GUI.Box(r,""+i);
		}
	}

	public static void Activate()
	{
		activating = true;
	}

	public static void Inactivate()
	{
		activating = false;
	}

	/*
	 * Static
	 */
	public static bool isClick ()
	{
		if (!activating) return false;
		return (Input.GetMouseButtonDown (0) || (Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Began));
	}
	public static bool isClickEnd()
	{
		if (!activating) return false;
		return (Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended));
	}
	public static bool isMove()
	{
		if (!activating) return false;
		return (Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved));
	}
	public static bool isCancel()
	{
		if (!activating) return false;
		return (Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Canceled));
	}
	public static bool isStationary()
	{
		if (!activating) return false;
		return (Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Stationary));
	}
	public static Vector3 position
	{
		get { return Input.mousePosition; }
	}
}

