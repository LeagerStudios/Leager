using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceLauncher : MonoBehaviour {

    public GameObject packPrefab;
    public bool spawn;
    public bool animationPlayed = false;

    public List<string> Items
    {
        get
        {
            return GetComponent<TileProperties>().storedItems;
        }
        set
        {
            GetComponent<TileProperties>().storedItems = value;
        }
    }

	void Start ()
    {
        transform.eulerAngles = transform.parent.eulerAngles;
        transform.localPosition = new Vector2(0, 0.5f);

        TileProperties properties = gameObject.AddComponent<TileProperties>();
        properties.parentTile = 89;
        properties.associatedTile = transform.parent.gameObject;
        properties.canDropStoredItems = true;

        StartCoroutine(PlaceAnimation());
	}


    void Update()
    {
        //ManagingFunctions.DropItem(7, transform.parent.position + new Vector3(Mathf.Sin(transform.eulerAngles.z * Mathf.Deg2Rad) * 0.7f, Mathf.Cos(transform.eulerAngles.z * Mathf.Deg2Rad) * -0.7f), 1, 1f);
        if (spawn)
        {
            spawn = false;
            //GameObject a = Instantiate(packPrefab);
            //target = a.GetComponent<PackageBlock>();
            //target.target = this;
            //a.transform.position = transform.position + Vector3.one * 10;
        }
    }

    public void TriggerAnimation(string state)
    {
        StartCoroutine(IETriggerAnimation(state));
    }

    IEnumerator IETriggerAnimation(string state)
    {
        if (state == "packagelaunch")
        {
            GameObject child = Instantiate(packPrefab, transform);
            child.transform.position = transform.position + (Vector3.up * 0.5f);
            float speed = 10f;

            Camera.main.GetComponent<CameraController>().focus = child;

            while (child.transform.position.y < GameManager.gameManagerReference.WorldHeight + 10f)
            {
                child.transform.Translate(Vector2.up * speed * Time.deltaTime);
                speed += Time.deltaTime * 10;
                yield return new WaitForSeconds(0.01666f);
            }
        }
        else if(state == "antenna")
        {
            float t = 0f;
            while (t < 1f)
            {
                transform.eulerAngles = Vector3.forward * Mathf.LerpAngle(transform.eulerAngles.z, -20f, 0.4f);
                t += Time.deltaTime;
                yield return new WaitForSeconds(0.01666f);
            }
        }

        animationPlayed = true;
    }

    IEnumerator PlaceAnimation()
    {
        transform.localScale = new Vector3(0f, 0f, 1f);

        while(transform.localScale.x < 0.99f)
        {
            transform.localScale = Vector2.Lerp(transform.localScale, Vector2.one, 0.3f); 
            yield return new WaitForSeconds(0.05f);
        }

        transform.localScale = Vector3.one;
    }

}
