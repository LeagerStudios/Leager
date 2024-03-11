using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PARTICLE_OrangeLaser : MonoBehaviour {

    SpriteRenderer spriteRenderer;
	void Start ()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
	}
	
	void Update ()
    {
        Color color = spriteRenderer.color;
        color.a -= Time.deltaTime * 2;
        if(color.a < 0)
        {
            Destroy(gameObject);
        }
        spriteRenderer.color = color;
	}
}
