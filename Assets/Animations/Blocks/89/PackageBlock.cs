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
        if(target == null)
        {
            Destroy(gameObject);
        }
        else
        {
            if(time < 1f)
            {
                time += 1 / Vector2.Distance(target.transform.position, startPos);
                transform.position = Vector2.Lerp(startPos, target.transform.position, time);
            }
            else
            {
                transform.position = target.transform.position;
                target.Receive("");
                Destroy(this);
            }
        }
       
	}
}
