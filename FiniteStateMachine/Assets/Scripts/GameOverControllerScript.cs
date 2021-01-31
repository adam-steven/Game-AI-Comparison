using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverControllerScript : MonoBehaviour {

    public float preFadewaitTime;
    public float fadeTime;
    public float postFadeWaitTime;
    private float preFadeStartTime;
    private SpriteRenderer spriteRenderer;
    private bool beginEndGame = false;

    // Use this for initialization
    void Start ()
    {
        preFadeStartTime = preFadewaitTime;
        spriteRenderer = this.GetComponent<SpriteRenderer>();
    }
	
	// Update is called once per frame
	void Update () {
        if (beginEndGame)
        {
            if (preFadewaitTime > 0f)
            {
                preFadewaitTime -= Time.deltaTime;
            }
            else if (fadeTime > 0f)
            {
                fadeTime -= Time.deltaTime;
                spriteRenderer.color = new Color(1f, 1f, 1f, Mathf.SmoothStep(0f, 1f, preFadeStartTime - fadeTime));
            }
            else if(postFadeWaitTime > 0f)
            {
                postFadeWaitTime -= Time.deltaTime;
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }
	}

    public void BeginEndGame()
    {
        beginEndGame = true;
		GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
		foreach(GameObject enemy in enemies)
		{
			enemy.GetComponent<EnemyFiniteStateMachine>().StopBots();
		}
    }
}
