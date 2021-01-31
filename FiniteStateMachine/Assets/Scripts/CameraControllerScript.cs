using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControllerScript : MonoBehaviour {

    public GameObject player;
    private float furthestXPos = 0f;
	
	// Update is called once per frame
	void Update ()
    {
        Vector3 newCameraPosition = player.transform.position;

        if(newCameraPosition.y < -0.6f)
        {
            newCameraPosition.y = -0.6f;
        }

        if(newCameraPosition.x <= furthestXPos)
        {
            newCameraPosition.x = furthestXPos;
        }
        else
        {
            furthestXPos = newCameraPosition.x;
        }

        newCameraPosition.z = this.transform.position.z;

        this.transform.position = newCameraPosition;

		if(Input.GetKey(KeyCode.Escape))
		{
			Application.Quit();
		}
	}
}
