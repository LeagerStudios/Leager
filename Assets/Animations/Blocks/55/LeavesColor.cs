using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeavesColor : MonoBehaviour {

	void Start ()
    {
        GetComponent<SpriteRenderer>().color = transform.parent.parent.GetComponent<ChunkController>().chunkColor;
	}
}
