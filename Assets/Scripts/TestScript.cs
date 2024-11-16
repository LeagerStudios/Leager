using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer spriteRenderer2;
    public SpriteRenderer spriteRenderer3;
    public SpriteRenderer spriteRenderer4;
    public float time = 0;
    public float apo = 2;
    public float per = 2;
    public float rot = 0;
    public float apo2 = 2;
    public float per2 = 2;
    public float rot2 = 0;
    public float apo3 = 2;
    public float per3 = 2;
    public float rot3 = 0;
    public float apo4 = 2;
    public float per4 = 2;
    public float rot4 = 0;

    private void Update()
    {
        spriteRenderer.transform.localPosition = FindPoint(apo, per, rot);
        spriteRenderer2.transform.localPosition = FindPoint(apo2, per2, rot2);
        spriteRenderer3.transform.localPosition = FindPoint(apo3, per3, rot3);
        spriteRenderer4.transform.localPosition = FindPoint(apo4, per4, rot4);
        spriteRenderer.GetComponent<TrailRenderer>().time = apo * per;
        spriteRenderer2.GetComponent<TrailRenderer>().time = apo2 * per2;
        spriteRenderer3.GetComponent<TrailRenderer>().time = apo3 * per3;
        spriteRenderer4.GetComponent<TrailRenderer>().time = apo4 * per4;
        if (time > 0.1f)
        {
            spriteRenderer.GetComponent<TrailRenderer>().emitting = true;
            spriteRenderer2.GetComponent<TrailRenderer>().emitting = true;
            spriteRenderer3.GetComponent<TrailRenderer>().emitting = true;
            spriteRenderer4.GetComponent<TrailRenderer>().emitting = true;
        }
       
        time += Time.deltaTime;
    }

    public Vector2 FindPoint(float apo, float per, float rot)
    {
        Vector2 point = new Vector2();
        float timePerRev = apo * per;
        float rotVal = time % timePerRev;
        rotVal *= 2;
        rotVal /= timePerRev;
        bool inverse = false;
        if (rotVal > 1)
        {
            rotVal = 0 - (rotVal - 2);
            inverse = true;
        }

        float orbit = per + apo;

        float halfOrbit = Mathf.Lerp(apo, per, 0.8f);


        point = new Vector2(Mathf.Sin(rotVal * Mathf.PI) * halfOrbit * (inverse ? 1 : -1), Mathf.Cos(rotVal * Mathf.PI) * (orbit / 2));
        point += Vector2.up * ((apo - per) / 2);
        if(rot != 0)
        {
            point = Quaternion.AngleAxis(rot, Vector3.forward) * point;
        }

        return point;
    }
}