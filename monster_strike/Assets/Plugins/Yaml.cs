using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;

public class Yaml {
	public static Regex YAML_IGNORE = new System.Text.RegularExpressions.Regex("(#.*$| )");
	public Dictionary<string, object> values {get; private set;}
	
	public void Load(string file)
	{
		if (values == null) {
			values = new Dictionary<string, object>();
		}
#if USING_ASSET_RESOURCES		
		TextAsset _textAsset = null;
		if (AssetResources.instance != null) {
			_textAsset = (TextAsset)AssetResources.instance.Load(file);
		} else {
			_textAsset = (TextAsset)Resources.Load(file);
		}
#else
		TextAsset _textAsset = null;
		_textAsset = (TextAsset)Resources.Load(file);
#endif
		if (_textAsset != null) {
			string result = _textAsset.text;
			
			result = result.Replace('\r', '\n');
			string[] lines = result.Split("\n" [0]);
			
			for(int i = 0; i < lines.Length; i++)
			{
				string line = lines[i];
				string data = YAML_IGNORE.Replace(line,"");
				
				List<string> items = split(data);
				if (items.Count == 0) {
					continue;
				}
				if (items.Count == 2) {
					values[items[0]] = items[1];
					continue;
				}
				
				Dictionary<string, string> v = new Dictionary<string, string>();
				while (i + 1 < lines.Length) {
					string nextLine = lines[i + 1];
					if (!nextLine.StartsWith(" ")) {
						break;
					}
					var childData = YAML_IGNORE.Replace(nextLine, "");
					List<string> childItems = split(childData);
					if (childItems.Count == 0) {
						continue;
					}
					if (childItems.Count == 2) {
						v[childItems[0]] = childItems[1];
					}
					i++;
				}
				if (v.Count > 0) {
					values[items[0]] = v;
				}
			}
		}
		Resources.UnloadAsset(_textAsset);
	}
	
	private List<string> split(string data) {
		List<string> items = new List<string>();
		int sepratorIndex = data.IndexOf(":");
		if (sepratorIndex < 0) {
			return items;
		}
		string key = data.Substring(0, sepratorIndex);
		if (key.Length == 0) {
			return items;
		}
		items.Add(key);
		if (sepratorIndex == data.Length - 1) {
			return items;
		}
		string item = data.Substring(sepratorIndex + 1);
		if (item.Trim().Length > 0) {
			items.Add(item);
			return items;
		}
		return items;
	}
	
	public void setDefault(string key, string v)
	{
		if (values == null) {
			values = new Dictionary<string, object>();
		}
		if (!values.ContainsKey(key)) {
			values[key] = v;
		}
	}
	
	public string v (string key, string defaultValue = null)
	{
		if (!values.ContainsKey(key)) {
			return defaultValue;
		}
		return values[key].ToString();
	}

	public string this[string key]
	{
		get { return v(key); }
		private set {}
	}

	public T Enum<T>(string key, T defaultValue)
	{
		string _value = v(key);
		if (_value == null) {
			return defaultValue;
		} else {
			int _num;
			if (int.TryParse(_value, out _num)) {
				return (T)(object)_num;
			}
			
			return (T)System.Enum.Parse(typeof(T), _value);
		}
	}
	
	public int n(string key, int defaultValue)
	{
		string _value = v(key);
		if (_value == null) {
			return defaultValue;
		} else {
			return int.Parse(_value);
		}
	}
	
	public float f(string key, float defaultValue)
	{
		string _value = v(key);
		if (_value == null) {
			return defaultValue;
		} else {
			return float.Parse(_value);
		}
	}
	
	public bool b(string key, bool defaultValue)
	{
		string _value = v(key);
		if (_value == null) {
			return defaultValue;
		} else {
			return bool.Parse(_value);
		}
	}
	
	public string[] a(string key) 
	{
		string _value = v(key);
		if (_value == null) {
			return new string[]{};
		}
		return _value.Split(',');
	}
	
	public static readonly Dictionary<string, string> zero = new Dictionary<string, string>();
	public Dictionary<string, string> hash(string key) 
	{
		if (!values.ContainsKey(key)) {
			return zero;
		} else {
			object _value = values[key];
			return (Dictionary<string, string>) _value;
		}
	}
	
	public Vector3 vector3(string key, Vector3 defaultValue) 
	{
		string[] _a = a(key);
		if (_a.Length != 3) {
			return defaultValue;
		}
		return new Vector3(float.Parse(_a[0]), float.Parse(_a[1]), float.Parse(_a[2]));
	}	
}
