using UnityEngine;
using System.Collections;

public class playerShot : MonoBehaviour {

    public const float VELOCITY = 20f;
    public GameObject explode;


	// Use this for initialization
	void Start () {
                Vector3 v = this.transform.TransformDirection(VELOCITY,0,0);
                this.rigidbody2D.velocity = v;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        GameObject obj = (GameObject)Instantiate( explode, this.transform.position,Quaternion.identity);
        Object.Destroy( obj, 2f);
        Object.Destroy(gameObject);
    }
}
