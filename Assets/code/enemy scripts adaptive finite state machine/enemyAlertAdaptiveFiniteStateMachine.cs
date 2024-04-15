using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class enemyAlertAdaptiveFiniteStateMachine : MonoBehaviour
{
    private enemyStateMachine stateMachine;
    private GameObject player;
    private Transform sprite;
    private Animator anim;
    private AIDestinationSetter destination;
    private AIPath path;
    private GameObject closestGun;
    private gameControllerAndCamera gameController;
    private gunScript gunController;
    private GameObject room;
    private SpriteRenderer roomShadow;
    private bool alertingBuilding = false;

    //private int disitionToChase = 0; //0= not decided, 1= stay, 2=chase
    private int inWait = 0; //for anything that has a dealy 0 = standby, 1 = waiting, 2 = waitOver

    private adaptiveFSMPointCounter pointCounter;
    private int ratioTotal = 0;

    private float standGaurdTimer;
    public float AmountOfTimeToStandGaurd;

    private void Start()
    {
        stateMachine = this.GetComponent<enemyStateMachine>();
        player = GameObject.FindGameObjectWithTag("Player");
        sprite = this.GetComponent<enemyFiniteStateMachine>().sprite;
        anim = sprite.GetComponent<Animator>();
        destination = this.GetComponent<AIDestinationSetter>();
        path = this.GetComponent<AIPath>();
        gameController = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<gameControllerAndCamera>();
        pointCounter = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<adaptiveFSMPointCounter>();

        for (int i = 0; i < pointCounter.alertPoints.Length; i++)
            ratioTotal += pointCounter.alertPoints[i];
    }


    private void Disition()
    {
        //disition to get the player back will get a point - apply at death

        int randomDesition = Random.Range(0, ratioTotal);

        for (int i = 0; i < pointCounter.alertPoints.Length; i++)
        {
            if ((randomDesition -= pointCounter.alertPoints[i]) < 0)
            {
                if ((enemyStateMachine.AlertSubState)i == enemyStateMachine.AlertSubState.Stand_Gaurd)
                    standGaurdTimer = AmountOfTimeToStandGaurd;

                stateMachine.ChangeAlertSubState((enemyStateMachine.AlertSubState)i);
                return;
            }
        }

        Debug.Log("out of disition range");
        stateMachine.ChangeAlertSubState(enemyStateMachine.AlertSubState.Stand_Gaurd);
        return;
    }

    internal void RepairSelf(ref int health, int startingHealth, ref GameObject gun, ref bool gunInHolster)
    {
        HasPlayerApeared(ref gun, ref gunInHolster);
        if (health < startingHealth)
        {
            if (gun && !gunInHolster)
            {
                gunInHolster = true;
                gun.GetComponent<SpriteRenderer>().enabled = false;
                gunController = gun.GetComponent<gunScript>();
                anim.SetInteger("gun size", 0);
            }

            anim.SetBool("moving", false);
            anim.SetInteger("idel actions", 3);

            if (inWait == 0)
            {
                StartCoroutine(Heal());
                inWait = 1;
            }
            else if (inWait == 2)
            {
                health += 1;
                inWait = 0;
            }
        }
        else
        {
            anim.SetInteger("idel actions", 0);
            if (gunInHolster)
            {
                gunInHolster = false;
                gun.GetComponent<SpriteRenderer>().enabled = true;
                anim.SetInteger("gun size", gunController.gunHoldNum);
            }
            inWait = 0;
            Disition();
        }
    }

    internal void AlertBuilding(ref GameObject gun, ref bool gunInHolster)
    {
        HasPlayerApeared(ref gun, ref gunInHolster);
        if (gameController.alertTimer > 0)
        {
            alertingBuilding = true;
            if (gun && !gunInHolster)
            {
                gunInHolster = true;
                gun.GetComponent<SpriteRenderer>().enabled = false;
                gunController = gun.GetComponent<gunScript>();
                anim.SetInteger("gun size", 0);
            }

            anim.SetBool("moving", false);
            anim.SetBool("alert building", true);
            gameController.alertTimer -= Time.deltaTime;
        }
        else
        {
            alertingBuilding = false;
            anim.SetBool("alert building", false);
            if (gunInHolster)
            {
                gunInHolster = false;
                gun.GetComponent<SpriteRenderer>().enabled = true;
                anim.SetInteger("gun size", gunController.gunHoldNum);
            }

            Disition();
        }
    }

    internal void StandGaurd(ref GameObject gun, ref bool gunInHolster)
    {
        destination.target = null;
        path.canMove = false;
        anim.SetBool("moving", false);

        if (gunController && gunController.enemy != this.gameObject)
        {
            gun = null;
            gunController = null;
            anim.SetInteger("gun size", 0);
            return;
        }

        HasPlayerApeared(ref gun, ref gunInHolster);

        standGaurdTimer -= Time.deltaTime;
        if(standGaurdTimer <= 0)
            Disition();
    }

    internal void PickUpGun(ref GameObject gun, ref bool gunInHolster)
    {
        HasPlayerApeared(ref gun, ref gunInHolster);
        FindTheClosestGun();
        //assume that the closest gun is able/worth it, to be picked up
        if (closestGun && !gun)
        {
            if (Vector3.Distance(closestGun.transform.position, transform.position) > 0.5f)
            {
                destination.target = closestGun.transform;
                path.canMove = true;
                anim.SetBool("moving", true);
            }
            else
            {
                destination.target = null;
                path.canMove = false;

                anim.SetBool("moving", false);

                if (closestGun)
                {
                    gun = closestGun;

                    if (!gun.GetComponent<gunScript>().enemy && !gun.GetComponent<gunScript>().beingHeldByPlayer)
                    {
                        gunController = gun.GetComponent<gunScript>();
                        gunController.enemy = this.gameObject;
                        anim.SetInteger("gun size", gunController.gunHoldNum);
                    }
                    else
                    {
                        closestGun = null;
                        gun = null;
                        destination.target = null;
                        path.canMove = false;
                        anim.SetBool("moving", false);

                        Disition();
                    }
                }
            }
        }
        else
        {
            destination.target = null;
            path.canMove = false;
            anim.SetBool("moving", false);

            Disition();
        }
    }

    //look into path finding
    internal void Chase(ref GameObject gun, ref bool gunInHolster, bool seenPlayer)
    {
        if (player && Vector3.Distance(transform.position, player.transform.position) < 10 && seenPlayer)
        {
            destination.target = player.transform;
            path.canMove = true;
            anim.SetBool("moving", true);

            HasPlayerApeared(ref gun, ref gunInHolster);

            if (!path.hasPath || !gun)
            {
                destination.target = null;
                path.canMove = false;
                anim.SetBool("moving", false);
                Disition();
            }
        }
        else
        {
            destination.target = null;
            path.canMove = false;
            anim.SetBool("moving", false);
            Disition();

        }
    }

    IEnumerator Heal()
    {
        yield return new WaitForSeconds(3);
        inWait = 2;
    }

    private void HasPlayerApeared(ref GameObject gun, ref bool gunInHolster)
    {
        if (player)
        {
            if (!roomShadow.enabled || !Physics2D.Linecast(this.transform.position, player.transform.position, 1 << LayerMask.NameToLayer("wall")))
            {
                pointCounter.alertPoints[(int)stateMachine.aiAlertSubState] += 1;

                destination.target = null;
                path.canMove = false;
                anim.SetBool("moving", false);
                anim.SetInteger("idel actions", 0);

                if (alertingBuilding && gameController.alertTimer > 0)
                {
                    gameController.alertTimer = 5f;
                    anim.SetBool("alert building", false);
                    alertingBuilding = false;
                }

                if (gunInHolster)
                {
                    gunInHolster = false;
                    gun.GetComponent<SpriteRenderer>().enabled = true;
                    anim.SetInteger("gun size", gunController.gunHoldNum);
                }

                inWait = 0;

                stateMachine.ChangeAlertSubState(enemyStateMachine.AlertSubState.Stand_Gaurd);
                stateMachine.ChangeState(enemyStateMachine.State.Fight);
            }
        }
    }

    private void FindTheClosestGun()
    {
        GameObject[] allGuns;
        allGuns = GameObject.FindGameObjectsWithTag("dropped gun");

        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in allGuns)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closestGun = go;
                distance = curDistance;
            }

            if (closestGun && closestGun.GetComponent<gunScript>().enemy)
                closestGun = null;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        //set what room the enemy is in
        if (collision.CompareTag("room shadow"))
        {
            room = collision.gameObject;
            roomShadow = room.GetComponent<SpriteRenderer>();
        }
    }
}
