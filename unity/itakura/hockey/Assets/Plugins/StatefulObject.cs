using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class StatefulObject<T> : InstanceAttribute<T> 
	where T : MonoInheritance
{
	protected Enum state;
	System.Type stateType;
	Dictionary<Enum, System.Action> actions;
	float waitTime = 0;
	
	public void EnableState(System.Type stateType) 
	{
		this.stateType = stateType;
		state = (Enum)Activator.CreateInstance(stateType);
		actions = new Dictionary<Enum, Action>();
	}
	
	public void AddState(Enum e, System.Action action)
	{
		actions.Add(e, action);
	}

	public void GoNext()
	{
		int _next = Convert.ToInt32(state) + 1;
		state = Enum.Parse(stateType, _next.ToString()) as Enum;	
	}

	public void GoNext(float time)
	{
		Wait(time);
		GoNext();
	}

	public void Wait(float time)
	{
		waitTime = time;
	}
	
	protected void Update()
	{
		if (stateType == null) {
			return;
		}
		if (waitTime > 0) {
			waitTime -= Time.deltaTime;
			if (waitTime < 0) {
				waitTime = 0;
			}
		} else if (actions.ContainsKey(state)) {
			actions[state]();
		}
	}
}

