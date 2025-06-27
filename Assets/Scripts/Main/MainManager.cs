using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainManager : MonoBehaviour {


	void Start () 
    {
        Invoke("OpenTransition", 0.5f);
    }
	

	void OpenTransition () 
    {
        GameObject.Find("Transition").GetComponent<Animator>().SetBool("CanStart", true);
    }
}
