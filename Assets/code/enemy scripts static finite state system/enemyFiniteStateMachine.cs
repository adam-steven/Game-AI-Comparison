using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

//using pathfinding add on
public class enemyFiniteStateMachine : MonoBehaviour
{
    private GameObject player;
    private enemyStateMachine stateMachine;
    private enemyIdleFiniteStaightMachine idleFSS;
    private enemyPrepFiniteStaightMachine prepFSS;
    private enemyFightFiniteStateMachine fightFSS;
    private enemyAlertFiniteStateMachine alertFSS;

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

    [SerializeReference] private float speed = 3;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        stateMachine = this.GetComponent<enemyStateMachine>();
        idleFSS = this.GetComponent<enemyIdleFiniteStaightMachine>();
        prepFSS = this.GetComponent<enemyPrepFiniteStaightMachine>();
        fightFSS = this.GetComponent<enemyFightFiniteStateMachine>();
        alertFSS = this.GetComponent<enemyAlertFiniteStateMachine>();

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

    void PrepController() 
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

    void FightController() 
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

    void AlertController()
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

        //if (alertFSS.HasPlayerApeared(room))
        //{
        //    alertFSS.StopAlertingBuilding(anim, gameController);
        //    alertFSS.StopRepair(anim);
        //    anim.SetBool("moving", false);
        //    stateMachine.ChangeState(enemyStateMachine.State.Fight);
        //}
    }

    IEnumerator DeathDelay() // delay allows time for the gun to drop
    {
        yield return new WaitForSeconds(0.2f);
        Destroy(this.gameObject);
    }
}
