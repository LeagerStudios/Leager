using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarManager : MonoBehaviour
{
    public static HealthBarManager self;
    public Dictionary<Transform, HealthBarDisplay> healthBars;
    public GameObject healthBarPrefab;

    void Start()
    {
        self = this;
        healthBars = new Dictionary<Transform, HealthBarDisplay>();
    }

    void Update()
    {
        
    }

    public void RemoveHealthBar(Transform target)
    {
        if (target != null)
            healthBars.Remove(target);

    }

    public void UpdateHealthBar(Transform target, float hp, float maxHP, Vector2 offset)
    {
        if (healthBars.TryGetValue(target, out HealthBarDisplay healthBar))
        {
            healthBar.Trigger(Mathf.Clamp01(hp / maxHP));
        }
        else
        {
            HealthBarDisplay healthBarDisplay = Instantiate(healthBarPrefab, (Vector2)target.position + offset, Quaternion.identity).GetComponent<HealthBarDisplay>();
            healthBarDisplay.target = target;
            healthBarDisplay.Trigger(Mathf.Clamp01(hp / maxHP));
            healthBarDisplay.offset = offset;
            healthBars.Add(target, healthBarDisplay);
        }
    }
}
