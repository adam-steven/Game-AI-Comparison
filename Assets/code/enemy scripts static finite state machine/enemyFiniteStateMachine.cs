using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


//static FSM uses' if statments and absolute randomness where needed
//adaptive FSM uses' weighted randomness for all desisions

//using pathfinding add on
public class enemyFiniteStateMachine : MonoBehaviour
{
    private GameObject player;
    private enemyStateMachine stateMachine;

    private enemyIdleFiniteStaightMachine idleFSS;
    private enemyPrepFiniteStaightMachine prepFSS;
    private enemyFightFiniteStateMachine fightFSS;
    private enemyAlertFiniteStateMachine alertFSS;

    private enemyIdleAdaptiveFiniteStaightMachine idleAFSS;
    private enemyPrepAdaptiveFiniteStaightMachine prepAFSS;
    private enemyFightAdaptiveFiniteStateMachine fightAFSS;
    private enemyAlertAdaptiveFiniteStateMachine alertAFSS;

    private adaptiveFSMPointCounter pointCounter;

    private bool isAdaptive;

    private int startingHealth;
    public int health = 10;

    public Transform sprite;
    private SpriteRenderer spriteRender;

    public GameObject gun;
    public Transform gunHoldPoint;
    private bool gunInHolster;

    public enemySight sight;

    [HideInInspector]
    public bool seenPlayer;

    [SerializeReference] private readonly float speed = 3;

    [HideInInspector]
    public bool shotPlayer;


    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        stateMachine = this.GetComponent<enemyStateMachine>();
        if (this.GetComponent<enemyIdleFiniteStaightMachine>())
        {
            idleFSS = this.GetComponent<enemyIdleFiniteStaightMachine>();
            prepFSS = this.GetComponent<enemyPrepFiniteStaightMachine>();
            fightFSS = this.GetComponent<enemyFightFiniteStateMachine>();
            alertFSS = this.GetComponent<enemyAlertFiniteStateMachine>();
            isAdaptive = false;
        }
        else
        {
            pointCounter = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<adaptiveFSMPointCounter>();
            idleAFSS = this.GetComponent<enemyIdleAdaptiveFiniteStaightMachine>();
            prepAFSS = this.GetComponent<enemyPrepAdaptiveFiniteStaightMachine>();
            fightAFSS = this.GetComponent<enemyFightAdaptiveFiniteStateMachine>();
            alertAFSS = this.GetComponent<enemyAlertAdaptiveFiniteStateMachine>();
            isAdaptive = true;
        }

        spriteRender = sprite.GetComponent<SpriteRenderer>();

        startingHealth = health;

        if(gun)
        {
            gun.GetComponent<gunScript>().enemy = this.gameObject;
            gunInHolster = true;
            gun.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch (stateMachine.aiState)
        {
            case enemyStateMachine.State.Idle:
                IdleController();
                break;
            case enemyStateMachine.State.Prepare_For_Fight:
                PrepController();
                break;
            case enemyStateMachine.State.Fight:
                FightController();
                break;
            case enemyStateMachine.State.Alert:
                AlertController();
                break;
            default:
                break;
        }

        spriteRender.color = Color.Lerp(Color.white, Color.red, (1f - ((float)health / (float)startingHealth)));

        if (health <= 0)
        {
            if (gun)
            {
                gun.GetComponent<gunScript>().enemy = null;
                gun = null;
            }

            StartCoroutine(DeathDelay());
        }

        if (!player)
            Destroy(this.gameObject);
    }

