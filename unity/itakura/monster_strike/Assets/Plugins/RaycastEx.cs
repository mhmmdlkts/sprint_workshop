using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum RaycastType {
	Collider,
	Texture,
	Both,
}

public class RaycastEx : MonoBehaviour
{
	public static RaycastHit[] GetAll(RaycastType type = RaycastType.Collider, int layerMask = 0)
	{
		Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		switch (type) {
		case RaycastType.Both:
			return RaycastAllByCondition(layerMask);
		case RaycastType.Texture:
			return RaycastAllVisible(layerMask);
		case RaycastType.Collider:
		default:
			return Physics.RaycastAll(_ray);
		}
	}
	
	private static readonly RaycastHit[] RAYCAST_HIT_EMPTY = new RaycastHit[] {};
	public static RaycastHit[] RaycastAllByCondition(int layerMask = 0)
	{
		if (Camera.main == null) {
			return RAYCAST_HIT_EMPTY;
		}
		Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit[] _hitAll = (layerMask == 0) ? Physics.RaycastAll(_ray) : Physics.RaycastAll(_ray, Mathf.Infinity, layerMask);
		List<RaycastHit> _hitObjects = new List<RaycastHit>();
		foreach (RaycastHit hit in _hitAll) {
			MeshCollider _meshCollider = hit.collider as MeshCollider;
			if (_meshCollider) {
				if (CheckByTextureAlpha(hit, 0f)) {
					_hitObjects.Add(hit);
				}
			} else {
				_hitObjects.Add(hit);
			}
		}
		Sort(ref _hitObjects);	
		return _hitObjects.ToArray();
	}
	
	private static void Sort(ref List<RaycastHit> list)
	{
		list.Sort((a, b) => Mathf.FloorToInt(a.distance - b.distance));
	}

	public static RaycastHit[] RaycastAllVisible(int layerMask = 0)
	{
		if (Camera.main == null) {
			return RAYCAST_HIT_EMPTY;
		}
		Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit[] _hitAll = (layerMask == 0) ? Physics.RaycastAll(_ray) : Physics.RaycastAll(_ray, Mathf.Infinity, layerMask);
		List<RaycastHit> _hitObjects = new List<RaycastHit>();
		foreach (RaycastHit hit in _hitAll) {
			if (CheckByTextureAlpha(hit, 0f)) {
				_hitObjects.Add(hit);
			}
		}
		Sort(ref _hitObjects);	
		return _hitObjects.ToArray();
	}

	public static bool CheckByTextureAlpha(RaycastHit hit, float alpha)
	{
		Renderer _hitRender = hit.collider.renderer;
		MeshCollider _meshCollider = hit.collider as MeshCollider;
		if (_hitRender == null ||
				_hitRender.sharedMaterial == null ||
				_hitRender.sharedMaterial.mainTexture == null ||
				_meshCollider == null
				) {
			return false;
		}
		Texture2D _hitTex = _hitRender.sharedMaterial.mainTexture as Texture2D;
		Vector2 _pixelUV = hit.textureCoord;
		_pixelUV = new Vector2(_pixelUV.x * _hitTex.width, _pixelUV.y * _hitTex.height);
		
		try {
			Color _pixelValue = _hitTex.GetPixel((int)_pixelUV.x, (int)_pixelUV.y);
			if (_pixelValue.a > alpha) {
				return true;
			}
			if (_pixelValue.a > alpha) {
				return true;
			}
			return false;
		} catch (UnityException) {
			return true;
		}
	}

	public static GameObject GetObjectByName(string name, bool tag = false, RaycastType type = RaycastType.Collider)
	{
		RaycastHit[] _hits = GetAll(type);
		GameObject _object = null;
		foreach (RaycastHit _hit in _hits) {
			string _name = (tag == true) ? _hit.transform.gameObject.tag : _hit.transform.gameObject.name;
			if (_name == name) {
				_object = _hit.transform.gameObject;
				break;
			}
		}
		return _object;
	}

	public static Dictionary<string, GameObject> GetComponentsByTags(Dictionary<string, int> targets, RaycastType type = RaycastType.Collider)
	{
		if (Camera.main == null) {
			return new Dictionary<string, GameObject>();
		}

		RaycastHit[] _hits = GetAll(type);

		Dictionary<string, GameObject> _result = new Dictionary<string, GameObject>();

		foreach (RaycastHit _hit in _hits) {
			string _tag = _hit.transform.gameObject.tag;
			if (targets.ContainsKey(_tag)) {
				GameObject _hitObject = _hit.transform.gameObject;
				int _depth = targets[_tag];
				if (_depth > 0) {
					while (_hitObject.transform.parent != null) {
						if (_depth-- <= 0) { break; }
						_hitObject = _hitObject.transform.parent.gameObject;
					}
				}
				if (!_result.ContainsKey(_tag)) {
					_result.Add(_tag, _hitObject);
				}
			}
		}
		return _result;
	}

	public static T GetComponentByName<T>(string name, bool tag = false, int max_parent = 0, RaycastType type = RaycastType.Collider, string layer = default(string))
		where T : Component
	{
		if (Camera.main == null) {
			return default(T);
		}

		RaycastHit[] _hits;
		if (layer == default(string)) {
			_hits = GetAll(type);
		} else {
			int _layerMask = 1 << LayerMask.NameToLayer(layer);
			_hits = GetAll(type, _layerMask);
		}
		List<T> _results = new List<T>();

		foreach (RaycastHit _hit in _hits) {
 			string _name = tag ? _hit.transform.gameObject.tag : _hit.transform.gameObject.name;
			if (_name == name) {
				T _object = default(T);
				GameObject _hitObject = _hit.transform.gameObject;
				if (max_parent == 0) {
					_object = _hitObject.GetComponent<T>();
				} else {
					for (int i = 0; i < max_parent; i++) {
						if (_hitObject.transform.parent == null) {
							break;
						}

						_hitObject = _hitObject.transform.parent.gameObject;
						if (_hitObject == null) {
							break;
						}

						_object = _hitObject.GetComponent<T>();
						if (_object != null) {
							_results.Add(_object);
						}
					}
					continue;
				}
				if (_object != null) {
					_results.Add(_object);
				}
			}
		}
		T _result = default(T);
		foreach (T _o in _results) {
			if (_result == default(T) || _o.transform.localPosition.z < _result.transform.localPosition.z) {
				_result = _o;
			}
		}
		return _result;
	}

}
