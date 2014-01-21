using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BattleState {
	PrepareStandby,
	Standby,
	PrepareAim,
	Aim,
	Strike,
	Striking,
	ChangeTurn,
}

public class BattleManager : StatefulObject<BattleManager> 
{
	private StoneNetwork ownerStone;
	public BattleNetwork network;

	private bool isAiming = false;
	private Vector3 aimingPosition1 = Vector3.zero;
	private Vector3 aimingPosition2 = Vector3.zero;
	private GameObject aiming = null;

	public BattleState GetState()
	{
		return (BattleState)state;
	}

	public void SetState(BattleState state)
	{
		this.state = state;
	}

	public StoneType turn = StoneType.Yellow;

	protected void Awake() 
	{
		Application.targetFrameRate = 30;
		EnableState(typeof(BattleState));
		AddState(BattleState.PrepareStandby, OnPrepareStandby);
		AddState(BattleState.Standby, OnStandby);
		AddState(BattleState.PrepareAim, OnPrepareAim);
		AddState(BattleState.Aim, OnAim);
		AddState(BattleState.Striking, OnStriking);
		AddState(BattleState.ChangeTurn, OnChangeTurn);

		if (!PhotonNetwork.connected)
		{
			PhotonNetwork.autoJoinLobby = false;
			PhotonNetwork.ConnectUsingSettings("1");
		}

		PhotonView _pv = GetComponent<PhotonView>();
		network = GetComponent<BattleNetwork>();
		_pv.observed = network;
	}

	protected void FixedUpdate()
	{
		if (state != null) {
			switch ((BattleState)state) {
			case BattleState.Strike:
				OnFixedStrike();
				break;
			}
		}
	}

	protected void OnPrepareStandby() 
	{
		GoNext();
	}

	protected void OnStandby() 
	{
	}

	protected void OnPrepareAim() 
	{
		PhotonNetwork.room.open = false;

		if (turn != ownerStone.stoneType) {
			WaitingView.instance.Show();
		} else {
			if (WaitingView.instance.IsShow()) {
				WaitingView.instance.Hide();
			}
			foreach (GameObject _obj in GameObject.FindGameObjectsWithTag("Player")) {
				Vector3 _pos = _obj.transform.position;
				Quaternion _rot = _obj.transform.rotation;
				StoneNetwork _o = _obj.GetComponent<StoneNetwork>();
				StoneType _st = _o.stoneType;

				_o.photonView.ownerId = PhotonNetwork.player.ID;

				PhotonNetwork.Destroy(_obj);

				StoneNetwork _n = StoneNetwork.Create(_st, _pos, _rot);

				if (ownerStone.stoneType == _st) {
					ownerStone = _n;
				}
			}
		}
		GoNext();
	}

	protected void OnAim() 
	{
		if (turn == ownerStone.stoneType) {

			if (InputInterface.isClick()) {
				isAiming = true;
				aimingPosition1 = GetMousePosition(ownerStone.gameObject);
				aiming = PhotonNetwork.Instantiate("Common/Prefabs/Arrow", new Vector3(ownerStone.transform.position.x, 1f, ownerStone.transform.position.z) , Quaternion.identity, 0);
			}
			if (InputInterface.isClickEnd()) {
				aimingPosition2 = GetMousePosition(ownerStone.gameObject);
				isAiming = false;
				if (aiming != null) {
					PhotonNetwork.Destroy(aiming);
					aiming = null;
				}
			}
			if (isAiming) {
				if (aiming != null) {
					aiming.transform.rotation = Quaternion.LookRotation(aimingPosition1 - GetMousePosition(ownerStone.gameObject));
				}
			} else {
				float _power = Vector3.SqrMagnitude(aimingPosition1 - aimingPosition2);
				Debug.Log ("power=" + _power);
				if (_power > 0.1f) {
					GoNext();
				}
			}
		}
	}

	protected void OnFixedStrike()
	{
		if (turn == ownerStone.stoneType) {
			Vector3 _power = (aimingPosition1 - aimingPosition2).normalized*400f;
			ownerStone.rigidbody.AddForceAtPosition(_power, new Vector3(0f, 0f, -1f), ForceMode.Impulse);
			aimingPosition1 = Vector3.zero;
			aimingPosition2 = Vector3.zero;
			GoNext();
		}
	}

	protected void OnStriking()
	{
		if (turn == ownerStone.stoneType) {
			foreach (GameObject _obj in GameObject.FindGameObjectsWithTag("Player")) {
				if (_obj.rigidbody.velocity != Vector3.zero || _obj.rigidbody.angularVelocity != Vector3.zero) {
					return;
				}
			}
			GoNext();
		}
	}

	protected void OnChangeTurn()
	{
		if (turn == ownerStone.stoneType) {
			GameObject[] _objs = GameObject.FindGameObjectsWithTag("Player");
			GameObject _obj = _objs[Random.Range(0, _objs.Length)];
			StoneNetwork _c = _obj.GetComponent<StoneNetwork>();
			turn = _c.stoneType;
			state = BattleState.PrepareAim;
			network.SendChangingTurn();
		}
		Debug.Log ("OnChangeTurn:" + turn);
	}

	protected Vector3 GetMousePosition(GameObject gameObject)
	{
		Vector3 _screenObj = Camera.main.WorldToScreenPoint(gameObject.transform.position);
		Vector3 _screenMouse = new Vector3(InputInterface.position.x, InputInterface.position.y, _screenObj.z);
		return Camera.main.ScreenToWorldPoint(_screenMouse);
	}

	// This is one of the callback/event methods called by PUN (read more in PhotonNetworkingMessage enumeration)
	public void OnConnectedToMaster()
	{
		Debug.Log ("OnConnectedToMaster");
		PhotonNetwork.JoinRandomRoom();
	}
	
	// This is one of the callback/event methods called by PUN (read more in PhotonNetworkingMessage enumeration)
	public void OnPhotonRandomJoinFailed()
	{
		Debug.Log ("OnPhotonRandomJoinFailed");
		PhotonNetwork.CreateRoom(null, true, true, 4);
	}
	
	// This is one of the callback/event methods called by PUN (read more in PhotonNetworkingMessage enumeration)
	public void OnJoinedRoom()
	{
		Debug.Log ("OnJoinedRoom");
		if (ownerStone == null) {
			ownerStone = StoneNetwork.Create((StoneType)PhotonNetwork.countOfPlayersInRooms);
		}
		StandbyView.instance.Show();
		state = BattleState.Standby;
	}
	
	// This is one of the callback/event methods called by PUN (read more in PhotonNetworkingMessage enumeration)
	public void OnCreatedRoom()
	{
		Debug.Log ("OnCreatedRoom");
		ownerStone = StoneNetwork.Create(StoneType.Yellow);
	}

	public void OnLeftRoom()
	{
		Debug.Log("OnLeftRoom (local)");
	}

	public void OnPhotonPlayerConnected(PhotonPlayer player)
	{
		Debug.Log("OnPhotonPlayerConnected: " + player);
	}
	
	public void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		Debug.Log("OnPlayerDisconneced: " + player);
	}
}
