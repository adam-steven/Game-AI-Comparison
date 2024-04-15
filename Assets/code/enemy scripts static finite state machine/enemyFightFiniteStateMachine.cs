using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class enemyFightFiniteStateMachine : MonoBehaviour
{

    private enemyStateMachine stateMachine;
    private GameObject player;
    private Transform sprite;
    private Animator anim;
    private AIDestinationSetter destination;
    private AIPath path;
    private GameObject closestGun;
    private gunScript gunController;
    private bool playerFacingMe;
    private int randomDesition;
    private Vector3 strafePosition;
    private float actionTimer;
    public float resetActionTimer;
    public Transform[] wallDetection;
    private int inWait = 0; //for anything that has a dealy 0 = standby, 1 = waiting, 2 = waitOver
    public float shootTime = 0.8f;
    private GameObject room;
    private SpriteRenderer roomShadow;

    private void Start()
    {
        stateMachine = this.GetComponent<enemyStateMachine>();
        player = GameObject.FindGameObjectWithTag("Player");
        sprite = this.GetComponent<enemyFiniteStateMachine>().sprite;
        anim = sprite.GetComponent<Animator>();
        destination = this.GetComponent<AIDestinationSetter>();
        path = this.GetComponent<AIPath>();
        actionTimer = Random.Range(resetActionTimer/5f, resetActionTimer);
    }

    //faces player and shoot (if no gun punch)
    internal void StandAndShoot(ref GameObject gun)
    {
        if (player)
        {
            Vector3 dir = player.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
        }
        HasPlayerRan();

        if (!gun)
        {
            if (player && Vector3.Distance(transform.position, player.transform.position) < 0.5f)
            {
                anim.ResetTrigger("punch");
                anim.SetTrigger("punch");
                PunchPlayer();
            }
            else
            {
                anim.ResetTrigger("punch");
                stateMachine.ChangeFightSubState(enemyStateMachine.FightSubState.Move_Closer);
                return;
            }
        }
        else
        {
            if (gunController.enemy != this.gameObject)
            {
                gun = null;
                gunController = null;
                anim.SetInteger("gun size", 0);
                return;
            }

            if (!playerFacingMe)
            {
                if (gunController && gunController.amountOfAmmo > 0)
                    ShootGun();
                else
                {
                    stateMachine.ChangeFightSubState(enemyStateMachine.FightSubState.Drop_Gun);
                    return;
                }
            }
            else
            {
                randomDesition = Random.Range(0, 4);
                gunController.shotByEnemy = false;

                switch (randomDesition)
                {
                    case 0:
                        strafePosition = new Vector3(transform.position.x + Random.Range(-3.0f, 3.0f), transform.position.y + Random.Range(-3.0f, 3.0f), 0);
                        stateMachine.ChangeFightSubState(enemyStateMachine.FightSubState.Strafe_Shoot);
                        break;
                    case 1:
                        stateMachine.ChangeFightSubState(enemyStateMachine.FightSubState.Circle_Left);
                        break;
                    case 2:
                        stateMachine.ChangeFightSubState(enemyStateMachine.FightSubState.Circle_Right);
                        break;
                    case 3:
                        stateMachine.ChangeFightSubState(enemyStateMachine.FightSubState.Move_Away);
                        break;
                    default:
                        stateMachine.ChangeFightSubState(enemyStateMachine.FightSubState.Move_Away);
                        break;
                }
            }
        }
    }

    internal void Strafe(ref GameObject gun, float speed)
    {
        actionTimer -= Time.deltaTime;
        HasPlayerRan();
        if (playerFacingMe && actionTimer > 0 && !WallDetection())
        {
            if (transform.position != strafePosition && !Physics2D.Linecast(this.transform.position, strafePosition, 1 << LayerMask.NameToLayer("wall")))
            {
                if (player)
                {
                    Vector3 dir = player.transform.position - transform.position;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
                }
                anim.SetBool("moving", true);
                transform.position = Vector3.MoveTowards(transform.position, strafePosition, Time.deltaTime * speed);

                if (gunController && gunController.amountOfAmmo > 0)
                    ShootGun();
                else
                {
                    stateMachine.ChangeFightSubState(enemyStateMachine.FightSubState.Drop_Gun);
                    return;
                }
            }
            else
            {
                strafePosition = new Vector3(transform.position.x + Random.Range(-3.0f, 3.0f), transform.position.y + Random.Range(-3.0f, 3.0f), 0);
            }
        }
        else
        {
            actionTimer = Random.Range(resetActionTimer / 5f, resetActionTimer);
            anim.SetBool("moving", false);
            stateMachine.ChangeFightSubState(enemyStateMachine.FightSubState.Stand_Shoot);
        }
    }

    internal void CircleRight(ref GameObject gun, float speed)
    {
        actionTimer -= Time.deltaTime;
        HasPlayerRan();
        if (playerFacingMe && actionTimer > 0)
        {
            if (player)
            {
                Vector3 dir = player.transform.position - transform.position;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
            }
            transform.Translate(Vector3.right * Time.deltaTime * speed);
            if (WallDetection())
                transform.Translate(Vector3.right * Time.deltaTime * speed);
            anim.SetBool("moving", true);

            if (gunController && gunController.amountOfAmmo > 0)
                ShootGun();
            else
            {
                stateMachine.ChangeFightSubState(enemyStateMachine.FightSubState.Drop_Gun);
                return;
            }
        }
        else
        {
            actionTimer = Random.Range(resetActionTimer / 5f, resetActionTimer);
            anim.SetBool("moving", false);
            stateMachine.ChangeFightSubState(enemyStateMachine.FightSubState.Stand_Shoot);
        }
    }

    internal void CircleLeft(ref GameObject gun, float speed)
    {
        actionTimer -= Time.deltaTime;
        HasPlayerRan();
        if (playerFacingMe && actionTimer > 0)
        {
            if (player)
            {
                Vector3 dir = player.transform.position - transform.position;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
            }
            transform.Translate(Vector3.left * Time.deltaTime * speed);
            if (WallDetection())
                transform.Translate(Vector3.up * Time.deltaTime * speed);
            anim.SetBool("moving", true);

            if (gunController && gunController.amountOfAmmo > 0)
                ShootGun();
            else
            {
                stateMachine.ChangeFightSubState(enemyStateMachine.FightSubState.Drop_Gun);
                return;
            }
        }
        else
        {
            actionTimer = Random.Range(resetActionTimer / 5f, resetActionTimer);
            anim.SetBool("moving", false);
            stateMachine.ChangeFightSubState(enemyStateMachine.FightSubState.Stand_Shoot);
        }
    }

    internal void MoveAway(ref GameObject gun, float speed)
    {
        actionTimer -= Time.deltaTime;
        HasPlayerRan();
        if (playerFacingMe && actionTimer > 0 && !WallDetection())
        {
            if (player)
            {
                Vector3 dir = player.transform.position - transform.position;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
            }
            transform.Translate(Vector3.down * Time.deltaTime * speed/2);
            anim.SetBool("moving", true);

            if (gunController && gunController.amountOfAmmo > 0)
                ShootGun();
            else
            {
                stateMachine.ChangeFightSubState(enemyStateMachine.FightSubState.Drop_Gun);
                return;
            }
        }
        else
        {
            actionTimer = Random.Range(resetActionTimer / 5f, resetActionTimer);
            anim.SetBool("moving", false);
            stateMachine.ChangeFightSubState(enemyStateMachine.FightSubState.Stand_Shoot);
        }
    }

    internal void MoveCloser(ref GameObject gun, ref bool gunInHolster)
    {
        //if the alert script failed to rest these components
        anim.SetInteger("idel actions", 0);
        anim.SetBool("alert building", false);

        if (gun && gunInHolster)
        {
            gunInHolster = false;
            gun.GetComponent<SpriteRenderer>().enabled = true;
            anim.SetInteger("gun size", gunController.gunHoldNum);
        }

        HasPlayerRan();

        //move in closer before punching 
        FindTheClosestGun();
        if (player && Vector3.Distance(transform.position, player.transform.position) > 0.5f && !gun)
        {
            destination.target = player.transform;
            path.canMove = true;
            anim.SetBool("moving", true);

            if(closestGun && !closestGun.GetComponent<gunScript>().enemy)
            {
                if(Vector3.Distance(transform.position, closestGun.transform.position) < Vector3.Distance(transform.position, player.transform.position))
                {
                    destination.target = null;
                    path.canMove = false;
                    anim.SetBool("moving", false);
                    stateMachine.ChangeFightSubState(enemyStateMachine.FightSubState.Grab_Gun);
                    return;
                }
            }
        }
        else
        {
            destination.target = null;
            path.canMove = false;
            anim.SetBool("moving", false);

            //make sure the gun controller is set
            if(gun)
                gunController = gun.GetComponent<gunScript>();

            stateMachine.ChangeFightSubState(enemyStateMachine.FightSubState.Stand_Shoot);
        }
    }

    internal void PickUpGun(ref GameObject gun)
    {
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

                        stateMachine.ChangeFightSubState(enemyStateMachine.FightSubState.Move_Closer);
                    }
                }
            }
        }
        else
        {
            destination.target = null;
            path.canMove = false;
            anim.SetBool("moving", false);

            stateMachine.ChangeFightSubState(enemyStateMachine.FightSubState.Stand_Shoot);
        }
    }

    internal void DropGun(ref GameObject gun)
    {
        if (gun && gunController)
        {
            gunController.enemy = null;
            gun = null;
            gunController = null;
            anim.SetInteger("gun size", 0);
        }
        stateMachine.ChangeFightSubState(enemyStateMachine.FightSubState.Move_Closer);
    }

    private void HasPlayerRan()
    {
        if (roomShadow && roomShadow.enabled && player && Physics2D.Linecast(this.transform.position, player.transform.position, 1 << LayerMask.NameToLayer("wall")))
        {
            destination.target = null;
            path.canMove = false;
            if(gunController)
                gunController.shotByEnemy = false;
            anim.ResetTrigger("punch");
            anim.SetBool("moving", false);
            inWait = 0;
            actionTimer = Random.Range(resetActionTimer / 5f, resetActionTimer);
            stateMachine.ChangeFightSubState(enemyStateMachine.FightSubState.Move_Closer);
            stateMachine.ChangeState(enemyStateMachine.State.Alert);
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("player sight"))
        {
            playerFacingMe = true;
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

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("player sight"))
        {
            playerFacingMe = false;
        }
    }

    private bool WallDetection()
    {
        bool hitWall = false;
        for(int i = 0; i < wallDetection.Length; i++)
        {
            if (Physics2D.Linecast(this.transform.position, wallDetection[i].transform.position, 1 << LayerMask.NameToLayer("wall")))
                hitWall = true;
        }
        return hitWall;
    }

    IEnumerator Shoot()
    {
        yield return new WaitForSeconds(Random.Range(0.1f, shootTime));
        inWait = 2;
    }
    
    private void ShootGun()
    {
        if (inWait == 0)
        {
            StartCoroutine(Shoot());
            inWait = 1;
        }
        else if (inWait == 2)
        {
            gunController.shotByEnemy = true;
            inWait = 0;
        }
    }

    IEnumerator Punch()
    {
        yield return new WaitForSeconds(0.5f);
        inWait = 2;
    }

    private void PunchPlayer()
    {
        if (inWait == 0)
        {
            StartCoroutine(Punch());
            inWait = 1;
        }
        else if (inWait == 2)
        {
            if (player && Vector3.Distance(transform.position, player.transform.position) < 0.5f)
                player.GetComponent<playerController>().health -= 1;
            inWait = 0;
        }
    }
}
