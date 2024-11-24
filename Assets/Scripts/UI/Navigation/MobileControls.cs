using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileControls : MonoBehaviour {

    public Transform[] childs;
    public GameObject subPlanetMenu;

    void Start()
    {
        if(GInput.leagerInput.platform != "Mobile")
        {
            gameObject.SetActive(false);
        }
    }

    void Update ()
    {
        if ((!GameManager.gameManagerReference.InGame)|| !GameManager.gameManagerReference.player.alive)
        {
            SetOverlay("Null");
        }
        else if (subPlanetMenu.activeInHierarchy)
        {
            SetOverlay("SubPlanetMenu");
        }
        else if (StackBar.stackBarController.InventoryDeployed)
        {
            SetOverlay("Inventory");
        }
        else
        {
            SetOverlay("Playing");
        }
    }

    void SetOverlay(string overlay)
    {
        foreach (Transform child in childs)
        {
            child.gameObject.SetActive(child.gameObject.name == overlay);
        }
    }

    public void Hold(string key)
    {
        GInput.PressKey((KeyCode)System.Enum.Parse(typeof(KeyCode), key));
    }

    public void Release(string key)
    {
        GInput.ReleaseKey((KeyCode)System.Enum.Parse(typeof(KeyCode), key));
    }
}
