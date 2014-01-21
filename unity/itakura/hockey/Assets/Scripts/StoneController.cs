using UnityEngine;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public enum StoneType {
	Yellow = 0,
	Red,
	Green,
	Blue
}

[RequireComponent(typeof(PhotonView))]
public class StoneController : Photon.MonoBehaviour
{
	private static Vector3[] STONE_POSITIONS = new Vector3[] {
		new Vector3(3f, 0f, -1.5f),
		new Vector3(3f, 0, 1.5f), 
		new Vector3(1.5f, 0, 0f), 
		new Vector3(4.5f, 0, 0f)
	};

    private Vector3 latestCorrectPos;
    private Vector3 onUpdatePos;
    private float fraction;

	public StoneType stoneType;
	public int playerId;

	public bool isSync {
		get { return isSync_; }
		private set { isSync_ = value; }
	}

	private bool isSync_ = true;

	private const int MAX_HIT_POINT = 40;
	public int attackPoint = 10;
	public int hitPoint = MAX_HIT_POINT;


	private GameObject hp;

	public static StoneController Create(StoneType stoneType, bool sync = true)
	{
		return Create(stoneType, STONE_POSITIONS[(int)stoneType], Quaternion.identity, sync);
	}

	public static StoneController Create(StoneType stoneType, Vector3 position, Quaternion rotation, bool sync = true) 
	{
		GameObject _obj;
		if (sync) {
			_obj = PhotonNetwork.Instantiate("Stone/Prefabs/" + stoneType.ToString(), position, rotation, 0);
		} else {
			GameObject _prefab = (GameObject)Resources.Load("Stone/Prefabs/" + stoneType.ToString(), typeof(GameObject));
			_obj = (GameObject)Instantiate(_prefab, position, rotation);
		}
		StoneController _c = _obj.GetComponent<StoneController>();
		_c.isSync = sync;
		_c.playerId = PhotonNetwork.player.ID;
		_c.stoneType = stoneType;
		return _c;
	}

	public static StoneController[] Find(System.Func<StoneController, bool> condition=null)
	{
		List<StoneController> _stones = new List<StoneController>();
		foreach (GameObject _obj in GameObject.FindGameObjectsWithTag("Player")) {
			StoneController _c = null;
			bool _adding = true;
			if (condition != null) {
				_c = _obj.GetComponent<StoneController>();
				_adding = condition(_c);
			}

			if (_adding) {
				if (_c == null) {
					_c = _obj.GetComponent<StoneController>();
				}
				_stones.Add(_c);
			}
		}
		_stones.Sort((a, b) => (int)a.stoneType - (int)b.stoneType);
		return _stones.ToArray();
	}

	public static StoneController[] Find(StoneType stoneType)
	{
		return Find ((_c) => {return _c.stoneType == stoneType;});
	}

	public static StoneController[] FindAlive()
	{
		return Find ((_c) => {return _c.isAlive;});
	}

	public bool isMoving {
		get {
			return (rigidbody.velocity != Vector3.zero || rigidbody.angularVelocity != Vector3.zero) && !isDropped;
		}
		private set {}
	}

	public bool isDropped {
		get {
			return transform.position.y < -1f;
		}
	}

	public bool isAlive {
		get {
			return hitPoint > 0 && !isDropped;
		}
		private set {}
	}

	public StoneController Duplicate(int ownerId)
	{
		Vector3 _pos = transform.position;
		Quaternion _rot = transform.rotation;
		StoneType _st = stoneType;
		photonView.ownerId = ownerId;
		int _hitPoint = hitPoint;
		int _attackPoint = attackPoint;

		StoneController _n = Create(_st, _pos, _rot);
		_n.hitPoint = _hitPoint;
		_n.attackPoint = _attackPoint;
		return _n;
	}

	public void CreateHp()
	{
		if (isAlive) {
			GameObject _prefab = (GameObject)Resources.Load("Stone/Prefabs/Hp");
			hp = (GameObject)Instantiate(_prefab, transform.localPosition, Quaternion.identity);
		}
	}
	
	void Awake()
    {
        latestCorrectPos = transform.position;
        onUpdatePos = transform.position;

		PhotonView _pv = GetComponent<PhotonView>();
		_pv.observed = this;
	}


