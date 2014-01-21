using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ViewState {
	Standby,
	Prepare,
	ShowIn,
	Show,
	ShowOut,
	PrepareHide,
	Hide,
	SwipeIn,
}

public class View<T> : StatefulObject<T> 
	where T : MonoInheritance
{
	private const float APPEARANCE_TIME = 0.5f;
	private const float CLOSED_DISTANCE = 0.001f;
	
	protected Dictionary<string, Part> parts {
		get { return parts_; }
		private set {}
	}
	private Dictionary<string, Part> parts_ = new Dictionary<string, Part>();
	private List<string> dynamicParts = new List<string>();
	
	private const int OUTER_ID = 0;
	private const int INNER_ID = 1;
	
	public new Camera camera {
		get {
			if (camera_ == null) {
				camera_ = transform.Find("Camera").camera;
			}
			return camera_;
		}
		private set {}
	}
	private Camera camera_;

	public new Renderer renderer {
		get {
			if (!isCheckRenderer) {
				renderer_ = GetComponent<Renderer>();
				isCheckRenderer = true;
			}
			return renderer_;
		}
		private set {}
	}
	private Renderer renderer_;
	private bool isCheckRenderer = false;
	
	private GameObject tapObject;
	private GameObject swipeObject;
	
	public bool isTapHiding {
		get { return _isTapHiding; }
		set { _isTapHiding = value; }
	}
	private bool _isTapHiding = true;
	
	private System.Action hiddenAction = null;
	
	float showInTime;
	float showOutTime;
	float swipeInTime;
	
	private Vector3 swipePosition;
	Vector3 swipeVelocity;
	
	private bool isSwiped = false;
	
	private bool isDisableAction = false;
	
	protected void Awake()
	{
		EnableState(typeof(ViewState));
		AddState(ViewState.Standby, OnStandby);
		AddState(ViewState.Prepare, OnPrepare);
		AddState(ViewState.ShowIn, OnShowIn);
		AddState(ViewState.Show, OnShow);
		AddState(ViewState.ShowOut, OnShowOut);
		AddState(ViewState.PrepareHide, OnPrepareHide);
		AddState(ViewState.Hide, OnHide);
		AddState(ViewState.SwipeIn, OnSwipeIn);
		camera.enabled = false;
		if (renderer != null) {
			renderer.enabled = false;
		}
		
	}
	
	public void AddPart(Transform part, InMotion inMotion, Vector3[] positions)
	{
		parts[part.name] = new Part(part.name, part, inMotion, positions);
		part.localPosition = positions[OUTER_ID];
		part.gameObject.SetActive(false);
	}
	
	public void AddPart(Transform part, InMotion inMotion)
	{
		Vector3[] _positions = new Vector3[2];
		float _x = 0f;
		float _z = 0f;
		switch (inMotion) {
		case InMotion.Top:
			_x = 0.7f;
			break;
		case InMotion.Bottom:
			_x = -0.7f;
			break;
		case InMotion.Left:
			_z = 0.35f;
			break;
		case InMotion.Right:
			_z = -0.35f;
			break;
		}
		
		_positions[OUTER_ID] = new Vector3(part.transform.localPosition.x + _x, part.transform.localPosition.y, part.transform.localPosition.z + _z);
		_positions[INNER_ID] = part.transform.localPosition;
		AddPart(part, inMotion, _positions);
	}
	
	public void AddDynamicPart(Transform part, InMotion inMotion, Vector3[] positions)
	{
		dynamicParts.Add(part.name);
		AddPart(part, inMotion, positions);
	}
	
	public void AddDynamicPart(Transform part, InMotion inMotion)
	{
		dynamicParts.Add(part.name);
		AddPart(part, inMotion);
	}	
	
	public void ClearDynamicParts()
	{
		foreach (string _name in dynamicParts) {
			parts.Remove(_name);
		}
		dynamicParts = new List<string>();
	}
	
	public void SetPartActive(string name, bool isActive)
	{
		parts[name].isActive = isActive;
		parts[name].transform.gameObject.SetActive(isActive);
	}

	public void AddAction(string name, ActionId actionId, System.Action<Transform> action) 
	{
		if (parts[name].actions.Count == 0) {
			parts[name].transform.gameObject.AddComponent<MeshCollider>();
		}
		parts[name].actions[actionId] = action;
	}
	
	public void AddSwipe(string name, Vector3[] positions)
	{
		if (parts[name].actions.Count == 0) {
			parts[name].transform.gameObject.AddComponent<MeshCollider>();
		}
		parts[name].swipePositions = positions;
	}
	
	public void Show() {
		gameObject.SetActive(true);
		SetPartsActiveWhenShow();
		state = ViewState.Prepare;
		camera.enabled = true;
	}
	
	public bool IsShow()
	{
		ViewState _viewState = (ViewState)state;
		return (_viewState == ViewState.Prepare || _viewState == ViewState.ShowIn || _viewState == ViewState.Show || _viewState == ViewState.ShowOut);
	}
	
	public bool IsClick()
	{
		return (InputInterface.isClick() && GetTapObject() != null);
	}

	public void Hide(System.Action hiddenAction)
	{
		this.hiddenAction = hiddenAction;
		Hide();
	}
	
	public void Hide(bool immediately=false)
	{
		if (immediately) {
			state = ViewState.PrepareHide;	
		} else {
			if ((ViewState)state == ViewState.ShowIn || (ViewState)state == ViewState.Show) {
				state = ViewState.ShowOut;
			} else if ((ViewState)state == ViewState.Prepare) {
				state = ViewState.Hide;
			}
		}
	}

	public void EnableAction()
	{
		isDisableAction = false;
	}
	
	public void DisableAction()
	{
		isDisableAction = true;
	}

	protected void EnableButton(string name)
	{
		if (parts[name].enableTexture != null) {
			parts[name].transform.renderer.sharedMaterial.mainTexture = parts[name].enableTexture;
		}
		parts[name].enableTexture = null;
	}
	
	protected void DisableButton(string name)
	{
		if (parts[name].enableTexture == null) {
			parts[name].enableTexture = parts[name].transform.renderer.sharedMaterial.mainTexture;
		}
		parts[name].transform.renderer.sharedMaterial = CreateCutoffMaterial();
		parts[name].transform.renderer.sharedMaterial.mainTexture = parts[name].enableTexture;
	}
	
	protected Material CreateCutoffMaterial(float cutoff=0.9f, Color color=default(Color))
	{
		if (color == default(Color)) {
			color = Color.gray;
		}
		
		Shader _shader = Shader.Find("Transparent/Cutout/Soft Edge Unlit");
				
		Material _material = new Material(_shader);
		_material.SetFloat("_Cutoff", cutoff);
		_material.SetColor("_Color", color);
		return _material;
	}
	
	protected virtual void OnStandby()
	{
		gameObject.SetActive(false);
	}
	
	protected virtual void OnPrepare()
	{
		showInTime = 0;
		showOutTime = 0;
		swipeInTime = 0;
		ClearDynamicParts();
		GoNext();
	}
	
	protected virtual void OnShowIn()
	{
		if (showInTime == 0) {
			if (renderer != null) {
				renderer.enabled = true;
			}
			SetPartsActive(true);
		}
		if (!Move(INNER_ID, ref showInTime)) {
			GoNext();
		}
	}
	
	protected virtual void OnShow()
	{
		if (isDisableAction) {
			return;
		}
		
		if (InputInterface.isClick()) {
			tapObject = GetTapObject();

			if (tapObject != null && (!parts.ContainsKey(tapObject.name) || parts[tapObject.name].enableTexture != null)) {
				tapObject = null;
			}
			
			if ((tapObject == null || parts[tapObject.name].actions.Count == 0) && isTapHiding) {
				GoNext();
			}
		} else {
			if (InputInterface.isMove()) {
				if (swipeObject == null) {
					swipeObject = GetSwipeObject();
					if (swipeObject != null) {
						if (parts[swipeObject.name].enableTexture != null) {
							swipeObject = null;
						} else {
							swipePosition = swipeObject.transform.position - camera.ScreenToWorldPoint(Input.mousePosition);
						}
					}
				} 
				if (swipeObject != null && parts[swipeObject.name].swipePositions != null) {
					Vector3 _position = (camera.ScreenToWorldPoint(Input.mousePosition) + swipePosition);
					if (Vector3.Distance(_position, swipeObject.transform.position) > CLOSED_DISTANCE) {
						isSwiped = true;
						Vector3 _swipeDirection = parts[swipeObject.name].swipePositions[1] - parts[swipeObject.name].swipePositions[0];
						Vector3 _nextPosition = Vector3.Project(_position , _swipeDirection) + Vector3.Scale(swipeObject.transform.position, Invert(_swipeDirection));
						swipeVelocity = (_nextPosition - swipeObject.transform.position) / Time.deltaTime;
						swipeObject.transform.position = _nextPosition;
						OnBeginSwipe(swipeObject.transform);
					}
				}
			}			
		}	
		if (InputInterface.isClickEnd()) {
			if (!isSwiped) {
				if (tapObject != null && GetTapObject() == tapObject) {
					if (parts[tapObject.name].actions.ContainsKey(ActionId.Tap)) {
						parts[tapObject.name].actions[ActionId.Tap](tapObject.transform);
					}
					tapObject = null;
				}
				swipeObject = null;
			} else {
				if (swipeVelocity.magnitude > 1f) {
					swipePosition = GetNextPosition(swipeVelocity, parts[swipeObject.name].transform.localPosition, parts[swipeObject.name].swipePositions);
					swipeInTime = APPEARANCE_TIME;
				} else {
					swipePosition = GetClosePosition(parts[swipeObject.name].transform.localPosition, parts[swipeObject.name].swipePositions);
				}
				isSwiped = false;
				GoSwipeIn();				
			}
		}
		
	}
	
	protected virtual void OnShowOut()
	{
		if (!Move(OUTER_ID, ref showOutTime)) {
			GoNext();
		}
	}
	
	protected virtual void OnPrepareHide()
	{
		if (renderer != null) {
			renderer.enabled = false;
		}
		camera.enabled = false;
		gameObject.SetActive(false);
		SetPartsActive(false);
		if (hiddenAction != null) {
			hiddenAction();
			hiddenAction = null;
		}
		GoNext();
	}
	
	protected virtual void OnHide()
	{
	}
	
	protected virtual void OnBeginSwipe(Transform t)
	{
	}
	
	protected virtual void OnSwipeIn()
	{
		if (!Move(swipeObject.transform, swipePosition, ref swipeInTime)) {
			OnFinishSwipe(swipeObject.transform);
			swipeObject = null;
			swipeInTime = 0f;
			GoShow();		
		}
	}
	
	protected virtual void OnFinishSwipe(Transform t)
	{
	}
	
	private bool Move(int position_id, ref float time)
	{
		if (time == 0) {
			time = Time.time;
		}
		float _smoothTime = APPEARANCE_TIME - (Time.time - time);
		foreach (Part _part in parts.Values) {
			_part.transform.gameObject.SetActive(_part.isActive);
			Vector3 _currentVelocity = Vector3.zero;
			_part.transform.localPosition = Vector3.SmoothDamp(_part.transform.localPosition, _part.positions[position_id], ref _currentVelocity, _smoothTime);
		}
		return (_smoothTime > 0);
	}

	private bool Move(Transform t, Vector3 position, ref float time)
	{
		if (time == 0) {
			time = Time.time;
		}
		float _smoothTime = APPEARANCE_TIME - (Time.time - time);
		Vector3 _currentVelocity = Vector3.zero;
		t.localPosition = Vector3.SmoothDamp(t.localPosition, position, ref _currentVelocity, _smoothTime);
		return (_smoothTime > 0);
	}
	
	private Vector3 GetClosePosition(Vector3 targetPosition, Vector3[] positions)
	{
		Vector3 _result = default(Vector3);
		float _minDistance = float.MaxValue;
		foreach (Vector3 _position in positions) {
			float _d = Vector3.Distance(targetPosition, _position);
			if (_d < _minDistance) {
				_result = _position;
				_minDistance = _d;
			}
		}
		return _result;
	}
	
	private Vector3 GetNextPosition(Vector3 velocity, Vector3 targetPosition, Vector3[] positions)
	{
		float _half = Vector3.Distance(positions[0], positions[1]) / 2;
		Vector3 _pos = targetPosition + velocity.normalized * _half;
		return GetClosePosition(_pos, positions);
	}
	
	private void SetPartsActiveWhenShow()
	{
		foreach (Part _part in parts.Values) {
			switch (_part.inMotion) {
			case InMotion.None:
				_part.transform.gameObject.SetActive(true);
				break;
			case InMotion.Hidden:
				_part.transform.gameObject.SetActive(false);
				break;
			}
		}
		if (renderer != null) {
			renderer.enabled = true;
		}
	}
		
	private void SetPartsActive(bool isActive)
	{
		foreach (Part _part in parts.Values) {
			if (_part.inMotion != InMotion.Hidden || !isActive) {
				_part.transform.gameObject.SetActive(isActive);
			}
		}
		if (renderer != null) {
			renderer.enabled = isActive;
		}
	}
	
	private GameObject GetSwipeObject()
	{
		return GetInputObject((target) => {
			 return parts[target.name].swipePositions != null;
		});
	}
	
	private GameObject GetTapObject()
	{
		return GetInputObject((target) => {
			 return parts[target.name].actions.ContainsKey(ActionId.Tap);
		});
	}
	
	private GameObject GetInputObject(System.Func<GameObject, bool> condition) 
	{
		GameObject _inputObject = null;
		
		SetMainCamera();
		RaycastHit[] _hits = RaycastEx.RaycastAllByCondition();
		RevertMainCamera();
		if (_hits.Length > 0) {
			foreach (RaycastHit _ray in _hits) {
				GameObject _target = _ray.collider.gameObject;
				if (_target.layer == LayerMask.NameToLayer("View") && parts.ContainsKey(_target.name) && condition(_target)) {
					_inputObject = _target;
					break;
				}
			}
		}
		
		return _inputObject;
	}
	
	protected void SetMainCamera()
	{
		// この辺りでエラーが出た場合は、ExMainCameraタグを追加すること
		GameObject _oldCamera = GameObject.FindGameObjectWithTag("MainCamera");
		_oldCamera.tag = "ExMainCamera";
		camera.gameObject.tag = "MainCamera";
	}
	
	protected void RevertMainCamera()
	{
		GameObject _oldCamera = GameObject.FindGameObjectWithTag("ExMainCamera");
		camera.gameObject.tag = "Untagged";
		_oldCamera.tag = "MainCamera";
	}
	
	protected void GoShow()
	{
		state = ViewState.Show;
	}
	
	protected void GoSwipeIn()
	{
		state = ViewState.SwipeIn;
	}
	
	private Vector3 Invert(Vector3 v) 
	{
		float _x = 0f, _y = 0f, _z = 0f;
		if (v.x == 0f) {
			_x = 1f;
		}
		if (v.y == 0f) {
			_y = 1f;
		}
		if (v.z == 0f) {
			_z = 1f;
		}
		return new Vector3(_x, _y, _z);
	}
}

public enum InMotion {
	None,
	Left,
	Right,
	Top,
	Bottom,
	Hidden,
}

public enum ActionId {
	Tap,
}

public class Part 
{
	public string name;
	public InMotion inMotion;
	public Transform transform;
	public Vector3[] positions;
	public bool isActive;
	public Dictionary<ActionId, System.Action<Transform>> actions;
	public Vector3[] swipePositions;
	public Texture enableTexture;
	
	public Part(string name, Transform transform, InMotion inMotion, Vector3[] positions)
	{
		this.name = name;
		this.inMotion = inMotion;
		this.transform = transform;
		this.positions = positions;
		this.isActive = true;
		this.actions = new Dictionary<ActionId, System.Action<Transform>>();
		this.swipePositions = null;
		this.enableTexture = null;
	}
}
	