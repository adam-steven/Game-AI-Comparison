using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class roomShadow : MonoBehaviour {

    [HideInInspector]
    public SpriteRenderer sp;

    public Color seeThrough;

    public GameObject topDoor;
    public GameObject bottomDoor;
    public GameObject leftDoor;
    public GameObject rightDoor;

    private gameControllerAndCamera gameCon;
    private bool spawned;

    public GameObject chest;
    [HideInInspector]
    public int enemyCounter;

    void Start () {
        sp = GetComponent<SpriteRenderer>();
        sp.enabled = true;
        gameCon = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<gameControllerAndCamera>();
        gameCon.romShadows.Add(gameObject);
    }

	void Update () {
        if (topDoor.GetComponent<layoutScript>().doorCanBeSeenThrough)
            Spawn();
        else if (bottomDoor.GetComponent<layoutScript>().doorCanBeSeenThrough)
            Spawn();
        else if (leftDoor.GetComponent<layoutScript>().doorCanBeSeenThrough)
            Spawn();
        else if (rightDoor.GetComponent<layoutScript>().doorCanBeSeenThrough)
            Spawn();
        else
        {
            sp.color = Color.black;
        }
    }

    void Spawn()
    {
        sp.color = seeThrough;
        if (!spawned)
        {
            if (gameObject == gameCon.romShadows[gameCon.romShadows.Count - 1])
                Instantiate(gameCon.bossTemplates[Random.Range(0, gameCon.bossTemplates.Length)], transform.position, Quaternion.identity);
            else
                Instantiate(gameCon.enemyTemplates[Random.Range(0, gameCon.enemyTemplates.Length)], transform.position, Quaternion.identity);

            spawned = true;
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            sp.enabled = false;
            spawned = true;
        }

        if (collision.CompareTag("enemy"))
        {
            enemyCounter += 1;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            sp.enabled = true;

        if (collision.CompareTag("enemy") && enemyCounter == 0)
        {
            if (Random.value < 0.2f)
            {
                Instantiate(chest, transform.position, transform.rotation);
            }
        }
    }
}
