using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gunScript : MonoBehaviour {

    private GameObject player;
    private playerController playerControl;
    private SpriteRenderer sp;
    public int gunHoldNum;
    public bool beingHeldByPlayer;

    [HideInInspector]
    public GameObject enemy;
    [HideInInspector]
    public bool shotByEnemy;

    public Sprite dropedGun;
    public Sprite heldGun;
    public Sprite heldGunByEnemy;

    private bool wasPickedUp = false;

    public GameObject bullet;
    public GameObject enemyBullet;
    private bulletScript shotBullet;
    public float recoil;
    public int amountOfBulletBursts = 1; //this is for burst shots thier is a delay between each bullet
    public float burstDelay;
    public int amountOfSimeltaniusBullets = 1; //this is for a shot gun like effect
    public float fireRateDelay;
    private float fireRateDelayCounter;

    [HideInInspector]
    public int startingAmountOfAmmo;
    public int amountOfAmmo;

    public int damage = 1;
    public int bulletspeed = 5;
    public float shootShake = 0.2f;

    public bool automatic;

    [HideInInspector]
    public bool startWithPlayer;

    void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
        playerControl = player.GetComponent<playerController>();
        sp = GetComponent<SpriteRenderer>();
        if(startingAmountOfAmmo == 0)
            startingAmountOfAmmo = amountOfAmmo;
        this.tag = "dropped gun";
        wasPickedUp = false;
    }

	void Update () {

        sp.color = Color.Lerp(Color.white, Color.red, (1f - ((float)amountOfAmmo / (float)startingAmountOfAmmo)));

        if (fireRateDelayCounter > 0)
        {
            fireRateDelayCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Drop") && player)
        {
            playerControl.gun = null;
            wasPickedUp = false;
        }


        if (player && playerControl.gun == this.gameObject || enemy)
        {
            if (sp.sprite != heldGun)
            { 
                if (playerControl.gun == this.gameObject)
                {
                    sp.sprite = heldGun;
                    sp.sortingOrder = 3;
                    transform.parent = player.GetComponent<playerController>().gunHoldPoint.transform;
                    transform.localPosition = Vector3.zero;
                    transform.localEulerAngles = Vector3.zero;
                    this.tag = "Untagged";
                    beingHeldByPlayer = true;
                }
                else if (enemy)
                {
                    sp.sprite = heldGunByEnemy;
                    sp.sortingOrder = 3;
                    transform.parent = enemy.GetComponent<enemyFiniteStateMachine>().gunHoldPoint.transform;
                    transform.localPosition = Vector3.zero;
                    transform.localEulerAngles = Vector3.zero;
                    this.tag = "Untagged";
                    wasPickedUp = false;
                }
            }

            if (fireRateDelayCounter <= 0)
            {
                if (((!automatic && Input.GetButtonDown("Fire1")) || (automatic && Input.GetButton("Fire1"))) && !enemy)
                {
                    StartCoroutine(TheBurstDelay());
                    fireRateDelayCounter = fireRateDelay + (amountOfBulletBursts * burstDelay);
                }

                if(shotByEnemy && enemy && sp.enabled)
                {
                    StartCoroutine(TheBurstDelay());
                    fireRateDelayCounter = fireRateDelay + (amountOfBulletBursts * burstDelay);
                }
            }
        }
        else
        {
            if ((player && Vector3.Distance(player.transform.position, transform.position) < 0.5f && (Input.GetButtonUp("Pick Up")) && !wasPickedUp) || startWithPlayer)
            {
                playerControl.gun = this.gameObject;
                startWithPlayer = false;
                wasPickedUp = true;
            }

            if (sp.sprite != dropedGun)
            {
                sp.sprite = dropedGun;
                sp.sortingOrder = 0;
                transform.parent = null;
                StartCoroutine(TheSwapDelay());
                shotByEnemy = false;
                beingHeldByPlayer = false;
                if (amountOfAmmo > 0) //"dropped gun" tell the AI that it can pick up the gun something that should not happen if its out of ammo
                    this.tag = "dropped gun";
                else
                    Destroy(this.gameObject);
            }
        }
	}

    private readonly WaitForSeconds sd = new WaitForSeconds(0.1f);
    IEnumerator TheSwapDelay()
    {
        yield return sd;
        wasPickedUp = false;
    }

   
    IEnumerator TheBurstDelay()
    {
        for (int i = 0; i < amountOfBulletBursts; i++)
        {
            if (amountOfAmmo > 0 && sp.sprite != dropedGun)
            {
                for (int j = 0; j < amountOfSimeltaniusBullets; j++)
                {
                    transform.localEulerAngles = new Vector3(0, 0, Random.Range(-recoil, recoil));
                    if (!enemy)
                        shotBullet = Instantiate(bullet, transform.position, transform.rotation).GetComponent<bulletScript>();
                    else
                    {
                        shotBullet = Instantiate(enemyBullet, transform.position, transform.rotation).GetComponent<bulletScript>();
                        shotByEnemy = false;
                        shotBullet.finiteStateMachine = enemy.GetComponent<enemyFiniteStateMachine>();
                    }
                    shotBullet.damage = damage;
                    shotBullet.speed = bulletspeed;
                    shotBullet.shake = shootShake;
                    transform.localEulerAngles = new Vector3(0, 0, 0);
                }
                amountOfAmmo -= 1;
                yield return new WaitForSeconds(burstDelay);
            }
        }
    }
}
