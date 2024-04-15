using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class alertCircle : MonoBehaviour {

    private Transform cam;
	
	void Start () {
        cam = GameObject.FindGameObjectWithTag("MainCamera").transform;

    }
	
	void Update () {
        transform.parent = cam;
        transform.localPosition = new Vector3(-20, 10, 1);

        float scaler = transform.localScale.x + (Time.deltaTime * 20);
        transform.localScale = new Vector3(scaler, scaler,1);

        transform.Rotate(Vector3.forward * Time.deltaTime * 10);
    }
}
