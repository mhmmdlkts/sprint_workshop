using UnityEngine;
using System.Collections;

public class autoRemove : MonoBehaviour {
    GameObject mainCamera;
	// Use this for initialization
	void Start () {
        mainCamera = GameObject.Find("Main Camera");
	}
	
	// Update is called once per frame
	void Update () {
                Vector3 myPos = this.mainCamera.camera.WorldToScreenPoint(this.transform.position);
                Vector3 camPos =this.mainCamera.camera.WorldToScreenPoint(mainCamera.transform.position);

        Vector2 myPos2 = new Vector2(myPos.x,myPos.y);
        Vector2 camPos2 = new Vector2(camPos.x,camPos.y);

        if (Vector2.Distance( myPos2,camPos2) > Screen.height + Screen.width ) {
             Object.Destroy(gameObject);
        }
	}
}
