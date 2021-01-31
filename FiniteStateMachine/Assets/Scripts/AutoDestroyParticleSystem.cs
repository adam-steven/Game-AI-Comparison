using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroyParticleSystem : MonoBehaviour {

    private ParticleSystem particleSys;

	// Use this for initialization
	void Start ()
    {
        particleSys = GetComponent<ParticleSystem>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (particleSys)
        {
            if(!particleSys.IsAlive())
            {
                Destroy(this.gameObject);
            }
        }
	}
}
