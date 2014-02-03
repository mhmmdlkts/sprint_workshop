using UnityEngine;
using System.Collections;

public class camera : MonoBehaviour {
    public GameObject back;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnPreRender() {
        float x = playerScript.instance.transform.position.x / 30f;
        float y = playerScript.instance.transform.position.y / 30f;
        back.renderer.material.mainTextureOffset = new Vector2 (x,y);
    }
}
