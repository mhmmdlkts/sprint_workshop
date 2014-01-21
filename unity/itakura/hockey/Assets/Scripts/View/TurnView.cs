using UnityEngine;
using System.Collections;

public class TurnView : View<TurnView>
{
	private StoneController stone = null;

	void Start()
	{
		TurnView.InitInstance();
		AddPart(transform.Find("Turn"), InMotion.Right);
		isTapHiding = false;
	}

	protected override void OnPrepare()
	{
		Debug.Log("TurnView:OnPrepare");
		stone = StoneController.Create(BattleManager.instance.turn, false);
		stone.transform.parent = parts["Turn"].transform;
		stone.transform.localPosition = Vector3.zero;
		stone.transform.localRotation = Quaternion.Euler(0f, 0f, -50f);
		stone.collider.enabled = false;
		stone.rigidbody.useGravity = false;
		stone.gameObject.layer = LayerMask.NameToLayer("View");
		stone.gameObject.tag = "";

		base.OnPrepare();
	}

	protected override void OnPrepareHide()
	{
		Debug.Log("TurnView:OnPrepareHide");
		if (stone != null) {
			Destroy(stone.gameObject);
			stone = null;
		}
		BattleManager.instance.GoNext();
		base.OnPrepareHide();
	}

	protected override void OnShow()
	{
		GoNext(1f);
	}
}
