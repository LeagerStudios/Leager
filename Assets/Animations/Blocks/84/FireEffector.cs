using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireEffector : MonoBehaviour {

    private void OnTriggerStay2D(Collider2D collider)
    {
        if(collider.gameObject.GetComponent<EntityCommonScript>() != null)
        {
            collider.gameObject.GetComponent<EntityCommonScript>().AddState(EntityState.OnFire, 5f);
        }
    }
}
