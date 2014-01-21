using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public enum StoneType {
	Yellow = 0,
	Red,
	Green,
	Blue
}

[RequireComponent(typeof(PhotonView))]
public class StoneNetwork : Photon.MonoBehaviour
{
	private static Vector3[] STONE_POSITIONS = new Vector3[] {
		new Vector3(1f, 0f, -2f),
		new Vector3(5f, 0, 2f), 
		new Vector3(1f, 0, 2f), 
		new Vector3(5f, 0, -2f)
	};

    private Vector3 latestCorrectPos;
    private Vector3 onUpdatePos;
    private float fraction;

	public StoneType stoneType;
	public int playerId;

	public static StoneNetwork Create(StoneType stoneType)
	{
		return Create(stoneType, STONE_POSITIONS[(int)stoneType], Quaternion.identity);
	}

	public static StoneNetwork Create(StoneType stoneType, Vector3 position, Quaternion rotation) 
	{
		GameObject _obj = PhotonNetwork.Instantiate("Stone/Prefabs/" + stoneType.ToString(), position, rotation, 0);
		StoneNetwork _c = _obj.GetComponent<StoneNetwork>();
		_c.playerId = PhotonNetwork.player.ID;
		_c.stoneType = stoneType;
		return _c;
	}


    void Awake()
    {
		Debug.Log ("StoneNetwork:Awake");
        if (photonView.isMine)
        {
            this.enabled = false;   // due to this, Update() is not called on the owner client.
        }

        latestCorrectPos = transform.position;
        onUpdatePos = transform.position;

		PhotonView _pv = GetComponent<PhotonView>();
		_pv.observed = this;
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
		Debug.Log ("OnPhotonSerializeView");
        if (stream.isWriting)
        {
            Vector3 pos = transform.localPosition;
            Quaternion rot = transform.localRotation;
            stream.Serialize(ref pos);
            stream.Serialize(ref rot);
			stream.Serialize(ref playerId);
			int _st = (int)stoneType;
			stream.Serialize(ref _st);
        }
        else
        {
            // Receive latest state information
            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;
			int _st = (int)stoneType;

            stream.Serialize(ref pos);
            stream.Serialize(ref rot);
			stream.Serialize(ref playerId);
			stream.Serialize(ref _st);

            latestCorrectPos = pos;                 // save this to move towards it in FixedUpdate()
            onUpdatePos = transform.localPosition;  // we interpolate from here to latestCorrectPos
            fraction = 0;                           // reset the fraction we alreay moved. see Update()
            transform.localRotation = rot;          // this sample doesn't smooth rotation
			stoneType = (StoneType)_st;
        }
    }

    void Update()
    {
		Debug.Log ("Update");
        // We get 10 updates per sec. sometimes a few less or one or two more, depending on variation of lag.
        // Due to that we want to reach the correct position in a little over 100ms. This way, we usually avoid a stop.
        // Lerp() gets a fraction value between 0 and 1. This is how far we went from A to B.
        //
        // Our fraction variable would reach 1 in 100ms if we multiply deltaTime by 10.
        // We want it to take a bit longer, so we multiply with 9 instead.

        fraction = fraction + Time.deltaTime * 9;
        transform.localPosition = Vector3.Lerp(onUpdatePos, latestCorrectPos, fraction);    // set our pos between A and B
    }
}
