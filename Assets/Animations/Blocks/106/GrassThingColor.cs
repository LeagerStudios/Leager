using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassThingColor : MonoBehaviour {

    public Sprite[] grasses;

	void Start ()
    {
        GetComponent<SpriteRenderer>().color = transform.parent.parent.GetComponent<ChunkController>().chunkColor;
        GetComponent<SpriteRenderer>().sprite = grasses[Random.Range(0, grasses.Length)];

    }
}
