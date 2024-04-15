using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyStateMachine : MonoBehaviour
{
	// Declare AI states. 
	public enum State {Idle = 0,  Prepare_For_Fight = 1, Fight = 2, Alert = 3};

	//Declare AI sub-states
	public enum IdleSubState { Talking = 0, Phone = 1, TV = 2, Cards = 3, Wandering = 4, Pick_Gun_Up = 5};
	public enum PrepSubState { Face_Player = 0, Alert_Room = 1, Pull_Out_Gun = 2, Grab_Gun = 3};
	public enum FightSubState { Stand_Shoot = 0, Strafe_Shoot = 1, Circle_Left = 2, Circle_Right = 3, Move_Closer = 4, Move_Away = 5, Drop_Gun = 6, Grab_Gun = 7 };
	public enum AlertSubState { Repair_Self = 0, Alert_Building = 1, Stand_Gaurd = 2, Switch_Gun = 3, Chase = 4};


	// State all the Enemies start in
	internal State aiState = State.Idle;
	//Sub-state starts for all enimies 
	internal IdleSubState aiIdleSubState = IdleSubState.Wandering;
	internal PrepSubState aiPrepSubState = PrepSubState.Face_Player;
	internal FightSubState aiFightSubState = FightSubState.Move_Closer;
	internal AlertSubState aiAlertSubState = AlertSubState.Stand_Gaurd;

	// Change the state
	internal void ChangeState(State newState){ aiState = newState; }
	internal bool IsState(State checkState){ return (aiState == checkState); }

	//change the sub states
	internal void ChangeIdleSubState(IdleSubState newSubState){ aiIdleSubState = newSubState; }
	internal void ChangePrepSubState(PrepSubState newSubState) { aiPrepSubState = newSubState; }
	internal void ChangeFightSubState(FightSubState newSubState) { aiFightSubState = newSubState; }
	internal void ChangeAlertSubState(AlertSubState newSubState) { aiAlertSubState = newSubState; }
}
