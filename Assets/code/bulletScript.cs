using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletScript : MonoBehaviour {

    [HideInInspector]
    public int damage = 0;
    [HideInInspector]
    public int speed = 0;
    public bool enemy;

    private gameControllerAndCamera gameCon;
    [HideInInspector]
    public float shake = 0.1f;

    public enemyFiniteStateMachine finiteStateMachine;

    private void Start()
    {
        gameCon = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<gameControllerAndCamera>();
        gameCon.shake = shake;
    }

    void Update () {
        transform.Translate(Vector2.up * Time.deltaTime * speed);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("wall"))
            Destroy(gameObject);

        if (collision.CompareTag("door"))
            Destroy(gameObject);

        if (collision.CompareTag("enemy") && !enemy)
        {
            //damage enemy 
            collision.gameObject.GetComponent<enemyFiniteStateMachine>().health -= damage;
            Destroy(gameObject);
        }

        if (collision.CompareTag("Player") && enemy)
        {
            if (finiteStateMachine)
                finiteStateMachine.shotPlayer = true;
            collision.gameObject.GetComponent<playerController>().health -= damage;
            Destroy(gameObject);
        }

        if (collision.CompareTag("closed door"))
            Destroy(gameObject);
    }
}
