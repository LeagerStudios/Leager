using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreenController : MonoBehaviour {

    public static DeathScreenController deathScreenController;
    PlayerController player;
    bool called = false;
    [SerializeField] GameObject child1;
    [SerializeField] GameObject child2;
    [SerializeField] GameObject child3;

    MainSoundController soundController;
    [SerializeField] AudioClip instaKillSound;
    [SerializeField] Animator animator;
    EntityCommonScript procedence;


	void Start () {
        deathScreenController = this;
        child1.SetActive(false);
        child2.SetActive(false);
        child3.SetActive(false);
        player = GameObject.Find("Lecter").GetComponent<PlayerController>();
        soundController = GameObject.Find("Audio").GetComponent<MainSoundController>();
    }

    public void ResetScreen()
    {
        child1.SetActive(false);
        child2.SetActive(false);
        child3.SetActive(false);
        animator.SetBool("trigger", false);
        animator.SetBool("postDeath", false);
        called = false;
    }

    void Update()
    {
        if((!player.alive) && player.GetComponent<Rigidbody2D>().bodyType == RigidbodyType2D.Static && !called)
        {
            child1.SetActive(true);
            child2.SetActive(true);

            StartCoroutine(DeathScreen());
            called = true;
        }
    }

    public void InstaKill()
    {
        StartCoroutine(InstaDeathScreen());
        called = true;
    }

    public void StartDeath(EntityCommonScript procedence)
    {
        if (!called) StartCoroutine(IEDeath(procedence));
    }

    IEnumerator IEDeath(EntityCommonScript procedence)
    {
        this.procedence = procedence;
        Image image = transform.GetChild(0).GetComponent<Image>();
        child1.SetActive(true);

        Color startColor = image.color;
        startColor.a = 0f;
        image.color = startColor;

        for (int i = 0; i < 120; i++)
        {
            Color color = image.color;
            color.a += 1f / 120f;
            image.color = color;
            yield return new WaitForSeconds(0.016f);
            if (called) yield break;
        }
    }

    public void SetTriggerAnim(int value)
    {
        bool state = false;
        if (value > 0)
            state = true;

        animator.SetBool("trigger", state);
    }

    IEnumerator DeathScreen()
    {
        Image image = transform.GetChild(0).GetComponent<Image>();

        try
        {
            Camera.main.GetComponent<CameraController>().focus = procedence.gameObject;
        }
        finally
        {

        }

        Color startColor = image.color;
        startColor.a = 1f;
        image.color = startColor;

        for (int i = 0; i < 120; i++)
        {
            Color color = image.color;
            color.a -= 1f / 180f;
            image.color = color;
            yield return new WaitForSeconds(0.016f);
        }

        yield return new WaitForSeconds(0.7f);

        child3.SetActive(true);
    }

    IEnumerator InstaDeathScreen()
    {
        SetTriggerAnim(1);
        Image image = transform.GetChild(0).GetComponent<Image>();
        Color startColor = image.color;
        startColor.a = 1f;
        image.color = startColor;

        child1.SetActive(true);
        child2.SetActive(true);
        soundController.PlaySfxSound(instaKillSound);

        yield return new WaitForSeconds(1);
        animator.SetBool("postDeath", true);

        yield return new WaitForSeconds(2);

        child3.SetActive(true);
    }
}
