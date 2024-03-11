using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundOndaController : MonoBehaviour {

    public Vector2 coming;
    public SoundHearer target;
    public float dir;
	
	public void InvokeOnda(Vector2 origPos, SoundHearer objetive, float direction)
    {
        coming = origPos;
        target = objetive;
        dir = direction;
    }
	
	
	void Update ()
    {
        transform.position = Vector2.Lerp(transform.position, target.transform.position, 0.1f);
        
        if(Vector2.Distance(transform.position, target.transform.position) <= 0.9f)
        {
            target.Notify(coming);
            Destroy(gameObject);
        }
	}
}