	public void SendForcing(Vector3 aimingPosition1, Vector3 aimingPosition2, float power)
	{
		Debug.Log ("StoneController:SendForcing");
		photonView.RPC("ReceiveForcing", PhotonTargets.All, aimingPosition1, aimingPosition2, power);
	}
	[RPC]
	void ReceiveForcing(Vector3 aimingPosition1, Vector3 aimingPosition2, float power)
	{
		Debug.Log ("StoneController:ReceiveForcing");
		BattleManager.instance.DoAim(this, aimingPosition1, aimingPosition2, power);
	}
	
	
	/// <summary>
    /// While script is observed (in a PhotonView), this is called by PUN with a stream to write or read.
    /// </summary>
    /// <remarks>
    /// The property stream.isWriting is true for the owner of a PhotonView. This is the only client that
    /// should write into the stream. Others will receive the content written by the owner and can read it.
    /// 
    /// Note: Send only what you actually want to consume/use, too!
    /// Note: If the owner doesn't write something into the stream, PUN won't send anything.
    /// </remarks>
    /// <param name="stream">Read or write stream to pass state of this GameObject (or whatever else).</param>
    /// <param name="info">Some info about the sender of this stream, who is the owner of this PhotonView (and GameObject).</param>
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
		if (!isSync) {
			return;
		}

		Debug.Log ("OnPhotonSerializeView");
        if (stream.isWriting)
        {
            Vector3 pos = transform.localPosition;
            Quaternion rot = transform.localRotation;
			int _st = (int)stoneType;
			int _hitPoint = hitPoint;
			stream.Serialize(ref pos);
            stream.Serialize(ref rot);
			stream.Serialize(ref playerId);
			stream.Serialize(ref _st);
			stream.Serialize(ref _hitPoint);
		}
        else
        {
            // Receive latest state information
            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;
			int _st = (int)stoneType;
			int _hitPoint = hitPoint;

            stream.Serialize(ref pos);
            stream.Serialize(ref rot);
			stream.Serialize(ref playerId);
			stream.Serialize(ref _st);
			stream.Serialize(ref _hitPoint);

			latestCorrectPos = pos;                 // save this to move towards it in FixedUpdate()
			onUpdatePos = transform.localPosition;  // we interpolate from here to latestCorrectPos
			fraction = 0;                           // reset the fraction we alreay moved. see Update()

			if (BattleManager.instance.isSyncStone) {
				transform.localRotation = rot;          // this sample doesn't smooth rotation
			}

			stoneType = (StoneType)_st;
			hitPoint = _hitPoint;
		}
    }

    void Update()
    {
		Debug.Log ("Update:" + transform.position);

		if (!isSync) {
			return;
		}

		if (hp == null) {
			CreateHp();
		}

		if (hp != null) {
			if (isAlive) {
				hp.transform.position = new Vector3(transform.position.x, 1f, transform.position.z);
				Transform _bar = hp.transform.Find("Bar");
				float _hpRate = (float)hitPoint / (float)MAX_HIT_POINT;
				_bar.localScale = new Vector3(0.95f * (_hpRate), _bar.localScale.y, _bar.localScale.z);
				_bar.localPosition = new Vector3(- (1f - _hpRate) / 2, _bar.localPosition.y, _bar.localPosition.z);
			} else {
				Destroy(hp);
				hp = null;
			}
		}

		if (!photonView.isMine && BattleManager.instance.isSyncStone) {
	        fraction = fraction + Time.deltaTime * 9;
	        transform.localPosition = Vector3.Lerp(onUpdatePos, latestCorrectPos, fraction);    // set our pos between A and B
		}
    }

	private void OnCollisionEnter(Collision collision)
	{
		if (BattleManager.instance.GetState() > BattleState.ShowTurn) {
			if (collision.gameObject.tag == "Player") {
				StoneController _other = collision.gameObject.GetComponent<StoneController>();
				if (_other.stoneType != stoneType && _other.stoneType == BattleManager.instance.turn) {
					hitPoint -= _other.attackPoint;
					if (hitPoint <= 0) {
						transform.localPosition = new Vector3(transform.localPosition.x, -2f, transform.localPosition.z);
					}
				}
			}
		}
	}

	void OnDestroy()
	{
		if (hp != null) {
			Destroy(hp);
			hp = null;
		}
	}
}
