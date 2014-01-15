using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonoInheritance : MonoBehaviour
{
	protected bool isPause = false;
	
	public virtual void Pause()
	{
		isPause = true;
	}
	public virtual void Restart()
	{
		isPause = false;
	}
	
	protected PausableCoroutine StartPausableCoroutine(IEnumerator enumerator)
	{
		return StartPausableCoroutine(enumerator,()=>{});
	}
	
	protected PausableCoroutine StartPausableCoroutine(IEnumerator enumerator,System.Action callback)
	{
		return new PausableCoroutine(new PausableEnumerator(enumerator,()=>{return isPause;},callback),this);
	}
	private IEnumerator Dummy()
	{
		yield return null;
	}
	public void Stop()
	{
		gameObject.SetActive(false);
		gameObject.SetActive(true);
	}
}
