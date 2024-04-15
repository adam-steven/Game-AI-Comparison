using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class titleButtonScript : MonoBehaviour
{

    public void NewScene(int level)
    {
        string savePath = Application.persistentDataPath + "/gamesave.save";
        if (File.Exists(savePath))
            File.Delete(savePath);

        SceneManager.LoadScene(level);
    }
}
