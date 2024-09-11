using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour {

    [SerializeField] Slider slider;
    [SerializeField] RectTransform cooldownBar;
    [SerializeField] Text nLives;
    float targetSlider = 0;
    float previousSlider = 0;
    float timeSlider = 0;
    float maxCooldown = 0.01f;

    public float MaxCooldown
    {
        set { maxCooldown = Mathf.Clamp(value, 0.01f, 9999f); }
    }

    public float cooldown = 0f;


    public void Update()
    {
        if(timeSlider < 1f)
        {
            timeSlider += Time.deltaTime * 10;
        }
        if (timeSlider > 1) timeSlider = 1;

        slider.value = Mathf.Lerp(previousSlider, targetSlider, timeSlider);
        cooldownBar.sizeDelta = new Vector2(cooldown / maxCooldown * 65, 5);
    }

    public void SetHealth(int health)
    {
        timeSlider = 0;
        previousSlider = slider.value;
        targetSlider = Mathf.Clamp(health, 0, slider.maxValue);
        nLives.text = Mathf.Clamp(health, 0, slider.maxValue) + "";
    }

    public void SetMaxHealth(int value)
    {
        slider.maxValue = value;
    }
}
