using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EntityCommonScript : MonoBehaviour
{
    [Header("Entity Config")]
    [SerializeField] public GameObject damagerObject;
    public IDamager entityDamager;
    public EntityBase entityBase;
    public string EntityType = "null";
    public string EntityFamily = "null";

    [Header("Secondary")]
    public List<EntityState> entityStates = new List<EntityState>();
    public List<float> stateDuration = new List<float>();
    public bool saveToFile = false;
    public bool canBeNetworked = true;

    private void OnValidate()
    {
        if(damagerObject == null)
        {
            Debug.LogWarning("Damager of " + gameObject.name + " is null");
        }
        else if (damagerObject.GetComponent<IDamager>() == null)
        {
            Debug.LogError("Object not containing a valid IDamager");
        }
        else
        {
            entityDamager = damagerObject.GetComponent<IDamager>();
        }
    }

    private void Start()
    {
        if (canBeNetworked)
            if (GameManager.gameManagerReference.isNetworkClient || GameManager.gameManagerReference.isNetworkHost)
            {
                gameObject.AddComponent<NetworkEntity>();
            }
    }

    public void AddState(EntityState state, float duration)
    {
        if (!entityStates.Contains(state))
        {
            entityStates.Add(state);
            stateDuration.Add(duration);
        }
        else
        {
            stateDuration[entityStates.IndexOf(state)] = duration;
        }
    }

    private void Update()
    {
        if(GameManager.gameManagerReference.InGame)
        for (int i = 0; i < entityStates.Count; i++)
        {
            stateDuration[i] = stateDuration[i] - Time.deltaTime;
            if(stateDuration[i] <= 0)
            {
                entityStates.RemoveAt(i);
                stateDuration.RemoveAt(i);
            }
        }

        if (transform.position.x < -1)
        {
            transform.Translate(new Vector2(GameManager.gameManagerReference.WorldWidth * 16, 0));
            if (EntityType == "lecter")
            {
                LightController.lightController.AddRenderQueue(transform.position);
                GameManager.gameManagerReference.UpdateChunksRelPos();
            }
        }

        if (transform.position.x > GameManager.gameManagerReference.WorldWidth * 16)
        {
            transform.Translate(new Vector2(-GameManager.gameManagerReference.WorldWidth * 16, 0));
            if (EntityType == "lecter")
            {
                LightController.lightController.AddRenderQueue(transform.position);
                GameManager.gameManagerReference.UpdateChunksRelPos();
            }
        }
    }
}

public enum EntityState : int
{
    OnFire, Paralisis,
}