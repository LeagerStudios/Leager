using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour {

    [SerializeField] RectTransform bar;
    [SerializeField] RectTransform armorBar;
    [SerializeField] RectTransform cooldownBar;
    [SerializeField] Image blueHeart;
    [SerializeField] Text nLives;
    float targetSlider = 0;
    float previousSlider = 0;
    float timeSlider = 0;
    float maxCooldown = 0.01f;
    int maxHealth = 20;
    public int defense = 0;

    public float MaxCooldown
    {
        set { maxCooldown = Mathf.Clamp(value, 0.01f, 9999f); }
    }

    public float cooldown = 0f;


    public void Update()
    {
        defense = GameManager.gameManagerReference.ToolEfficency[GameManager.gameManagerReference.equipedArmor[0]] + GameManager.gameManagerReference.ToolEfficency[GameManager.gameManagerReference.equipedArmor[1]] + GameManager.gameManagerReference.ToolEfficency[GameManager.gameManagerReference.equipedArmor[2]];
        defense = Mathf.Clamp(defense, 0, maxHealth);

        if (timeSlider < 1f)
        {
            timeSlider += Time.deltaTime * 10;
        }
        if (timeSlider > 1) timeSlider = 1;



        bar.sizeDelta = new Vector2(Mathf.Lerp(previousSlider, targetSlider, timeSlider) * 130 / maxHealth, 20);
        cooldownBar.sizeDelta = new Vector2(cooldown / maxCooldown * 65, 5);
        armorBar.sizeDelta = new Vector2(defense * 130 / maxHealth * (bar.sizeDelta.x / 130), 20);
        blueHeart.fillAmount = Mathf.MoveTowards(blueHeart.fillAmount, (float)defense / maxHealth, Time.deltaTime * Mathf.Abs(defense - blueHeart.fillAmount * maxHealth));
    }

    public void SetHealth(int health)
    {
        timeSlider = 0;
        previousSlider = bar.sizeDelta.x / 130 * maxHealth;
        targetSlider = Mathf.Clamp(health, 0, maxHealth);
        nLives.text = Mathf.Clamp(health, 0, maxHealth) + "";
    }

    public void SetMaxHealth(int value)
    {
        maxHealth = value;
        SetHealth((int)targetSlider);
    }
}
