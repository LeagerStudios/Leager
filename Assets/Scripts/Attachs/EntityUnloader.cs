using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityUnloader : MonoBehaviour {

    public GameObject target;
    public bool isThis;
    public bool canDespawn = true;

    private void OnValidate()
    {
        if (isThis) target = gameObject;
    }

}
