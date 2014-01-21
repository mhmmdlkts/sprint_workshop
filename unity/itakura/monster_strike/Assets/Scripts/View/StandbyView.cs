using UnityEngine;
using System.Collections;

public class StandbyView : View<StandbyView>
{
	void Start()
	{
		StandbyView.InitInstance();
		AddPart(transform.Find("TapStarting"), InMotion.Right);
		AddPart(transform.Find("EntryNow"), InMotion.Right);
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();

		Debug.Log ("OnPrepare");
		if (!PhotonNetwork.isMasterClient) {
			SetPartActive("TapStarting", false);
			isTapHiding = false;
		}
	}


	protected override void OnPrepareHide()
	{
		if (PhotonNetwork.isMasterClient) {
			BattleManager.instance.GoNext();
			BattleManager.instance.network.SendBattleState();
		}
		base.OnPrepareHide();
	}
}
