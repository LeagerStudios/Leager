using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingSceneTitle : MonoBehaviour
{

    [SerializeField] Canvas canvas;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            transform.parent = canvas.transform;
            transform.SetAsLastSibling();
            SceneManager.sceneLoaded -= OnSceneLoad;
        }
        else if (scene.name != "LoadScreen")
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
            Destroy(gameObject);
        }
    }
}
