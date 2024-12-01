using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingSceneTitle : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(transform.parent.gameObject);
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
            Destroy(transform.parent.GetChild(0).gameObject);
            Invoke("Boom", 1.5f);
        }
        else if (scene.name != "LoadScreen")
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
            Destroy(transform.parent.gameObject);
        }
    }

    void Boom()
    {
        Destroy(transform.parent.gameObject);
    }
}
