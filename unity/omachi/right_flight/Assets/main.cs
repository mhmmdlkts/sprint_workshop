using UnityEngine;
using System.Collections;

public class main : MonoBehaviour {

    const float ENEMY_APPEAR_INTERVAL = 5f;
    public GameObject player;
    GameObject playerInstance;
    public GameObject enemy;
    float enemyAppearWaitTime = 0;

    int score = 0;
    public GameObject scoreText;

    public int wave = 0;
    public GameObject waveText;
    float waveCount = 0f;
    public GameObject playerRoot;

    static public main instance;
    // Use this for initialization
    void Start () {
        instance = this;

        playerInstance = (GameObject)Instantiate( player,new Vector3(0f,0f,0f),Quaternion.identity );
        playerInstance.transform.parent = playerRoot.transform;

    }

    // Update is called once per frame
    void Update () {
        enemyAppearWaitTime -= Time.deltaTime;
        if (enemyAppearWaitTime < 0f){
                enemyAppearWaitTime += enemyAppearInterval(wave);
                float ang = Random.Range(0f,Mathf.PI * 2f);
                float dis = 10f;
                Vector3 offset = new Vector3(dis * Mathf.Cos(ang),dis * Mathf.Sin(ang),0f);
                Vector3 pos = offset += playerInstance.transform.position;
                GameObject e = (GameObject)Instantiate(enemy,pos,Quaternion.identity);
                e.GetComponent< enemy >().setVelocity(offset.x < 0f);
        }

        waveCount += Time.deltaTime;
        int currentWave = (int)(waveCount / 10f);
        if (wave != currentWave) {
            wave = currentWave;
            waveText.guiText.text = "wave " + wave;
        }
    }

    public void addScore(int s) {
        score += s;
        scoreText.guiText.text = "score " + score;
    }

    public float enemyAppearInterval(int wave)
    {
        float t = 5f * Mathf.Pow(0.9f,(float)wave);
        if ( t > 1f) {
            return t;
        }
        else {
            return 1f;
        }
    }
} 
