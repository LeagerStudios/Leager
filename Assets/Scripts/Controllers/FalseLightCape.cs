using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FalseLightCape : MonoBehaviour {

	void Update () {
        GetComponent<SpriteRenderer>().sprite = LightController.lightController.GetComponent<SpriteRenderer>().sprite;
		
        if (LightController.lightController.transform.position.x > 0)
        {
            transform.position = LightController.lightController.transform.position - new Vector3(GameManager.gameManagerReference.WorldWidth * 16, 0);
        }
        else
        {
            transform.position = LightController.lightController.transform.position + new Vector3(GameManager.gameManagerReference.WorldWidth * 16, 0);
        }
	}
}
