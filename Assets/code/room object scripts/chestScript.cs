using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class chestScript : MonoBehaviour {

    private GameObject player;
    public GameObject[] gun;

    void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
    }
	
	void Update () {
		if(player && Vector3.Distance(player.transform.position, transform.position) < 0.5f && (Input.GetButtonUp("Pick Up")))
        {
            if (!GetComponent<Animator>().GetBool("open"))
                GetComponent<Animator>().SetBool("open", true);
            else
                GetComponent<Animator>().SetBool("open", false);
        }

        if(GetComponent<Animator>().GetBool("open"))
        {
            StartCoroutine(Open());
        }
	}

    IEnumerator Open()
    {
        yield return new WaitForSeconds(0.2f);
        int randomNum = Random.Range(0, gun.Length);
        GameObject newGun = Instantiate(gun[randomNum], transform.position, transform.rotation);
        newGun.name = gun[randomNum].name;

        if(player)
            player.GetComponent<playerController>().health = player.GetComponent<playerController>().startingHealth;

        Destroy(gameObject);
    }
}
