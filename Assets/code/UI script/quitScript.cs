using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class quitScript : MonoBehaviour
{
    public void Quit()
    {
        Debug.Log("bye");
        Application.Quit();

    }
}
