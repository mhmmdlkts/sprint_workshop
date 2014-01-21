using UnityEngine;
using System.Collections;

public class WaitingView : View<WaitingView>
{
	void Start()
	{
		WaitingView.InitInstance();
		AddPart(transform.Find("Waiting"), InMotion.Right);
		isTapHiding = false;
	}
}
