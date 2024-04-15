using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemySight : MonoBehaviour
{
    public bool playerSeen;
    public enemyFiniteStateMachine finiteStateMachine;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            playerSeen = true;
            finiteStateMachine.seenPlayer = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerSeen = false;
        }
    }
}
