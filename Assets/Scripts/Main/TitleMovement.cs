using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleMovement : MonoBehaviour
{
    [SerializeField] Outline outline;
    
    void Start()
    {
        
    }

    void Update()
    {
        transform.eulerAngles = Vector3.forward * Mathf.Sin(Mathf.Deg2Rad * Time.time * 65) * 2.25f;
        transform.localScale = Vector3.one * (Mathf.Sin(Mathf.Deg2Rad * Time.time * 50) * 0.1f + 1f);

        if (outline)
        {
            outline.effectDistance = new Vector2(Mathf.Sin(Mathf.Deg2Rad * Time.time * 20) * 2, Mathf.Cos(Mathf.Deg2Rad * Time.time * 13) * 2);
        }
    }
}
