using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[RequireComponent(typeof(PhotonView))]
public class BattleNetwork : Photon.MonoBehaviour
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
		Debug.Log ("BattleNetwork:Awake");
		if (photonView.isMine) {
			this.enabled = false;   // due to this, Update() is not called on the owner client.
		}
		PhotonView _pv = GetComponent<PhotonView>();
		_pv.observed = this;
	}

	public void SendBattleState()
	{
		Debug.Log ("BattleNetwork:SendBattleState");
		photonView.RPC("ReceiveBattleState", PhotonTargets.All, (int)state);
	}
	[RPC]
	void ReceiveBattleState(int state)
	{
		Debug.Log ("BattleNetwork:ReceiveBattleState");
		this.state = (BattleState)state;

		if ((BattleState)state != BattleState.Standby) {
			if (StandbyView.instance.IsShow()) {
				StandbyView.instance.Hide();
           	}
		}
	}

	public void SendChangingTurn()
	{
		Debug.Log ("BattleNetwork:SendChangingTurn");
		photonView.RPC("ReceiveChangingTurn", PhotonTargets.All, (int)BattleManager.instance.turn);
	}
	[RPC]
	void ReceiveChangingTurn(int stoneType)
	{
		Debug.Log ("BattleNetwork:ReceiveChangingTurn");
		BattleManager.instance.turn = (StoneType)stoneType;
		this.state = BattleState.PrepareAim;
	}


	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
	}
}
