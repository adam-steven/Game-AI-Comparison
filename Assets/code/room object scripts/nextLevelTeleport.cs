using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class nextLevelTeleport : MonoBehaviour
{
    private playerController player;
    private adaptiveFSMPointCounter pointCounter;

    private string idelPoints = "";
    private string alertPoints = "";
    private string fightPoints = "";
    private GameObject gun = null;
    private string gunName = "";
    private int ammo = 0;

    private gameControllerAndCamera gameController;
    private string savePath;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<playerController>();
        pointCounter = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<adaptiveFSMPointCounter>();
        gameController = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<gameControllerAndCamera>();
        savePath = gameController.savePath;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if(player.gun)
            {
                gun = player.gun;
                gunName = gun.name;
                ammo = gun.GetComponent<gunScript>().amountOfAmmo;
            }
            for (int i = 0; i < pointCounter.idelPoints.Length; i++)
                idelPoints += pointCounter.idelPoints[i] + ",";

            for (int i = 0; i < pointCounter.alertPoints.Length; i++)
                alertPoints += pointCounter.alertPoints[i] + ",";

            for (int i = 0; i < pointCounter.fightPoints.Length; i++)
                fightPoints += pointCounter.fightPoints[i] + ",";

            var saveObj = new save()
            {
                savedIdelPoints = idelPoints,
                savedAlertPoints = alertPoints,
                savedFightPoints = fightPoints,
                savedPlayerHealth = player.health,
                savedPlayerGun = gunName,
                saveGunAmmo = ammo
            };

            var binaryFormatter = new BinaryFormatter();
            using (var fileStream = File.Create(savePath))
            {
                binaryFormatter.Serialize(fileStream, saveObj);
            }

            Debug.Log("save");

            //reload the level
            SceneManager.LoadScene(gameController.sceneNumber);
        }
    }
}
