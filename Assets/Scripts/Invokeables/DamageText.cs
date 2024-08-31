using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    [SerializeField] TextMeshPro text;
    float sinValue = 0;
    public int damage = 0;
    public float ySpeed = 1f;

    public void StartCall(int dmg)
    {
        damage = dmg;
        text.text = damage + "";
        text.color = Color.red;
        text.fontSize = 3.5f + damage / 10;
        transform.Translate(new Vector3(Random.Range(-0.5f, 0.5f), 0));
    }

    void Update()
    {
        if (/*GameManager.gameManagerReference.InGame*/true)
        {
            sinValue += Time.deltaTime;
            float grav = Time.deltaTime * 5;
            ySpeed -= grav;
            transform.Translate(Vector2.up * ySpeed * Time.deltaTime);


            float green = Mathf.Sin(sinValue * Mathf.Deg2Rad * 720);
            text.color = new Color(1, Mathf.Abs(green), 0, 1f + (2 - sinValue * 2));
            if (text.color.a <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
