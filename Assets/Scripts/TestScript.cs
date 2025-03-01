using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestScript : MonoBehaviour
{

    public Image sprite;
    public EcoTexture texture;

    private void Start()
    {
        texture = new EcoTexture(20, 20);
        texture.FillWith(Color.white);
    }

    private void Update()
    {
        for(int x = 0; x < 20; x++)
        {
            for (int y = 0; y < 20; y++)
            {
                float randomValue = Random.Range(-10f * Time.deltaTime, 10f * Time.deltaTime);
                Color color = texture.GetPixel(x, y);
                texture.SetPixel(x, y, Mathf.Clamp(color.r + randomValue, 0f, 1f), Mathf.Clamp(color.g + randomValue, 0f, 1f), Mathf.Clamp(color.b + randomValue, 0f, 1f), 1);
            }
        }


        Texture2D renderedTexture;

        renderedTexture = texture.Export(FilterMode.Point);
        Sprite illuminationMap = Sprite.Create(renderedTexture, new Rect(0.0f, 0.0f, renderedTexture.width, renderedTexture.height), new Vector2(0.5f, 0.5f), 10, 0, SpriteMeshType.FullRect);
        sprite.sprite = illuminationMap;
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