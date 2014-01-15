using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class  PausableEnumerator: IEnumerator
{
	private IEnumerator enumerator;
	private Func<bool> isPause;
	private bool isStop;
	private List<Action> callbacks;
	public PausableEnumerator(IEnumerator enumerator,Func<bool> checker,Action callback=null)
	{
		this.enumerator = enumerator;
		isPause = checker;
		isStop = false;
		this.callbacks = new List<Action>();
		AddCallback(callback);
	}

	public void AddCallback (Action c)
	{
		callbacks.Add(c);
	}
	
	public void Pause() {
		isPause = ()=>true;
	}
	public void UnPause() {
		isPause = ()=>false;
	}
	public void Stop() {
		isStop = true;
	}
	#region IEnumerator implementation
	public bool MoveNext ()
	{
		if (isStop) {
			return false;
		}
		if (isPause()) {
			return true;
		}
		if (enumerator.MoveNext()) {
			return true;
		}else {
			callbacks.ForEach(args=>args());
			callbacks.Clear();
			isStop = true;
			return false;
		}
	}

	public void Reset ()
	{
		enumerator.Reset();
	}

	public object Current {
		get {
			return enumerator.Current;
		}
	}
	#endregion
}



public class  PausableCoroutine
{
	private PausableEnumerator p;
	public PausableCoroutine(PausableEnumerator enumerator,MonoBehaviour self)
	{
		p=enumerator;
		self.StartCoroutine(enumerator);
	}
	public void Stop() {
		p.Stop();
	}
	public bool MoveNext() {
		return p.MoveNext();
	}
}
