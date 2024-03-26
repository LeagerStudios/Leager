using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeavesColor : MonoBehaviour {

	void Start ()
    {
        Color.RGBToHSV(transform.parent.parent.GetComponent<ChunkController>().chunkColor, out float h, out float s, out float v);
        h += 0.065f;

        GetComponent<SpriteRenderer>().color = Color.HSVToRGB(h, s, v);
	}
}
