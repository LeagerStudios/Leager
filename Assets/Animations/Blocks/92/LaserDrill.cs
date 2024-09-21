using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserDrill : MonoBehaviour, INodeEndPoint
{
    public Transform drill;
    public Transform laser;
    public Transform laserEnd;
    public NodeInstance nodeInstance;
    public TileProperties tileProperties;
    public Vector2 target = Vector2.zero;

    public AudioSource audioSource;

    public float lifetime = 0f;

    void Start()
    {
        nodeInstance = GetComponent<NodeInstance>();
    }

    void Update()
    {
        laserEnd.localScale = new Vector3(1f / laser.localScale.x, 1f, 1f);

        if(tileProperties == null)
            tileProperties = GetComponentInParent<TileProperties>();
        lifetime -= Time.deltaTime;
        lifetime = Mathf.Clamp(lifetime, 0f, 0.1f);

    }

    public void Update(EndPointNode endPoint)
    {
        lifetime = 0.1f;
    }
}
