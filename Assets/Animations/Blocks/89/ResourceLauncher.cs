using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceLauncher : MonoBehaviour, INodeEndPoint
{
    public GameObject packPrefab;
    public GameObject firePrefab;
    public GameObject explosion;
    public bool spawn;
    public bool animationPlayed = false;
    public GameObject playingCore = null;
    [SerializeField] LayerMask blockMask;
    [SerializeField] AudioClip[] explosions;

    public NodeInstance nodeInstance;
    public float lifetime;

    public bool powered = false;

    public List<string> Items
    {
        get
        {
            return transform.parent.GetComponent<TileProperties>().storedItems;
        }
        set
        {
            transform.parent.GetComponent<TileProperties>().storedItems = value;
        }
    }

	void Start ()
    {
        nodeInstance = GetComponent<NodeInstance>();
        transform.eulerAngles = transform.parent.eulerAngles;
        transform.localPosition = new Vector2(0, 0.5f);
        transform.GetChild(0).localScale = new Vector3(0f, 0f, 1f);

        if (transform.parent.GetComponent<TileProperties>() == null)
        {
            TileProperties properties = transform.parent.gameObject.AddComponent<TileProperties>();
            properties.parentTile = 89;
            properties.canDropStoredItems = true;
        }
        else
        {

        }
	}


    void Update()
    {
        if (playingCore != null)
        {
            if (playingCore.GetComponent<SpriteRenderer>().enabled)
                if (SendRaycast(playingCore, 0.6f, Vector2.up, Vector2.zero, true))
                {
                    Instantiate(explosion, playingCore.transform.position, Quaternion.identity).transform.localScale = new Vector3(3f, 3f, 1f);
                    GameManager.gameManagerReference.soundController.PlaySfxSound(explosions[Random.Range(0, explosions.Length)], 1f);

                    StopAllCoroutines();
                    playingCore.GetComponent<SpriteRenderer>().enabled = false;
                    GameManager.gameManagerReference.TileExplosionAt((int)playingCore.transform.position.x, (int)playingCore.transform.position.y, 5, 10);

                    foreach (SpriteRenderer a in playingCore.transform.GetComponentsInChildren<SpriteRenderer>()) { a.enabled = false; }

                    MenuController.menuController.CancelTravel();

                    Invoke("ReturnCamFocus", 1.5f);
                }
        }
        else
        {
            if(GameManager.gameManagerReference.InGame)
            if(lifetime > 0f)
            {
                    transform.GetChild(0).localScale = Vector2.Lerp(transform.GetChild(0).localScale, Vector2.one, 0.3f);
                    lifetime -= Time.deltaTime;
                powered = true;
            }
            else
            {
                transform.GetChild(0).localScale = Vector2.Lerp(transform.GetChild(0).localScale, Vector2.one, 0.3f);
                powered = false;
            }
        }
    }

    public bool SendRaycast(GameObject obj, float raycastDist, Vector2 raycastDir, Vector2 localOffset, bool ignoreSlabs = false)
    {
        bool colliding = false;
        Vector2 startpos = (Vector2)obj.transform.position + localOffset;

        if (ignoreSlabs)
        {
            RaycastHit2D rayHit = Physics2D.Raycast(startpos, raycastDir, raycastDist, blockMask);
            if (rayHit)
                colliding = rayHit.transform.GetComponent<PlatformEffector2D>() == null;
            else colliding = false;
        }
        else
            colliding = Physics2D.Raycast(startpos, raycastDir, raycastDist, blockMask);

        return colliding;
    }

    public void ReturnCamFocus()
    {
        PlayerController player = GameManager.gameManagerReference.player;

        Camera.main.GetComponent<CameraController>().focus = player.gameObject;
        Camera.main.GetComponent<CameraController>().lerp = true;
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

            Destroy(child);
        }
        else if (state == "antenna")
        {
            float t = 0f;
            while (t < 1f)
            {
                transform.GetChild(0).eulerAngles = Vector3.forward * Mathf.LerpAngle(transform.GetChild(0).eulerAngles.z, -20f, 0.4f);
                t += Time.deltaTime;
                yield return new WaitForSeconds(0.016f);
            }

            yield return new WaitForSeconds(0.5f);
            StaticController.self.Trigger(99, false);

            while (StaticController.self.staticAudio.volume < 0.5f)
            {
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForSeconds(0.5f);
        }

        animationPlayed = true;
    }


    public void UpdateEndPoint(EndPointNode endPoint)
    {
        if (endPoint.Power > 0f)
            lifetime = 1f;
    }
}
