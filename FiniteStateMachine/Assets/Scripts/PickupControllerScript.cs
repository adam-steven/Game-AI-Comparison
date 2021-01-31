using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupControllerScript : MonoBehaviour {

    public float fadeTimer;
    public float deathTimer;
    public GameObject prefabParticleSystem;

    private bool particlesTriggered = false;
    private bool triggered = false;
    private SpriteRenderer spriteRenderer;
    private float fadeTimerStart;

    void Start()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        fadeTimerStart = fadeTimer;
    }
	
	// Update is called once per frame
	void Update () {
		if(triggered)
        {
            transform.Rotate(Vector3.forward, 90f * Time.deltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, Vector2.zero, 0.75f * Time.deltaTime);
            spriteRenderer.color = new Color(1f, 1f, 1f, Mathf.SmoothStep(1f, 0f, fadeTimerStart - fadeTimer));

            fadeTimer -= Time.deltaTime;
            deathTimer -= Time.deltaTime;

            if(deathTimer <= 0f)
            {
                Destroy(this.gameObject);
            }
            else if((deathTimer - 0.5f <= 0f) && !particlesTriggered)
            {
                Instantiate(prefabParticleSystem, this.transform.position, this.transform.localRotation);
                particlesTriggered = true;
            }
        }
	}

    public void onPickupTriggered()
    {
        triggered = true;
    }
}
