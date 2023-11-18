using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainManager : MonoBehaviour {


	void Start () {
        GameObject.Find("Transition").GetComponent<Animator>().SetBool("CanStart", true);
    }
	

	void Update () {
		
	}
}
