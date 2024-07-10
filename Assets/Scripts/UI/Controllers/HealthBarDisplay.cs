using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarDisplay : MonoBehaviour
{
    public Transform target;
    public Vector2 offset;
    [SerializeField] Transform fill;
    [SerializeField] SpriteRenderer backgroundRenderer;
    [SerializeField] SpriteRenderer fillRenderer;
    [SerializeField] SpriteRenderer leftFillRenderer;
    [SerializeField] SpriteRenderer rightFillRenderer;
    [SerializeField] Transform rightFill;
    public Gradient colorGradient;
    public float life = 1f;
    public float lifetimeLenght = 3f;

    void Update()
    {
        if (target && lifetimeLenght > 0f)
        {
            transform.position = (Vector2)target.position + offset;
            CalculateLife();
            lifetimeLenght -= Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
            HealthBarManager.self.RemoveHealthBar(target);
        }
    }

    public void Trigger(float pLife)
    {
        lifetimeLenght = 3f;
        life = pLife;
        CalculateLife();
    }

    public void CalculateLife()
    {
        fill.localScale = new Vector3(life , 1, 1);
        rightFill.localPosition = Vector2.right * Mathf.Clamp(life - 1, -0.969f, 1f);
        Color color = colorGradient.Evaluate(life);
        if(lifetimeLenght < 1)
        {
            color.a = Mathf.Clamp01(lifetimeLenght);
        }

        fillRenderer.color = color;
        leftFillRenderer.color = color;
        rightFillRenderer.color = color;
        backgroundRenderer.color = color;
    }
}
