using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class seatScript : MonoBehaviour
{
    public bool[] seatTaken;
    public Transform[] seat;

    private void Start()
    {
        if(seat.Length != seatTaken.Length)
        {
            Debug.LogError("array count must be the same");
        }
    }
}
