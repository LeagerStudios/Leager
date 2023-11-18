using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileControls : MonoBehaviour {

    public Transform[] childs;

    void Start()
    {
        if(GInput.leagerInput.platform != "Mobile")
        {
            gameObject.SetActive(false);
        }
    }

    void Update ()
    {
        if (!GameManager.gameManagerReference.InGame)
        {
            SetOverlay("Null");
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
        foreach(Transform child in childs)
        {
            child.gameObject.SetActive(child.gameObject.name == overlay);
        }
    }
}
