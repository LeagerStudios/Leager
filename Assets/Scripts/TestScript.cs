using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestScript : MonoBehaviour
{

    public Image sprite;
    public EcoTexture texture;
    public AudioSource audio;

    private void Start()
    {
        texture = new EcoTexture(20, 20);
    }

    private void Update()
    {
        sprite.color = Static(Input.GetKey(KeyCode.F)) ? Color.grey : Color.white;
    }

    public bool Static(bool fill)
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
                    Mathf.Clamp(color.a + Random.Range(0f, fill ? 15f : -15f) * Time.deltaTime, 0f, 1f));

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



        audio.volume = recall * 0.5f;
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

    //public Vector2 FindPoint(float apo, float per, float rot)
    //{
    //    Vector2 point = new Vector2();
    //    float timePerRev = apo * per;
    //    float rotVal = time % timePerRev;
    //    rotVal *= 2;
    //    rotVal /= timePerRev;
    //    bool inverse = false;
    //    if (rotVal > 1)
    //    {
    //        rotVal = 0 - (rotVal - 2);
    //        inverse = true;
    //    }

    //    float orbit = per + apo;

    //    float halfOrbit = Mathf.Lerp(apo, per, 0.8f);


    //    point = new Vector2(Mathf.Sin(rotVal * Mathf.PI) * halfOrbit * (inverse ? 1 : -1), Mathf.Cos(rotVal * Mathf.PI) * (orbit / 2));
    //    point += Vector2.up * ((apo - per) / 2);
    //    if(rot != 0)
    //    {
    //        point = Quaternion.AngleAxis(rot, Vector3.forward) * point;
    //    }

    //    return point;
    //}
}