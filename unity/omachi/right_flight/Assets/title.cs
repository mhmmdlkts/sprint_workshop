using UnityEngine;
using System.Collections;

public class title : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if(Input.GetKeyDown( "mouse 0" )) {
            Application.LoadLevel("stage");
        }
	}
}
