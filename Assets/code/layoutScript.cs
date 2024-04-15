using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class layoutScript : MonoBehaviour {

    private GameObject player;
    public GameObject room;
    public Transform spawnPoint;
    private bool Spawning;
    private gameControllerAndCamera gameCon;
    private float timer;
    private Vector3 touckedCenter;
    private SpriteRenderer sp;
    [HideInInspector]
    public bool doorCanBeSeenThrough;
    private AstarPath pathFinder;

    void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
        gameCon = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<gameControllerAndCamera>();
        pathFinder = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<AstarPath>();

        sp = GetComponent<SpriteRenderer>();
        if (Random.value > (float)gameCon.roomCounter / (float)gameCon.roomLimit)
        {
            Spawning = true;
            timer = Random.Range(0.1f, 1f);
        }
    }

	void Update () {

        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            if (touckedCenter != spawnPoint.position && !gameObject.CompareTag("wall"))
            {
                sp.color = Color.black;
                gameObject.tag = "wall";
            }

            if (Spawning && !gameObject.CompareTag("door"))
            {
                Instantiate(room, spawnPoint.transform.position, Quaternion.identity);
                gameCon.roomCounter += 1;
                Spawning = false;
            }

            if(gameObject.CompareTag("door") && Input.GetButtonUp("Pick Up") && player &&Vector3.Distance(player.transform.position, transform.position) < 1f)
            {
                if (sp.enabled) //door is locked
                {
                    GetComponent<BoxCollider2D>().enabled = false;
                    sp.enabled = false;
                    gameObject.layer = 0;//default layer
                }
                else
                {
                    GetComponent<BoxCollider2D>().enabled = true;
                    sp.enabled = true;
                    gameObject.layer = 8;//wall layer
                }

                pathFinder.Scan(); //rescan all viable paths
            }

            if (player && Vector3.Distance(player.transform.position, transform.position) < 5f && !sp.enabled)
                doorCanBeSeenThrough = true;
            else
                doorCanBeSeenThrough = false;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("room"))
        {
            touckedCenter = collision.transform.position;
            sp.color = Color.white;
            gameObject.tag = "door";
        }
    }
}
