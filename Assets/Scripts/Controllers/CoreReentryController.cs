using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class CoreReentryController : MonoBehaviour
{
    public static CoreReentryController self;
    public float explosionHeight = 16f;
    public GameObject explosionPrefab;
    public GameObject flamePrefab;

    private void Awake()
    {
        self = this;
    }

    public void StartAnimation(int core)
    {
        StartCoroutine(Animation(core));
    }

    private IEnumerator Animation(int core)
    {
        transform.position = new Vector2(GameManager.gameManagerReference.player.transform.position.x, GameManager.gameManagerReference.WorldHeight + 15);
        GameManager.gameManagerReference.player.mainCamera.focus = gameObject;
        GameManager.gameManagerReference.player.rb2D.bodyType = RigidbodyType2D.Static;
        GameManager.gameManagerReference.player.Show = false;
        GameManager.gameManagerReference.player.onControl = false;
        GetComponent<SpriteRenderer>().sprite = GameManager.gameManagerReference.tiles[core];
        transform.localScale = Vector2.one;

        if (core == 96)
        {
            yield return StartCoroutine(FallFromSky());
            Destroy(gameObject);
        }
        else if (core == 97)
        {
            yield return StartCoroutine(FallFromSky());
            Destroy(gameObject);
        }
        else if (core == 100)
        {
            transform.localScale = Vector2.one * 2;
            yield return StartCoroutine(FallAndLand());
            //Destroy(gameObject);
        }


        
    }

    private IEnumerator FallFromSky()
    {
        Vector2 velocity = new Vector2(-16.5f, -16.5f);

        ParticleSystem particles = GameManager.gameManagerReference.player.transform.GetChild(6).GetComponent<ParticleSystem>();
        particles.transform.eulerAngles = Vector3.forward * ManagingFunctions.PointToPivotUp(Vector2.zero, velocity);

        ParticleSystem.MainModule main = particles.main;
        main.startSpeed = velocity.magnitude;

        if (!particles.isPlaying)
            particles.Play();

        

        while (true)
        {
            transform.position += (Vector3)velocity * Time.deltaTime;
            transform.eulerAngles += Vector3.forward * 720 * Time.deltaTime;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, velocity, 15, GameManager.gameManagerReference.player.interactableTilesLayer);

            if (hit)
            {
                if (hit.distance <= explosionHeight)
                {
                    if (hit.distance < 1)
                    {
                        Explode();
                        break;
                    }
                    else if (GameManager.gameManagerReference.player.onControl == false)
                        EjectPlayer();
                }
            }

            if (GameManager.gameManagerReference.player.rb2D.bodyType == RigidbodyType2D.Static)
            {
                GameManager.gameManagerReference.player.transform.position = transform.position;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator FallAndLand()
    {
        Vector2 velocity = new Vector2(-16.5f, -16.5f);

        ParticleSystem particles = GameManager.gameManagerReference.player.transform.GetChild(6).GetComponent<ParticleSystem>();
        particles.transform.eulerAngles = Vector3.forward * ManagingFunctions.PointToPivotUp(Vector2.zero, velocity);

        ParticleSystem.MainModule main = particles.main;
        main.startSpeed = velocity.magnitude;

        if (!particles.isPlaying)
            particles.Play();

        RaycastHit2D hit = Physics2D.Raycast(transform.position, velocity, 20f, GameManager.gameManagerReference.player.interactableTilesLayer);

        while (hit.distance > 16.5f || !hit)
        {
            transform.position += (Vector3)velocity * Time.deltaTime;
            transform.eulerAngles = Vector3.forward * -45f;
            hit = Physics2D.Raycast(transform.position, velocity, 16.5f, GameManager.gameManagerReference.player.interactableTilesLayer);

            if (GameManager.gameManagerReference.player.rb2D.bodyType == RigidbodyType2D.Static)
            {
                GameManager.gameManagerReference.player.transform.position = transform.position;
            }

            yield return new WaitForEndOfFrame();
        }

        particles.Stop();
        CoreFire coreFire = Instantiate(flamePrefab, transform).GetComponent<CoreFire>();
        coreFire.transform.eulerAngles = Vector3.zero;
        coreFire.transform.localPosition = new Vector2(0, -0.25f);

        while (hit.distance > 2f || !hit)
        {
            if (hit)
            {
                velocity = new Vector2(Mathf.Clamp(velocity.x - velocity.x * Time.deltaTime * 3, velocity.y, 0), -hit.distance + 0.4f);
                coreFire.rotPerSec = 3f / hit.distance;
            }
                
            transform.position += (Vector3)velocity * Time.deltaTime;
            transform.eulerAngles = Vector3.forward * (ManagingFunctions.PointToPivotUp(Vector2.zero, velocity) + 180);

            hit = Physics2D.Raycast(transform.position, velocity, 20f, GameManager.gameManagerReference.player.interactableTilesLayer);

            if (GameManager.gameManagerReference.player.rb2D.bodyType == RigidbodyType2D.Static)
            {
                GameManager.gameManagerReference.player.transform.position = transform.position;
            }

            yield return new WaitForEndOfFrame();
        }

        Vector2 initial = transform.position;
        float initialAngle = transform.localEulerAngles.z;
        int x = (int)hit.point.x;
        int y = (int)hit.point.y + 1;
        float t = 0f;
        float tVelocity = 0f;

        tVelocity = hit.distance - 0.4f;

        while (t < 1)
        {
            transform.eulerAngles = Vector3.forward * Mathf.LerpAngle(initialAngle, 0f, t * 1.2f);

            if (tVelocity > 1.05f - t)
                tVelocity = Mathf.Clamp(tVelocity - Time.deltaTime, 0, 1);

            t += tVelocity * Time.deltaTime;
            transform.position = Vector2.Lerp(initial, new Vector2(x + 0.5f, y), t);

            yield return new WaitForEndOfFrame();
        }

        coreFire.endOnNext = true;

        while(coreFire != null)
            yield return new WaitForEndOfFrame();

        GameManager.gameManagerReference.SetTileAt(ManagingFunctions.CreateIndex(new Vector2Int(x, y)), 98, false, true);
        GameManager.gameManagerReference.SetTileAt(ManagingFunctions.CreateIndex(new Vector2Int(x + 1, y)), 99, false, true);

        SpriteRenderer renderer1 = transform.GetChild(0).GetComponent<SpriteRenderer>();
        Transform scanLine = transform.GetChild(0).GetChild(0);

        float sinWave = Mathf.PI;

        while (renderer1.color.a < 1f || scanLine.localPosition.y > -1.15f)
        {
            sinWave += Time.deltaTime * 3;

            renderer1.color = new Color(1, 1, 1, renderer1.color.a + Time.deltaTime / 5f);
            scanLine.localPosition = new Vector2(-0.5f, Mathf.Cos(sinWave) * 1.2f);

            yield return new WaitForEndOfFrame();
        }

        transform.eulerAngles = Vector3.zero;
        GameManager.gameManagerReference.player.mainCamera.lerp = true;
        GameManager.gameManagerReference.player.Respawn(transform.position.x, transform.position.y + 0.5f + GameManager.gameManagerReference.player.entityScript.height);
        GameManager.gameManagerReference.player.rb2D.velocity = Vector2.zero;
        GameManager.gameManagerReference.player.damagedCooldown = 0f;
        GameManager.gameManagerReference.player.Show = true;
        GameManager.gameManagerReference.player.onControl = true;
        GameManager.gameManagerReference.player.GetComponent<SpriteRenderer>().flipX = false;
        renderer1.color = new Color(1, 1, 1, 0);
    }

    private void EjectPlayer()
    {
        GameManager.gameManagerReference.player.Respawn(transform.position.x, transform.position.y);
        GameManager.gameManagerReference.player.rb2D.velocity = new Vector2(-4f, 1f);
        GameManager.gameManagerReference.player.Show = true;
        GameManager.gameManagerReference.player.PutMyselfInATragicTerminalColisionCourse();
        GameManager.gameManagerReference.player.onControl = true;
    }

    private void Explode()
    {
        GameManager.gameManagerReference.soundController.PlaySfxSound(SoundName.Explosion1);
        Instantiate(explosionPrefab, transform.position, Quaternion.identity).transform.localScale = new Vector3(6f, 6f, 1f);
        GameManager.gameManagerReference.TileExplosionAt((int)transform.position.x, (int)transform.position.y, 3, 4);

    }
}
