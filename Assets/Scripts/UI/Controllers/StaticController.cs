using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaticController : MonoBehaviour
{
    public static StaticController self;
    public AudioSource staticAudio;
    public static EcoTexture texture;
    public Image sprite;
    public bool trigger = false;

    private void Awake()
    {
        self = this;
    }

    void Start()
    {
        if (texture == null)
            texture = new EcoTexture(20, 20);
        Static(false, false);
    }

    void Update()
    {
        if (self != this) Destroy(gameObject);

        bool condition = false;

        condition = GameManager.gameManagerReference.isNetworkClient && PlanetMenuController.planetMenu.timewarp > 1;

        if (!condition)
            condition = trigger;

        if (condition || staticAudio.volume > 0)
            Static(condition,false);
    }

    public void Trigger(float time, bool insta)
    {
        trigger = true;

        StartCoroutine(UnTrigger(time));

        if (insta)
            Static(true, true);
    }


    public IEnumerator UnTrigger(float time)
    {
        yield return new WaitForSeconds(time);
        trigger = false;
    }

    public bool Static(bool fill, bool insta)
    {
        float recall = fill ? 1f : 0f;

        for (int x = 0; x < 20; x++)
        {
            for (int y = 0; y < 20; y++)
            {
                float randomValue = Random.Range(-20f * Time.deltaTime, 20f * Time.deltaTime);
                Color color = texture.GetPixel(x, y);
                texture.SetPixel(x, y,
                    Mathf.Clamp(color.r + randomValue, 0f, 1f),
                    Mathf.Clamp(color.g + randomValue, 0f, 1f),
                    Mathf.Clamp(color.b + randomValue, 0f, 1f),
                    Mathf.Clamp(color.a + Random.Range(0f, fill ? 15f : -15f) * Time.deltaTime + (insta ? (fill ? 1 : -1) : 0), 0f, 1f));

                if (fill)
                {
                    if (color.a < recall)
                    {
                        recall = color.a;
                    }
                }
                else
                {
                    if (color.a > recall)
                    {
                        recall = color.a;
                    }
                }

            }
        }



        staticAudio.volume = recall * 0.5f;
        Texture2D renderedTexture;

        renderedTexture = texture.Export(FilterMode.Point);
        renderedTexture.wrapMode = TextureWrapMode.Repeat;
        Sprite illuminationMap = Sprite.Create(renderedTexture, new Rect(0.0f, 0.0f, renderedTexture.width, renderedTexture.height), new Vector2(0.5f, 0.5f), 10, 0, SpriteMeshType.FullRect);
        sprite.sprite = illuminationMap;

        if (fill)
        {
            return recall == 1f;
        }
        else
        {
            return recall == 0f;
        }
    }
}
