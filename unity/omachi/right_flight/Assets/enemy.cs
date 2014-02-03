using UnityEngine;
using System.Collections;

public class enemy : MonoBehaviour {
    const int MAX_HP = 10;
    const float VELOCITY = 1f;
    int hp = MAX_HP;
    public GameObject explode;

    // Use this for initialization
	void Start () {}

    public void setVelocity(bool isLeftSide) {
        Vector2 v = new Vector2( isLeftSide ? VELOCITY : -VELOCITY, 0f);
        this.rigidbody2D.velocity = v;
    }

    // Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        if( other.tag == "shot"){
            hp -= 1;
            if (hp < 1){
                for (int i=0 ; i<10 ; ++i) {
                    Vector3 pos = Random.insideUnitCircle * 2;
                    pos += this.transform.position;
                    GameObject obj = (GameObject)Instantiate( explode,pos,Quaternion.identity);
                    Object.Destroy(obj, 2f);
                }

                main.instance.addScore(1);
                Object.Destroy(gameObject);
            }
        }
    }

}
