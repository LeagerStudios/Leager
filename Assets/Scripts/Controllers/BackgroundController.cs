using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    Transform camTransform;

    void Start()
    {
        camTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.GetChild(0).position = new Vector2(camTransform.position.x, camTransform.position.y + 3f);
        transform.GetChild(1).position = new Vector2(camTransform.position.x * 0.8f, camTransform.position.y);
        transform.GetChild(2).position = new Vector2(camTransform.position.x * 0.6f, camTransform.position.y);
    }
}
