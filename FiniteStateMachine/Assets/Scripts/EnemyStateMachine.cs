using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine : MonoBehaviour {

	// Enum to describe AI state. Can be expanded
	protected enum State {Patrol = 0, Chase = 1, Dead = 2, Wait = 3};

	// State all the Enemies start in
	protected State aiState = State.Patrol;

	protected void ChangeState(State newState)
	{
		// End the current state
		EndCurrentState(aiState);

		// Change the state
		aiState = newState;

		// Call the start of the new state
		StartNewState(aiState);
	}

	protected bool isState(State checkState)
	{
		return (aiState == checkState);
	}

	protected virtual void EndCurrentState(State currentState)
	{
		// Do nothing - needs to be overwritten
	}

	protected virtual void StartNewState(State currentState)
	{
		// Do nothing - needs to be overwritten
	}
}
