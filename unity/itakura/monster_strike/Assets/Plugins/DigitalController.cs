using UnityEngine;
using System.Collections;

public class DigitalController : BaseObject
{
	private Texture2D texture;
	private int digitNumber;
	private int val;
	Mesh mesh;
	Vector3[] vertecs;
	int[] triangles;
	Vector2[] uvs;
	bool isUpdate;
	bool paddingZero;
	
	public static DigitalController Attach(Transform digitalTransform)
	{
		DigitalController _controller = digitalTransform.GetComponent<DigitalController>();
		if (_controller == null) {
			_controller = digitalTransform.gameObject.AddComponent<DigitalController>();
		}
		return _controller;
	}
	
	public void Set(int digitNumber, bool paddingZero = true, int defaultVal=0)
	{
		this.digitNumber = digitNumber;
		this.paddingZero = paddingZero;
		this.val = defaultVal;
		isUpdate = true;
	}
	
	public void SetValue(int val)
	{
		this.val = val;
		isUpdate = true;
	}
	
	public void AddValue(int val, float startTime=0f)
	{
		// TODO: counter animation
		this.val += val;
		isUpdate = true;
	}
	
	public int GetValue()
	{
		return val;
	}
	
	void Start()
	{
		mesh = gameObject.GetComponent<MeshFilter>().mesh;
		mesh.vertices = CreateVertecs();
		mesh.triangles = CreateTriangles();
		Update();
	}
	
	void Update()
	{
		if (isUpdate) {
			mesh.uv = CreateUvs();
			isUpdate = false;
		}
	}
	
	private Vector3[] CreateVertecs()
	{
		Vector3[] _vertecs;
		/* 
		 	mesh.vertices = { 
				{-0.5, 0,  0.5},
				{ 0.5, 0, -0.5},
				{-0.5, 0, -0.5},
				{ 0.5, 0,  0.5},
			}
		*/		
		
		float _w = mesh.vertices[3].x - mesh.vertices[0].x;
		// float _h = mesh.vertices[0].z - mesh.vertices[2].z;
		
		_vertecs = new Vector3[digitNumber*4];
		for (int _d = 0; _d < digitNumber; _d++) {
			int _i = _d * 4;
			_vertecs[_i + 0] = new Vector3(mesh.vertices[0].x, 0f, mesh.vertices[0].z + (_w / digitNumber) * _d);
			_vertecs[_i + 1] = new Vector3(mesh.vertices[0].x + _w, 0f, mesh.vertices[0].z + (_w / digitNumber) * (_d + 1));
			_vertecs[_i + 2] = new Vector3(mesh.vertices[0].x + _w, 0f, mesh.vertices[0].z + (_w / digitNumber) * _d); 
			_vertecs[_i + 3] = new Vector3(mesh.vertices[0].x, 0f, mesh.vertices[0].z + (_w / digitNumber) * (_d + 1));
		}
		return _vertecs;
	}

	private int[] CreateTriangles()
	{
		int[] _triangles;
		_triangles = new int[digitNumber*6];
		for (int _d = 0; _d < digitNumber; _d++) {
			int _i = _d * 6;
			int _j = _d * 4;
			_triangles[_i + 0] = 0 + _j;
			_triangles[_i + 1] = 1 + _j;
			_triangles[_i + 2] = 2 + _j;
			_triangles[_i + 3] = 0 + _j;
			_triangles[_i + 4] = 3 + _j;
			_triangles[_i + 5] = 1 + _j;
		}
		return _triangles;
	}
	
	private Vector2[] CreateUvs()
	{
		Vector2[] _uvs;
		_uvs = new Vector2[digitNumber*4];
		
		string _vs = val.ToString().PadLeft(digitNumber, '0');
		bool _headZero = true;
		for (int _d = 0; _d < digitNumber; _d++) {
			int _i = (digitNumber - _d - 1) * 4;
			float _v = float.Parse(_vs[_d].ToString());
			if (_headZero && _v != 0) {
				_headZero = false;
			}
			if (_v != 0f || paddingZero || !_headZero || _d == digitNumber - 1) {
				_uvs[_i + 0] = new Vector2((_v + 1) / 10f, 0f);
				_uvs[_i + 1] = new Vector2(_v / 10f, 1f);
				_uvs[_i + 2] = new Vector2((_v + 1) / 10f, 1f);
				_uvs[_i + 3] = new Vector2(_v / 10f, 0f);
			} else {
				_uvs[_i + 0] = Vector2.zero;
				_uvs[_i + 1] = Vector2.zero;
				_uvs[_i + 2] = Vector2.zero;
				_uvs[_i + 3] = Vector2.zero;
			}
		}		
		return _uvs;
	}
}

