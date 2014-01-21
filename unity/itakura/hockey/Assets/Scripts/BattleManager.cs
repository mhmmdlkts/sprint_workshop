using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BattleState {
	Spawn,
	PrepareStandby,
	Standby,
	PrepareBattle,
	Battle,
	PrepareShowTurn,
	ShowTurn,
	PrepareAim,
	Aim,
	Strike,
	Striking,
	ChangeTurn,
	PrepareResult,
	Result,
}

public class BattleManager : StatefulObject<BattleManager> 
{
	public StoneController ownerStone;
	public BattleController network;

	private StoneController aimingStone;
	private Vector3 aimingPosition1 = Vector3.zero;
	private Vector3 aimingPosition2 = Vector3.zero;
	private GameObject aiming = null;
	private float power = 0f;
	private const float MAX_POWOR = 2f;
	private const int MAX_STONE_NUM = 5;


	public BattleState GetState()
	{
		return (BattleState)state;
	}

	public void SetState(BattleState state)
	{
		this.state = state;
	}

	public StoneType turn = StoneType.Yellow;

	public bool isMyTurn {
		get {
			return ownerStone != null && ownerStone.stoneType == turn;
		}
		private set {}
	}

	public bool isSyncStone {
		get {
			return (BattleState)state == BattleState.Aim || (BattleState)state >= BattleState.ChangeTurn;
		}
		private set {}
	}

	public void DoAim(StoneController target, Vector3 aimingPosition1, Vector3 aimingPosition2, float power)
	{
		aimingStone = target;
		this.aimingPosition1 = aimingPosition1;
		this.aimingPosition2 = aimingPosition2;
		this.power = power;
		GoNext();
	}

	protected void Awake() 
	{
		Application.targetFrameRate = 30;
		EnableState(typeof(BattleState));
		AddState(BattleState.PrepareStandby, OnPrepareStandby);
		AddState(BattleState.Standby, OnStandby);
		AddState(BattleState.PrepareBattle, OnPrepareBattle);
		AddState(BattleState.Battle, OnBattle);
		AddState(BattleState.PrepareShowTurn, OnPrepareShowTurn);
		AddState(BattleState.ShowTurn, OnShowTurn);
		AddState(BattleState.PrepareAim, OnPrepareAim);
		AddState(BattleState.Aim, OnAim);
		AddState(BattleState.Striking, OnStriking);
		AddState(BattleState.ChangeTurn, OnChangeTurn);
		AddState(BattleState.PrepareResult, OnPrepareResult);
		AddState(BattleState.Result, OnResult);

		if (!PhotonNetwork.connected)
		{
			PhotonNetwork.autoJoinLobby = false;
			PhotonNetwork.ConnectUsingSettings("1");
		}

		PhotonView _pv = GetComponent<PhotonView>();
		network = GetComponent<BattleController>();
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
		if (ownerStone == null) {
			StoneType _stoneType = StoneType.Yellow;
			Dictionary<StoneType, bool> _player = new Dictionary<StoneType, bool>();
			foreach (StoneController _c in StoneController.Find()) {
				_player[_c.stoneType] = true;
			}
			foreach (StoneType _st in System.Enum.GetValues(typeof(StoneType))) {
				if (!_player.ContainsKey(_st)) {
					_stoneType = _st;
					break;
				}
			}
			ownerStone = StoneController.Create(_stoneType);
		}

		SetCamera();
		GoNext();
	}

	protected void OnStandby()
	{
	}

	protected void OnPrepareBattle()
	{
		if (isMyTurn) {
			foreach (StoneController _o in StoneController.Find()) {
				Vector3 _pos = _o.transform.position;
				Quaternion _rot = _o.transform.rotation;
				StoneType _st = _o.stoneType;
				_o.photonView.ownerId = PhotonNetwork.player.ID;
				PhotonNetwork.Destroy(_o.gameObject);

				StoneController _n = null;
				for (int i=0; i<MAX_STONE_NUM; i++) {
					_n = StoneController.Create(_st, new Vector3(_pos.x + Random.value* 1.2f - 0.6f, _pos.y + i, _pos.z + Random.value * 1.2f - 0.6f), _rot);
					_n.CreateHp();
				}
				
				if (ownerStone.stoneType == _st) {
					ownerStone = _n;
				}
			}
		}
		GoNext(4f);
	}

	protected void OnBattle()
	{
		GoNext();
	}

	protected void OnPrepareShowTurn()
	{
		TurnView.instance.Show();
		GoNext();
	}

