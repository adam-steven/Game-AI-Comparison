using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class enemyIdleAdaptiveFiniteStaightMachine : MonoBehaviour
{
    private float actionTimer;
    public float movingToTargetTime;
    public float watchingTVTime;
    public float playingCardsTime;
    public float onPhoneTime;
    public float wanderingTime;
    public float talkingTime;
    private bool gotToTarget = false;

    private enemyStateMachine stateMachine;
    private GameObject player;
    private Transform sprite;
    private GameObject room;
    private SpriteRenderer roomShadow;
    private gameControllerAndCamera gameController;
    private GameObject interactionObject;
    private Animator anim;
    private AIDestinationSetter destination;
    private AIPath path;

    private seatScript seat;
    private int seatBeingSatOn = -1;

    private float pratoltimer;

    private enemySight sight;

    private int inWait = 0; //for anything that has a dealy 0 = standby, 1 = waiting, 2 = waitOver

    private adaptiveFSMPointCounter pointCounter;
    private int ratioTotal = 0;

    private void Start()
    {
        stateMachine = this.GetComponent<enemyStateMachine>();
        player = GameObject.FindGameObjectWithTag("Player");
        gameController = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<gameControllerAndCamera>();
        sprite = this.GetComponent<enemyFiniteStateMachine>().sprite;
        anim = sprite.GetComponent<Animator>();
        destination = this.GetComponent<AIDestinationSetter>();
        path = this.GetComponent<AIPath>();
        actionTimer = Random.Range(0f, wanderingTime);
        sight = this.GetComponent<enemyFiniteStateMachine>().sight;
        pointCounter = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<adaptiveFSMPointCounter>();

        for (int i = 0; i < pointCounter.idelPoints.Length; i++)
            ratioTotal += pointCounter.idelPoints[i];
    }

    private void Disition()
    {
        //give a point to the actions that spot the player (sight not sound)  - apply at death 

        int randomDesition = Random.Range(0, ratioTotal);

        for (int i = 0; i < pointCounter.idelPoints.Length; i++)
        {
            if((randomDesition -= pointCounter.idelPoints[i]) < 0)
            {
                if((enemyStateMachine.IdleSubState)i != enemyStateMachine.IdleSubState.Wandering)
                    actionTimer = movingToTargetTime;
                else
                    actionTimer = wanderingTime;

                stateMachine.ChangeIdleSubState((enemyStateMachine.IdleSubState)i);
                return;
            }
        }

        Debug.Log("out of disition range");
        actionTimer = wanderingTime;
        stateMachine.ChangeIdleSubState(enemyStateMachine.IdleSubState.Wandering);
        return;
    }

    internal void Talking(int startingHealth, int health)
    {
        if (!interactionObject)
        {
            interactionObject = FindTheClosestInteraction("enemy");
            if (interactionObject)
                interactionObject.GetComponent<enemyIdleAdaptiveFiniteStaightMachine>().interactionObject = this.gameObject;
        }

        if (actionTimer > 0 && interactionObject && interactionObject.CompareTag("enemy"))
        {
            if (Vector3.Distance(interactionObject.transform.position, transform.position) > 0.8f)
            {
                destination.target = interactionObject.transform;
                path.canMove = true;
                anim.SetBool("moving", true);
            }
            else
            {
                if (gotToTarget == false)
                {
                    actionTimer = talkingTime;
                    gotToTarget = true;
                }

                destination.target = null;
                path.canMove = false;

                Vector3 dir = interactionObject.transform.position - transform.position;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);

                anim.SetBool("moving", false);

                if (inWait == 0)
                {
                    StartCoroutine(AnimationStartDelay(2));
                    inWait = 1;
                }
            }

            actionTimer -= Time.deltaTime;
            ListenForPlayer(startingHealth, health);
        }
        else
        {
            destination.target = null;
            path.canMove = false;
            inWait = 0;
            interactionObject = null;
            anim.SetBool("moving", false);

            gotToTarget = false;
            actionTimer = wanderingTime;
            transform.rotation = Quaternion.AngleAxis(Random.Range(0f, 360f) - 90, Vector3.forward);
            Disition();
        }
    }

    internal void Phone(int startingHealth, int health)
    {
        if (!interactionObject && actionTimer > 0)
        {
            actionTimer -= Time.deltaTime;
            anim.SetInteger("idel actions", 0);
            ListenForPlayer(startingHealth, health);
        }
        else
        {
            if (interactionObject && interactionObject.CompareTag("enemy"))
            {
                actionTimer = movingToTargetTime;
                stateMachine.ChangeIdleSubState(enemyStateMachine.IdleSubState.Talking);
                return;
            }

            Disition();
        }
    }

    internal void TV(int startingHealth, int health)
    {
        if (!interactionObject)
        {
            interactionObject = FindTheClosestInteraction("couch");
            if (interactionObject)
            {
                seat = interactionObject.GetComponent<seatScript>();
                bool seatAvalible = false;
                for (int i = 0; i < seat.seatTaken.Length; i++)
                {
                    if (!seat.seatTaken[i] && seatBeingSatOn == -1)
                    {
                        seatBeingSatOn = i;
                        seat.seatTaken[i] = true;
                        seatAvalible = true;
                        interactionObject = seat.seat[i].gameObject;
                    }
                }
                if (!seatAvalible)
                    interactionObject = null;
            }
        }

        if (actionTimer > 0 && interactionObject && interactionObject.CompareTag("seat"))
        {
            if (Vector3.Distance(interactionObject.transform.position, transform.position) > 0.5f)
            {
                destination.target = interactionObject.transform;
                path.canMove = true;
                anim.SetBool("moving", true);
            }
            else
            {
                if (gotToTarget == false)
                {
                    actionTimer = watchingTVTime;
                    gotToTarget = true;
                }

                destination.target = null;
                path.canMove = false;

                transform.parent = interactionObject.transform;
                transform.localPosition = Vector3.zero;
                transform.localEulerAngles = Vector3.zero;

                anim.SetBool("moving", false);
                anim.SetInteger("idel actions", 0);
            }

            actionTimer -= Time.deltaTime;
            ListenForPlayer(startingHealth, health);
        }
        else
        {
            transform.parent = null;

            //leave the seat
            for (int i = 0; i < 10; i++)
            {
                transform.Translate(Vector3.up * Time.deltaTime);
                anim.SetBool("moving", true);
            }

            destination.target = null;
            path.canMove = false;
            if (seatBeingSatOn != -1)
            {
                seat.seatTaken[seatBeingSatOn] = false;
                seatBeingSatOn = -1;
            }
            interactionObject = null;
            anim.SetBool("moving", false);

            gotToTarget = false;
            Disition();
        }
    }

    internal void Cards(int startingHealth, int health)
    {
        if (!interactionObject)
        {
            interactionObject = FindTheClosestInteraction("table");
            if (interactionObject)
            {
                seat = interactionObject.GetComponent<seatScript>();
                bool seatAvalible = false;
                for (int i = 0; i < seat.seatTaken.Length; i++)
                {
                    if (!seat.seatTaken[i] && seatBeingSatOn == -1)
                    {
                        seatBeingSatOn = i;
                        seat.seatTaken[i] = true;
                        seatAvalible = true;
                        interactionObject = seat.seat[i].gameObject;
                    }
                }
                if (!seatAvalible)
                    interactionObject = null;
            }
        }


        if (actionTimer > 0 && interactionObject && interactionObject.CompareTag("seat"))
        {

            if (Vector3.Distance(interactionObject.transform.position, transform.position) > 0.5f)
            {
                destination.target = interactionObject.transform;
                path.canMove = true;
                anim.SetBool("moving", true);
            }
            else
            {
                if (gotToTarget == false)
                {
                    actionTimer = watchingTVTime;
                    gotToTarget = true;
                }

                destination.target = null;
                path.canMove = false;

                transform.parent = interactionObject.transform;
                transform.localPosition = Vector3.zero;
                transform.localEulerAngles = Vector3.zero;

                anim.SetBool("moving", false);
                if (inWait == 0)
                {
                    StartCoroutine(AnimationStartDelay(1));
                    inWait = 1;
                }
            }

            actionTimer -= Time.deltaTime;
            ListenForPlayer(startingHealth, health);
        }
        else
        {
            transform.parent = null;
            destination.target = null;
            path.canMove = false;
            inWait = 0;
            if (seatBeingSatOn != -1)
            {
                seat.seatTaken[seatBeingSatOn] = false;
                seatBeingSatOn = -1;
            }
            interactionObject = null;
            anim.SetBool("moving", false);

            gotToTarget = false;
            actionTimer = wanderingTime;
            transform.rotation = Quaternion.AngleAxis(Random.Range(0f, 360f) - 90, Vector3.forward);
            Disition();
        }
    }

    internal void Wandering(Transform gunHoldPoint, int startingHealth, int health)
    {
        if (actionTimer > 0 && !interactionObject)
        {
            pratoltimer -= Time.deltaTime;
            actionTimer -= Time.deltaTime;

            if (pratoltimer <= 0)
            {
                transform.rotation = Quaternion.AngleAxis(Random.Range(0f, 360f) - 90, Vector3.forward);
                pratoltimer = Random.Range(0.5f, 5f);
            }
            else
            {
                //insures that the enemy doesn't keep running into the wall
                if (Physics2D.Linecast(transform.position, gunHoldPoint.position, 1 << LayerMask.NameToLayer("wall")))
                    pratoltimer = 0;

                transform.Translate(Vector2.up * Time.deltaTime);
                anim.SetBool("moving", true);
            }

            ListenForPlayer(startingHealth, health);
        }
        else
        {
            anim.SetBool("moving", false);

            if (interactionObject && interactionObject.CompareTag("enemy"))
            {
                actionTimer = talkingTime;
                stateMachine.ChangeIdleSubState(enemyStateMachine.IdleSubState.Talking);
            }
            else
            {
                Disition();
            }
        }
    }

    internal void PickUpGun(ref GameObject gun, ref bool gunInHolster, int startingHealth, int health)
    {
        if (!interactionObject)
            interactionObject = FindTheClosestInteraction("dropped gun");

        //assume that the closest gun is able/worth it, to be picked up
        if (interactionObject && !gun && actionTimer > 0)
        {
            if (Vector3.Distance(interactionObject.transform.position, transform.position) > 0.5f)
            {
                destination.target = interactionObject.transform;
                path.canMove = true;
                anim.SetBool("moving", true);
            }
            else
            {
                destination.target = null;
                path.canMove = false;

                anim.SetBool("moving", false);
                if (interactionObject.CompareTag("dropped gun"))
                {
                    gun = interactionObject;
                    gun.GetComponent<gunScript>().enemy = this.gameObject;
                    gunInHolster = true;
                    gun.GetComponent<SpriteRenderer>().enabled = false;
                }
            }

            ListenForPlayer(startingHealth, health);
            actionTimer -= Time.deltaTime;
        }
        else
        {
            destination.target = null;
            path.canMove = false;
            interactionObject = null;
            anim.SetBool("moving", false);

            Disition();
        }
    }

    IEnumerator AnimationStartDelay(int animNumber)
    {
        yield return new WaitForSeconds(Random.Range(0f, 5f));
        anim.SetInteger("idel actions", animNumber);
        inWait = 2;
    }

    private void ListenForPlayer(int startingHealth, int health)
    {
        //check if player is in the same room as them
        bool playerObscured = (player && Physics2D.Linecast(this.transform.position, player.transform.position, 1 << LayerMask.NameToLayer("wall")));
        //check if the player is in visual range
        bool visualRange = sight.playerSeen;

        //check is they can hear a sound (gun shots) or the alarm has went off
        bool soundOccurred = (room && !roomShadow.enabled && gameController.shake > 0);

        bool alarmWentOff = gameController.alertTimer <= 0;

        if (((visualRange || soundOccurred) && !playerObscured) || startingHealth != health || alarmWentOff)
        {
            if(visualRange)
                pointCounter.idelPoints[(int)stateMachine.aiIdleSubState] += 1;

            transform.parent = null;
            anim.SetBool("aware of player", true);
            destination.target = null;
            path.canMove = false;
            anim.SetBool("moving", false);
            stateMachine.ChangeState(enemyStateMachine.State.Prepare_For_Fight);
        }
    }

    private GameObject FindTheClosestInteraction(string tagToSearch)
    {
        GameObject closestInteraction = null;
        GameObject[] allInteraction;
        allInteraction = GameObject.FindGameObjectsWithTag(tagToSearch);

        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in allInteraction)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance && go != this.gameObject && (Vector3.Distance(transform.position, go.transform.position) < 5f))
            {
                closestInteraction = go;
                distance = curDistance;
            }
        }

        return closestInteraction;
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