    void IdleController()
    {
        if (!isAdaptive)
        {
            switch (stateMachine.aiIdleSubState)
            {
                case enemyStateMachine.IdleSubState.Talking:
                    idleFSS.Talking(startingHealth, health);
                    break;
                case enemyStateMachine.IdleSubState.Phone:
                    idleFSS.Phone(startingHealth, health);
                    break;
                case enemyStateMachine.IdleSubState.TV:
                    idleFSS.TV(startingHealth, health);
                    break;
                case enemyStateMachine.IdleSubState.Cards:
                    idleFSS.Cards(startingHealth, health);
                    break;
                case enemyStateMachine.IdleSubState.Wandering:
                    idleFSS.Wandering(gunHoldPoint, startingHealth, health);
                    break;
                case enemyStateMachine.IdleSubState.Pick_Gun_Up:
                    idleFSS.PickUpGun(ref gun, ref gunInHolster, startingHealth, health);
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (stateMachine.aiIdleSubState)
            {
                case enemyStateMachine.IdleSubState.Talking:
                    idleAFSS.Talking(startingHealth, health);
                    break;
                case enemyStateMachine.IdleSubState.Phone:
                    idleAFSS.Phone(startingHealth, health);
                    break;
                case enemyStateMachine.IdleSubState.TV:
                    idleAFSS.TV(startingHealth, health);
                    break;
                case enemyStateMachine.IdleSubState.Cards:
                    idleAFSS.Cards(startingHealth, health);
                    break;
                case enemyStateMachine.IdleSubState.Wandering:
                    idleAFSS.Wandering(gunHoldPoint, startingHealth, health);
                    break;
                case enemyStateMachine.IdleSubState.Pick_Gun_Up:
                    idleAFSS.PickUpGun(ref gun, ref gunInHolster, startingHealth, health);
                    break;
                default:
                    break;
            }
        }
    }

    void PrepController() 
    {
        if (!isAdaptive)
        {
            switch (stateMachine.aiPrepSubState)
            {
                case enemyStateMachine.PrepSubState.Face_Player:
                    prepFSS.FacePlayer(ref gun, ref gunInHolster, speed);
                    break;
                case enemyStateMachine.PrepSubState.Alert_Room:
                    prepFSS.AlertRoom(ref gun, ref gunInHolster);
                    break;
                case enemyStateMachine.PrepSubState.Pull_Out_Gun:
                    prepFSS.PullOutGun(ref gun, ref gunInHolster);
                    break;
                case enemyStateMachine.PrepSubState.Grab_Gun:
                    prepFSS.PickUpGun(ref gun);
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (stateMachine.aiPrepSubState)
            {
                case enemyStateMachine.PrepSubState.Face_Player:
                    prepAFSS.FacePlayer(ref gun, ref gunInHolster, speed);
                    break;
                case enemyStateMachine.PrepSubState.Alert_Room:
                    prepAFSS.AlertRoom(ref gun, ref gunInHolster);
                    break;
                case enemyStateMachine.PrepSubState.Pull_Out_Gun:
                    prepAFSS.PullOutGun(ref gun, ref gunInHolster);
                    break;
                case enemyStateMachine.PrepSubState.Grab_Gun:
                    prepAFSS.PickUpGun(ref gun);
                    break;
                default:
                    break;
            }
        }
    }

    void FightController() 
    {
        if (!isAdaptive)
        {
            switch (stateMachine.aiFightSubState)
            {
                case enemyStateMachine.FightSubState.Stand_Shoot:
                    fightFSS.StandAndShoot(ref gun);
                    break;
                case enemyStateMachine.FightSubState.Strafe_Shoot:
                    fightFSS.Strafe(ref gun, speed);
                    break;
                case enemyStateMachine.FightSubState.Circle_Left:
                    fightFSS.CircleLeft(ref gun, speed);
                    break;
                case enemyStateMachine.FightSubState.Circle_Right:
                    fightFSS.CircleRight(ref gun, speed);
                    break;
                case enemyStateMachine.FightSubState.Move_Closer:
                    fightFSS.MoveCloser(ref gun, ref gunInHolster);
                    break;
                case enemyStateMachine.FightSubState.Move_Away:
                    fightFSS.MoveAway(ref gun, speed);
                    break;
                case enemyStateMachine.FightSubState.Drop_Gun:
                    fightFSS.DropGun(ref gun);
                    break;
                case enemyStateMachine.FightSubState.Grab_Gun:
                    fightFSS.PickUpGun(ref gun);
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (stateMachine.aiFightSubState)
            {
                case enemyStateMachine.FightSubState.Stand_Shoot:
                    fightAFSS.StandAndShoot(ref gun, ref shotPlayer);
                    break;
                case enemyStateMachine.FightSubState.Strafe_Shoot:
                    fightAFSS.Strafe(ref gun, speed, ref shotPlayer);
                    break;
                case enemyStateMachine.FightSubState.Circle_Left:
                    fightAFSS.CircleLeft(ref gun, speed, ref shotPlayer);
                    break;
                case enemyStateMachine.FightSubState.Circle_Right:
                    fightAFSS.CircleRight(ref gun, speed, ref shotPlayer);
                    break;
                case enemyStateMachine.FightSubState.Move_Closer:
                    fightAFSS.MoveCloser(ref gun, ref gunInHolster);
                    break;
                case enemyStateMachine.FightSubState.Move_Away:
                    fightAFSS.MoveAway(ref gun, speed, ref shotPlayer);
                    break;
                case enemyStateMachine.FightSubState.Drop_Gun:
                    fightAFSS.DropGun(ref gun);
                    break;
                case enemyStateMachine.FightSubState.Grab_Gun:
                    fightAFSS.PickUpGun(ref gun);
                    break;
                default:
                    break;
            }
        }
    }

    void AlertController()
    {
        if (!isAdaptive)
        {
            switch (stateMachine.aiAlertSubState)
            {
                case enemyStateMachine.AlertSubState.Repair_Self:
                    alertFSS.RepairSelf(ref health, startingHealth, ref gun, ref gunInHolster);
                    break;
                case enemyStateMachine.AlertSubState.Alert_Building:
                    alertFSS.AlertBuilding(ref gun, ref gunInHolster);
                    break;
                case enemyStateMachine.AlertSubState.Stand_Gaurd:
                    alertFSS.StandGaurd(ref gun, health, startingHealth, seenPlayer, ref gunInHolster);
                    break;
                case enemyStateMachine.AlertSubState.Switch_Gun:
                    alertFSS.PickUpGun(ref gun, ref gunInHolster);
                    break;
                case enemyStateMachine.AlertSubState.Chase:
                    alertFSS.Chase(ref gun, ref gunInHolster);
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (stateMachine.aiAlertSubState)
            {
                case enemyStateMachine.AlertSubState.Repair_Self:
                    alertAFSS.RepairSelf(ref health, startingHealth, ref gun, ref gunInHolster);
                    break;
                case enemyStateMachine.AlertSubState.Alert_Building:
                    alertAFSS.AlertBuilding(ref gun, ref gunInHolster);
                    break;
                case enemyStateMachine.AlertSubState.Stand_Gaurd:
                    alertAFSS.StandGaurd(ref gun, ref gunInHolster);
                    break;
                case enemyStateMachine.AlertSubState.Switch_Gun:
                    alertAFSS.PickUpGun(ref gun, ref gunInHolster);
                    break;
                case enemyStateMachine.AlertSubState.Chase:
                    alertAFSS.Chase(ref gun, ref gunInHolster, seenPlayer);
                    break;
                default:
                    break;
            }
        }
    }

    IEnumerator DeathDelay() // delay allows time for the gun to drop
    {
        yield return new WaitForSeconds(0.2f);

        if(isAdaptive && stateMachine.aiState == enemyStateMachine.State.Fight)
        {
            for (int i = 0; i < pointCounter.fightPoints.Length; i++)
            {
                if(i != (int)stateMachine.aiFightSubState)
                {
                    pointCounter.fightPoints[i] += 1;
                }
            }

            if(stateMachine.aiFightSubState == enemyStateMachine.FightSubState.Move_Closer && pointCounter.fightPoints[(int)enemyStateMachine.FightSubState.Drop_Gun] > 0)
                pointCounter.fightPoints[(int)enemyStateMachine.FightSubState.Drop_Gun] -= 1;
        }

        Destroy(this.gameObject);
    }
}
