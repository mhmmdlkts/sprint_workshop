using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[RequireComponent(typeof(PhotonView))]
public class BattleController : Photon.MonoBehaviour
{
	public BattleState state {
		get {
			return BattleManager.instance.GetState();
		}
		set {
			BattleManager.instance.SetState(value);
		}
	}

    void Awake()
    {
		Debug.Log ("BattleController:Awake");
		if (photonView.isMine) {
			this.enabled = false;   // due to this, Update() is not called on the owner client.
		}
		PhotonView _pv = GetComponent<PhotonView>();
		_pv.observed = this;
	}

	public void SendBattleState()
	{
		Debug.Log ("BattleController:SendBattleState");
		photonView.RPC("ReceiveBattleState", PhotonTargets.All, (int)state);
	}
	[RPC]
	void ReceiveBattleState(int state)
	{
		Debug.Log ("BattleController:ReceiveBattleState");
		this.state = (BattleState)state;

		switch (this.state) {
		case BattleState.Standby:
			break;
		case BattleState.PrepareResult:
			if (WaitingView.instance.IsShow()) {
				WaitingView.instance.Hide();
			}
			break;
		default:
			if (StandbyView.instance.IsShow()) {
				StandbyView.instance.Hide();
           	}
			break;
		} 
	}

	public void SendChangingTurn()
	{
		Debug.Log ("BattleController:SendChangingTurn");
		photonView.RPC("ReceiveChangingTurn", PhotonTargets.All, (int)BattleManager.instance.turn);
	}
	[RPC]
	void ReceiveChangingTurn(int stoneType)
	{
		Debug.Log ("BattleController:ReceiveChangingTurn");
		BattleManager.instance.turn = (StoneType)stoneType;
		this.state = BattleState.PrepareShowTurn;
	}


	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
	}
}
