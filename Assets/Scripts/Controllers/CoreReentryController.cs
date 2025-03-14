using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class CoreReentryController : MonoBehaviour
{
    public static CoreReentryController self;
    public float explosionHeight = 16f;
    public GameObject explosionPrefab;

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

        if (core == 96)
        {
            yield return StartCoroutine(FallFromSky());
        }
        else if (core == 97)
        {
            yield return StartCoroutine(FallFromSky());
        }
        else if (core == 100)
        {

        }

        Destroy(gameObject);
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
