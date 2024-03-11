using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkEntity : MonoBehaviour
{
    void Start()
    {
        if(!GameManager.gameManagerReference.isNetworkClient && !GameManager.gameManagerReference.isNetworkHost)
        {
            Destroy(this);
        }
    }

    void Update()
    {
        
    }
}
