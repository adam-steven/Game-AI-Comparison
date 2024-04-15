using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class adaptiveFSMPointCounter : MonoBehaviour
{
    
    public int[] idelPoints;
    public int[] fightPoints;
    public int[] alertPoints;

    private void Start()
    {
        idelPoints = new int[System.Enum.GetValues(typeof(enemyStateMachine.IdleSubState)).Length];
        fightPoints = new int[System.Enum.GetValues(typeof(enemyStateMachine.FightSubState)).Length];
        alertPoints = new int[System.Enum.GetValues(typeof(enemyStateMachine.AlertSubState)).Length];

        for (int i = 0; i < idelPoints.Length; i++)
            idelPoints[i] = 1;
        for (int i = 0; i < fightPoints.Length; i++)
            fightPoints[i] = 1;
        for (int i = 0; i < alertPoints.Length; i++)
            alertPoints[i] = 1;

    }
}
