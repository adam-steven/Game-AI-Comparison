using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class nextLevelTeleport : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //reload the level
            SceneManager.LoadScene(0);
        }
    }
}
