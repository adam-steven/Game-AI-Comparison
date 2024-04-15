using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class enemyAlertFiniteStateMachine : MonoBehaviour
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

    private int disitionToChase = 0; //0= not decided, 1= stay, 2=chase
    private int inWait = 0; //for anything that has a dealy 0 = standby, 1 = waiting, 2 = waitOver

    private void Start()
    {
        stateMachine = this.GetComponent<enemyStateMachine>();
        player = GameObject.FindGameObjectWithTag("Player");
        sprite = this.GetComponent<enemyFiniteStateMachine>().sprite;
        anim = sprite.GetComponent<Animator>();
        destination = this.GetComponent<AIDestinationSetter>();
        path = this.GetComponent<AIPath>();
        gameController = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<gameControllerAndCamera>();
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
            stateMachine.ChangeAlertSubState(enemyStateMachine.AlertSubState.Stand_Gaurd);
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
            if(gunInHolster)
            {
                gunInHolster = false;
                gun.GetComponent<SpriteRenderer>().enabled = true;
                anim.SetInteger("gun size", gunController.gunHoldNum);
            }

            stateMachine.ChangeAlertSubState(enemyStateMachine.AlertSubState.Stand_Gaurd);
        }
    }

    internal void StandGaurd(ref GameObject gun, int health, int startingHealth, bool seenPlayer, ref bool gunInHolster)
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

        if (health < startingHealth)
        {
            stateMachine.ChangeAlertSubState(enemyStateMachine.AlertSubState.Repair_Self);
            return;
        }

        if (gameController.alertTimer >= 4.9f)
        {
            stateMachine.ChangeAlertSubState(enemyStateMachine.AlertSubState.Alert_Building);
            return;
        }

        FindTheClosestGun();
        if (closestGun && !gun && !Physics2D.Linecast(this.transform.position, closestGun.transform.position, 1 << LayerMask.NameToLayer("wall")))
        {
            stateMachine.ChangeAlertSubState(enemyStateMachine.AlertSubState.Switch_Gun);
            return;
        }

        switch(disitionToChase)
        {
            case 0:
                if (player && Vector3.Distance(transform.position, player.transform.position) < 10 && seenPlayer && gun)
                    disitionToChase = Random.Range(1, 3);
                else
                    disitionToChase = 1;
                break;
            case 1:
                //do nothing
                break;
            case 2:
                stateMachine.ChangeAlertSubState(enemyStateMachine.AlertSubState.Chase);
                break;
            default:
                stateMachine.ChangeAlertSubState(enemyStateMachine.AlertSubState.Stand_Gaurd);
                break;
        }
        HasPlayerApeared(ref gun, ref gunInHolster);
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

                        stateMachine.ChangeAlertSubState(enemyStateMachine.AlertSubState.Stand_Gaurd);
                    }
                }
            }
        }
        else
        {
            destination.target = null;
            path.canMove = false;
            anim.SetBool("moving", false);

            stateMachine.ChangeAlertSubState(enemyStateMachine.AlertSubState.Stand_Gaurd);
        }
    }

    //look into path finding
    internal void Chase(ref GameObject gun, ref bool gunInHolster)
    {
        if(player)
            destination.target = player.transform;
        path.canMove = true;
        anim.SetBool("moving", true);

        HasPlayerApeared(ref gun, ref gunInHolster);

        if (!path.hasPath || !gun)
        {
            destination.target = null;
            path.canMove = false;
            anim.SetBool("moving", false);
            disitionToChase = 1;
            stateMachine.ChangeAlertSubState(enemyStateMachine.AlertSubState.Stand_Gaurd);
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
                disitionToChase = 0;

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
