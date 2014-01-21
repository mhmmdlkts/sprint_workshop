using UnityEngine;
using System.Collections;

public class ResultView : View<ResultView>
{
	void Start()
	{
		ResultView.InitInstance();
		AddPart(transform.Find("Win"), InMotion.Right);
		AddPart(transform.Find("Draw"), InMotion.Right);
		AddPart(transform.Find("Lose"), InMotion.Right);
		isTapHiding = true;
	}

	protected override void OnPrepare()
	{
		base.OnPrepare();

		StoneController[] _all = StoneController.FindAlive();
		if (_all.Length <= 0) {
			SetPartActive("Draw", true);
			SetPartActive("Win", false);
			SetPartActive("Lose", false);
		} else {
			bool _win = _all[0].stoneType == BattleManager.instance.ownerStone.stoneType;
			SetPartActive("Draw", false);
			SetPartActive("Win", _win);
			SetPartActive("Lose", !_win);
		}
	}

	protected override void OnPrepareHide()
	{
		Application.Quit();
		base.OnPrepareHide();
	}
}
