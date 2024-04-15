using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class timeDestroyScript : MonoBehaviour {

    public float waitTime;

    void Start()
    {
        StartCoroutine(TheDelay());
    }

    IEnumerator TheDelay()
    {
        yield return new WaitForSeconds(waitTime);
        Destroy(gameObject);
    }
}
