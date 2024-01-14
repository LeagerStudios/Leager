using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceLauncher : MonoBehaviour {

    public GameObject packPrefab;
    public GameObject firePrefab;
    public GameObject explosion;
    public bool spawn;
    public bool animationPlayed = false;
    public GameObject playingCore = null;
    [SerializeField] LayerMask blockMask;

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
        if (playingCore != null)
            if (playingCore.GetComponent<SpriteRenderer>().enabled)
                if (Physics2D.Raycast(playingCore.transform.position, Vector2.up, 0.6f, blockMask))
                {
                    Instantiate(explosion, playingCore.transform.position, Quaternion.identity).transform.localScale = new Vector3(3f, 3f, 1f);
                    StopAllCoroutines();
                    playingCore.GetComponent<SpriteRenderer>().enabled = false;

                    foreach (SpriteRenderer a in playingCore.transform.GetComponentsInChildren<SpriteRenderer>()) { a.enabled = false; }

                    MenuController.menuController.CancelTravel();

                    Invoke("ReturnCamFocus", 1.5f);
                }
    }

    public void ReturnCamFocus()
    {
        PlayerController player = GameManager.gameManagerReference.player;

        Camera.main.GetComponent<CameraController>().focus = player.gameObject;
        Destroy(playingCore);
        player.onControl = true;
        MenuController.menuController.UIActive = true;
    }

    public void TriggerAnimation(string state, Sprite core)
    {
        StartCoroutine(IETriggerAnimation(state, core));
    }

    IEnumerator IETriggerAnimation(string state, Sprite core, bool cameraFocus = true)
    {
        if (state == "packagelaunch")
        {
            int coreTile = System.Array.IndexOf(GameManager.gameManagerReference.tiles, core);
            GameObject child = Instantiate(packPrefab, transform);
            child.GetComponent<SpriteRenderer>().sprite = core;
            playingCore = child;

            if (coreTile == 96)
            {
                GameObject child3 = Instantiate(firePrefab, child.transform);
                GameObject child4 = Instantiate(firePrefab, child.transform);

                child.transform.position = transform.position + (Vector3.up * 0.75f);
                child3.transform.localPosition = new Vector2(0.5f, -0.5f);
                child4.transform.localPosition = new Vector2(-0.5f, -0.5f);
                CoreFire fire3 = child3.GetComponent<CoreFire>();
                fire3.maxSize = 0.5f;
                CoreFire fire4 = child4.GetComponent<CoreFire>();
                fire4.maxSize = 0.5f;
                float speed = 0f;

                if (cameraFocus)
                    Camera.main.GetComponent<CameraController>().focus = child;
                float counter = 0f;
                while (counter < 2.5f)
                {
                    child.transform.position += Vector3.up * speed * Time.deltaTime;

                    if (speed < 2)
                    {
                        speed += Time.deltaTime;
                    }

                    counter += speed * Time.deltaTime;
                    yield return new WaitForEndOfFrame();
                }
                fire3.endOnNext = true;
                fire4.endOnNext = true;

                while (child.transform.position.y < GameManager.gameManagerReference.WorldHeight + 15f)
                {
                    child.transform.position += Vector3.up * speed * Time.deltaTime;
                    child.transform.eulerAngles += Vector3.forward * speed * 30 * Time.deltaTime;
                    speed += Time.deltaTime * 7;
                    yield return new WaitForEndOfFrame();
                }
            }
            else if (coreTile == 97)
            {
                GameObject child2 = Instantiate(firePrefab, child.transform);
                GameObject child3 = Instantiate(firePrefab, child.transform);
                GameObject child4 = Instantiate(firePrefab, child.transform);

                child.transform.position = transform.position + (Vector3.up * 0.75f);
                child2.transform.localPosition = new Vector2(0f, -0.5f);
                child3.transform.localPosition = new Vector2(0.5f, -0.5f);
                child4.transform.localPosition = new Vector2(-0.5f, -0.5f);
                CoreFire fire = child2.GetComponent<CoreFire>();
                fire.maxSize = 1.5f;
                CoreFire fire3 = child3.GetComponent<CoreFire>();
                fire3.maxSize = 0.5f;
                CoreFire fire4 = child4.GetComponent<CoreFire>();
                fire4.maxSize = 0.5f;
                float speed = 0f;

                if (cameraFocus)
                    Camera.main.GetComponent<CameraController>().focus = child;
                float counter = 0f;
                while (counter < 2.5f)
                {
                    child.transform.position += Vector3.up * speed * Time.deltaTime;

                    if (speed < 2)
                    {
                        speed += Time.deltaTime;
                    }

                    counter += speed * Time.deltaTime;
                    yield return new WaitForEndOfFrame();
                }
                fire.endOnNext = true;
                fire3.endOnNext = true;
                fire4.endOnNext = true;

                while (child.transform.position.y < GameManager.gameManagerReference.WorldHeight + 15f)
                {
                    child.transform.position += Vector3.up * speed * Time.deltaTime;
                    child.transform.eulerAngles += Vector3.forward * speed * 30 * Time.deltaTime;
                    speed += Time.deltaTime * 10;
                    yield return new WaitForEndOfFrame();
                }
            }
            else if (coreTile == 100)
            {
                
                float speed = 10f;

                if (cameraFocus)
                    Camera.main.GetComponent<CameraController>().focus = child;
                while (speed > -1f)
                {
                    child.transform.position += Vector3.up * speed * Time.deltaTime;

                    speed += Time.deltaTime * Physics2D.gravity.y;

                    yield return new WaitForEndOfFrame();
                }

                while (child.transform.localScale.x < 2f)
                {
                    child.transform.localScale += Vector3.one * 10 * Time.deltaTime;
                    child.transform.position += Vector3.up * speed * Time.deltaTime;

                    speed += Time.deltaTime * Physics2D.gravity.y;

                    yield return new WaitForEndOfFrame();
                }

                child.transform.localScale = Vector3.one * 2;

                GameObject child2 = Instantiate(firePrefab, child.transform);
                GameObject child3 = Instantiate(firePrefab, child.transform);
                GameObject child4 = Instantiate(firePrefab, child.transform);
                GameObject child5 = Instantiate(firePrefab, child.transform);
                GameObject child6 = Instantiate(firePrefab, child.transform);

                child2.transform.localPosition = new Vector2(0f, -0.25f);
                child3.transform.localPosition = new Vector2(0.25f, -0.25f);
                child4.transform.localPosition = new Vector2(-0.25f, -0.25f);
                child5.transform.localPosition = new Vector2(0.5f, -0.25f);
                child6.transform.localPosition = new Vector2(-0.5f, -0.25f);
                CoreFire fire = child2.GetComponent<CoreFire>();
                fire.maxSize = 0.6f;
                CoreFire fire3 = child3.GetComponent<CoreFire>();
                fire3.maxSize = 0.6f;
                CoreFire fire4 = child4.GetComponent<CoreFire>();
                fire4.maxSize = 0.6f;
                CoreFire fire5 = child5.GetComponent<CoreFire>();
                fire5.maxSize = 0.25f;
                CoreFire fire6 = child6.GetComponent<CoreFire>();
                fire6.maxSize = 0.25f;


                while (child.transform.position.y < GameManager.gameManagerReference.WorldHeight + 15f)
                {
                    child.transform.position += Vector3.up * speed * Time.deltaTime;
                    speed += Time.deltaTime * 12;
                    yield return new WaitForEndOfFrame();
                }
            }
            if (cameraFocus)
                Camera.main.GetComponent<CameraController>().enabled = false;

            Destroy(child);
        }
        else if (state == "antenna")
        {
            float t = 0f;
            while (t < 1f)
            {
                transform.eulerAngles = Vector3.forward * Mathf.LerpAngle(transform.eulerAngles.z, -20f, 0.4f);
                t += Time.deltaTime;
                yield return new WaitForSeconds(0.016f);
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
