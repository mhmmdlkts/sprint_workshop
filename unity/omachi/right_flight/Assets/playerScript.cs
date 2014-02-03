using UnityEngine;
using System.Collections;

public class playerScript : MonoBehaviour {
    const float VELOCITY = 7f;
    const float SHOOT_INTERVAL = 0.2f;
    float waitTimeToShoot = SHOOT_INTERVAL;
    public GameObject shoot;
    static public playerScript instance;
    GameObject root;

    // Use this for initialization
	void Start () {
        instance = this;
        root = main.instance.playerRoot;
	}
 
	// Update is called once per frame
	void Update () {

        if(Input.GetKey( "mouse 0" )) {
            float v = Time.deltaTime * 360f;
         //   if (Input.mousePosition.x > Screen.width / 2 ) {
                this.transform.Rotate(0f,0f,-v);
         //   }
         //   if (Input.mousePosition.x < Screen.width / 2 ) {
         //       this.transform.Rotate(0f,0f,v);
         //   }
        }

        Vector3 vel = this.transform.TransformDirection(new Vector3(VELOCITY * Time.deltaTime,0f,0f));
        root.transform.Translate(vel);
        //this.transform.Translate(vel);

                waitTimeToShoot -= Time.deltaTime;
                if (waitTimeToShoot < 0f){
                        waitTimeToShoot += SHOOT_INTERVAL;

                        Instantiate(shoot,this.transform.position,this.transform.rotation);
                }
    }

    void OnTriggerEnter2D(Collider2D col){
        //gameover
        Application.LoadLevel("title");

    }
}
