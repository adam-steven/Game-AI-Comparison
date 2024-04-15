using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class gameControllerAndCamera : MonoBehaviour {

    private GameObject player;
    private adaptiveFSMPointCounter pointCounter;

    [HideInInspector]
    public int roomCounter;
    public int roomLimit;
    public List<GameObject> romShadows;

    public GameObject[] enemyTemplates;
    public GameObject[] bossTemplates;

    [HideInInspector]
    public float alertTimer = 5;
    private bool alertShake = false; //indication for when the alert timer is at 0;

    [HideInInspector]
    public float shake;
    public float breathAddition; //this zooms out the camera  
    public float breathAmount; //pingpong goes between 0 and value this is the value 
    public float breathSpeed; //edits the speed of the pingpong - higher value, slower speed

    [HideInInspector]
    internal string savePath;

    public int sceneNumber;

    public Canvas canvas;

    void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
        pointCounter = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<adaptiveFSMPointCounter>();
        savePath = Application.persistentDataPath + "/gamesave.save";

        //if (File.Exists(savePath))
        //{
        //    File.Delete(savePath);
        //}

        if (canvas)
            canvas.enabled = false;

        StartCoroutine(LoadSave());
    }
	
	void Update () {
        //rooms = GameObject.FindGameObjectsWithTag("room");

        //cammera movement
        if (player)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 target = new Vector3((((mousePosition.x - player.transform.position.x) * 0.2f) + player.transform.position.x), (((mousePosition.y - player.transform.position.y) * 0.2f) + player.transform.position.y), transform.position.z);

            transform.position = new Vector3((Random.insideUnitCircle.x * shake) + target.x, (Random.insideUnitCircle.y * shake) + target.y, transform.position.z);
        }
        else
        {
            if (canvas)
                canvas.enabled = true;
        }

        if(Input.GetButtonUp("Cancel") && canvas)
        {
            if (canvas.enabled)
                canvas.enabled = false;
            else
                canvas.enabled = true;
        }

        if(shake > 0)
            shake -= Time.deltaTime;
        else
            shake = 0;

        gameObject.GetComponent<Camera>().orthographicSize = Mathf.PingPong(Time.time / breathSpeed, breathAmount) + breathAddition;

        if (alertTimer > 0)
        {
            gameObject.GetComponent<Camera>().backgroundColor = Color.Lerp(Color.white, Color.red, (1f - (alertTimer / 5f)));
        }
        else if(!alertShake)
        {
            shake = 1f;
            alertShake = true;
        }
    }

    IEnumerator LoadSave()
    {
        yield return new WaitForSeconds(0.05f);

        if (File.Exists(savePath))
        {
            save saveObj;
            var binaryFormatter = new BinaryFormatter();
            using (var fileStream = File.Open(savePath, FileMode.Open))
            {
                saveObj = (save)binaryFormatter.Deserialize(fileStream);
            }

            string[] splitIdelString = saveObj.savedIdelPoints.Split(',');
            string[] splitAlertString = saveObj.savedAlertPoints.Split(',');
            string[] splitFightString = saveObj.savedFightPoints.Split(',');

            for (int i = 0; i < pointCounter.idelPoints.Length; i++)
                pointCounter.idelPoints[i] = int.Parse(splitIdelString[i]);
            for (int i = 0; i < pointCounter.alertPoints.Length; i++)
                pointCounter.alertPoints[i] = int.Parse(splitAlertString[i]);
            for (int i = 0; i < pointCounter.fightPoints.Length; i++)
                pointCounter.fightPoints[i] = int.Parse(splitFightString[i]);

            player.GetComponent<playerController>().health = saveObj.savedPlayerHealth;

            if (saveObj.savedPlayerGun != "")
            {
                GameObject playerGun = Instantiate(Resources.Load(saveObj.savedPlayerGun, typeof(GameObject))) as GameObject;

                playerGun.name = saveObj.savedPlayerGun;

                int startingAmmo = 0;
                gunScript gun;

                gun = playerGun.GetComponent<gunScript>();

                startingAmmo = gun.amountOfAmmo;
                gun.amountOfAmmo = saveObj.saveGunAmmo;
                gun.startingAmountOfAmmo = startingAmmo;

                gun.startWithPlayer = true;
            }
        }
    }
}
