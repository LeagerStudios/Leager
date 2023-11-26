using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackageBlock : MonoBehaviour {

    public ResourceLauncher target;
    public Vector3 startPos;
    public float time = 0;

	void Start ()
    {
        startPos = transform.position;
        transform.eulerAngles = new Vector3(0, 0, ManagingFunctions.PointToPivotUp(transform.position, target.transform.position));
    }
	
	void Update ()
    {
    
	}
}