	protected void OnShowTurn()
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
			foreach (StoneController _c in StoneController.Find()) {
				StoneController _n = _c.Duplicate(PhotonNetwork.player.ID);
				_n.CreateHp();
				if (ownerStone.stoneType == _n.stoneType) {
					ownerStone = _n;
				}
				PhotonNetwork.Destroy(_c.gameObject);
			}
		}

		SetBasement();
		GoNext();
	}

	protected void OnAim() 
	{
		if (isMyTurn) {

			if (InputInterface.isClick()) {
				GameObject _obj = RaycastEx.GetObjectByName("Player", true);
				if (_obj != null) {
					StoneController _c = _obj.GetComponent<StoneController>();
					if (_c.stoneType == turn) {
						aimingStone = _c;
						aimingPosition1 = InputInterface.GetReverseProjection(aimingStone.gameObject);
						aiming = PhotonNetwork.Instantiate("Common/Prefabs/Arrow", new Vector3(aimingStone.transform.position.x, 0f, aimingStone.transform.position.z) , Quaternion.identity, 0);
					}
				}
			}
			if (aimingStone != null && InputInterface.isClickEnd()) {
				aimingPosition2 = InputInterface.GetReverseProjection(aimingStone.gameObject);
				if (aiming != null) {
					PhotonNetwork.Destroy(aiming);
					aiming = null;
				}
			}
			if (aimingStone != null && InputInterface.isMove()) {
				if (aiming != null) {
					Vector3 _dragPos = InputInterface.GetReverseProjection(aimingStone.gameObject);
					aiming.transform.rotation = Quaternion.LookRotation(aimingPosition1 - _dragPos);
					power = Mathf.Clamp(Vector3.Distance(aimingPosition1, _dragPos), 0f, MAX_POWOR);
					aiming.transform.localScale = new Vector3(1f, 0f, power * 2f);
				}
			} else {
				if (power > 0.1f) {
					aimingStone.SendForcing(aimingPosition1, aimingPosition2, power);
				}
			}
		}
	}

	protected void OnFixedStrike()
	{
		Vector3 _power = (aimingPosition1 - aimingPosition2).normalized * power * 800f;
		aimingStone.rigidbody.AddForceAtPosition(_power, new Vector3(0f, 0f, -1f), ForceMode.Impulse);
		aimingPosition1 = Vector3.zero;
		aimingPosition2 = Vector3.zero;
		power = 0f;
		aimingStone = null;
		GoNext();
	}

	protected void OnStriking()
	{
		foreach (StoneController _c in StoneController.Find()) {
			if (_c.isMoving) {
				return;
			}
		}
		GoNext();
	}

	protected void OnChangeTurn()
	{
		if (isMyTurn) {
			StoneController[] _all = StoneController.FindAlive();
			if (_all.Length == 0) {
				// draw
				state = BattleState.PrepareResult;
				network.SendBattleState();
			} else {
				StoneType _next = _all[0].stoneType;
				foreach (StoneController _c in _all) {
					if ((int)_c.stoneType > (int)turn) {
						_next = _c.stoneType;
						break;
					}
				}

				if (turn == _next || StoneController.Find((_c) => {return _c.isAlive && _c.stoneType != _next;}).Length == 0) {
					state = BattleState.PrepareResult;
					network.SendBattleState();
				} else {
					turn = _next;
					network.SendChangingTurn();
				}
			}
		}
		Debug.Log ("OnChangeTurn:" + turn);
	}

	protected void OnPrepareResult()
	{
		ResultView.instance.Show();
		GoNext();
	}

	protected void OnResult()
	{
	}

	// This is one of the callback/event methods called by PUN (read more in PhotonNetworkingMessage enumeration)
	protected void OnConnectedToMaster()
	{
		Debug.Log ("OnConnectedToMaster");
		PhotonNetwork.JoinRandomRoom();
	}
	
	// This is one of the callback/event methods called by PUN (read more in PhotonNetworkingMessage enumeration)
	protected void OnPhotonRandomJoinFailed()
	{
		Debug.Log ("OnPhotonRandomJoinFailed");
		PhotonNetwork.CreateRoom(null, true, true, 4);
	}
	
	// This is one of the callback/event methods called by PUN (read more in PhotonNetworkingMessage enumeration)
	protected void OnJoinedRoom()
	{
		Debug.Log ("OnJoinedRoom");
		StandbyView.instance.Show();
		state = BattleState.PrepareStandby;
	}
	
	// This is one of the callback/event methods called by PUN (read more in PhotonNetworkingMessage enumeration)
	protected void OnCreatedRoom()
	{
		Debug.Log ("OnCreatedRoom");
		ownerStone = StoneController.Create(StoneType.Yellow);
	}

	protected void OnLeftRoom()
	{
		Debug.Log("OnLeftRoom (local)");
	}

	protected void OnPhotonPlayerConnected(PhotonPlayer player)
	{
		Debug.Log("OnPhotonPlayerConnected: " + player);
	}
	
	protected void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		Debug.Log("OnPlayerDisconneced: " + player);
	}

	private void SetBasement()
	{

		foreach (GameObject _obj in GameObject.FindGameObjectsWithTag("Goal")) {
			StoneType _goalsStoneType = (StoneType)StoneType.Parse(typeof(StoneType), _obj.transform.parent.name);
			_obj.renderer.enabled = (turn != _goalsStoneType); 
			_obj.collider.enabled = _obj.renderer.enabled;
		}
	}

	private void SetCamera()
	{
		foreach (Transform _t in transform.Find("/Camera")) {
			_t.camera.enabled = (_t.name == ownerStone.stoneType.ToString());
		}
	}
}
