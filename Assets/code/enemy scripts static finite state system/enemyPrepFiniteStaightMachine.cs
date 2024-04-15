using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class enemyPrepFiniteStaightMachine : MonoBehaviour
{
    private enemyStateMachine stateMachine;
    private GameObject player;
    private gameControllerAndCamera gameController;
    private GameObject closestGun;
    private Transform sprite;
    private Animator anim;
    private AIDestinationSetter destination;
    private AIPath path;

    private bool notFinishedPrep = false;

    private void Start()
    {
        stateMachine = this.GetComponent<enemyStateMachine>();
        player = GameObject.FindGameObjectWithTag("Player");
        gameController = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<gameControllerAndCamera>();
        sprite = this.GetComponent<enemyFiniteStateMachine>().sprite;
        anim = sprite.GetComponent<Animator>();
        destination = this.GetComponent<AIDestinationSetter>();
        path = this.GetComponent<AIPath>();
    }

    internal void FacePlayer(ref GameObject gun, ref bool gunInHolster, float speed)
    {
        if (player)
        {
            Vector3 dir = player.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
        }

        path.maxSpeed = speed;

        notFinishedPrep = false;

        if (gameController.shake <= 0)
        {
            stateMachine.ChangePrepSubState(enemyStateMachine.PrepSubState.Alert_Room);
            notFinishedPrep = true;
            return;
        }

        if (gun)
        {
            stateMachine.ChangePrepSubState(enemyStateMachine.PrepSubState.Pull_Out_Gun);
            notFinishedPrep = true;
            return;
        }

        FindTheClosestGun();
        if (closestGun && !gunInHolster && Vector3.Distance(transform.position, closestGun.transform.position) < Vector3.Distance(transform.position, player.transform.position))
        {
            stateMachine.ChangePrepSubState(enemyStateMachine.PrepSubState.Grab_Gun);
            notFinishedPrep = true;
            return;
        }

        if(!notFinishedPrep)
        {
            stateMachine.ChangeState(enemyStateMachine.State.Fight);
            anim.SetBool("moving", false);
        }
    }

    internal void AlertRoom(ref GameObject gun, ref bool gunInHolster)
    {
        gameController.shake = 0.4f;

        notFinishedPrep = false;

        if (gun)
        {
            stateMachine.ChangePrepSubState(enemyStateMachine.PrepSubState.Pull_Out_Gun);
            notFinishedPrep = true;
            return;
        }

        FindTheClosestGun();
        if (closestGun && !gunInHolster && player && Vector3.Distance(transform.position, closestGun.transform.position) < Vector3.Distance(transform.position, player.transform.position))
        {
            stateMachine.ChangePrepSubState(enemyStateMachine.PrepSubState.Grab_Gun);
            notFinishedPrep = true;
            return;
        }

        if (!notFinishedPrep)
        {
            stateMachine.ChangeState(enemyStateMachine.State.Fight);
            anim.SetBool("moving", false);
        }
    }

    internal void PullOutGun(ref GameObject gun, ref bool gunInHolster)
    {
        gunInHolster = false;
        gun.GetComponent<SpriteRenderer>().enabled = true;
        anim.SetInteger("gun size", gun.GetComponent<gunScript>().gunHoldNum);

        stateMachine.ChangeState(enemyStateMachine.State.Fight);
        anim.SetBool("moving", false);
    }

    public void PickUpGun(ref GameObject gun)
    {
        FindTheClosestGun();
        //assume that the closest gun is able/worth it, to be picked up
        if (closestGun && notFinishedPrep)
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
                        gun.GetComponent<gunScript>().enemy = this.gameObject;
                        anim.SetInteger("gun size", gun.GetComponent<gunScript>().gunHoldNum);
                    }
                    else
                    {
                        closestGun = null;
                        gun = null;
                        destination.target = null;
                        path.canMove = false;
                        stateMachine.ChangeState(enemyStateMachine.State.Fight);
                        anim.SetBool("moving", false);
                    }
                }

                notFinishedPrep = false;
            }
        }
        else
        {
            destination.target = null;
            path.canMove = false;
            stateMachine.ChangeState(enemyStateMachine.State.Fight);
            anim.SetBool("moving", false);
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
}
