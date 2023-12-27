using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreFire : MonoBehaviour
{
    public float minSize = 1;
    public float maxSize = 2;
    public float t;
    public float rotPerSec = 1;
    public bool endOnNext = false;

    void Update()
    {
        t += rotPerSec * 360 * Time.deltaTime;
        if (t > 360)
        {
            t -= 360;

            if(endOnNext == true)
            {
                Destroy(gameObject);
            }
        }


        float x = Mathf.Lerp(minSize, maxSize, Mathf.Sin(t * Mathf.Deg2Rad));
        
        transform.localScale = new Vector3(x, x, 1);
    }
}
