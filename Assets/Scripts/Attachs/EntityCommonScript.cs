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
    public bool saveToFile = false;
    public bool canBeNetworked = true;
    public Rigidbody2D rb2D;

    [Header("States")]
    public List<EntityState> entityStates = new List<EntityState>();
    public List<float> stateDuration = new List<float>();
    public bool affectedByLiquids = true;
    public float swimming = 0;



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

        if (rb2D == null)
        {
            if (gameObject.GetComponent<Rigidbody2D>())
            {
                rb2D = gameObject.GetComponent<Rigidbody2D>();
                Debug.Log("Autoimported Rigidbody for " + gameObject.name);
            }
            else
            {
                Debug.LogError("Rigidbody of " + gameObject.name + " is null");
            }
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
            transform.position = transform.position + new Vector3(GameManager.gameManagerReference.WorldWidth * 16, 0);
            if (EntityType == "lecter")
            {
                LightController.lightController.AddRenderQueue(transform.position);
                GameManager.gameManagerReference.UpdateChunksRelPos();
            }
        }

        if (transform.position.x > GameManager.gameManagerReference.WorldWidth * 16)
        {
            transform.position = transform.position - new Vector3(GameManager.gameManagerReference.WorldWidth * 16, 0);
            if (EntityType == "lecter")
            {
                LightController.lightController.AddRenderQueue(transform.position);
                GameManager.gameManagerReference.UpdateChunksRelPos();
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (affectedByLiquids)
            if (collider.gameObject.layer == 10)
            {
                if (swimming == 0f)
                    rb2D.velocity = Vector2.ClampMagnitude(rb2D.velocity, 2);
                else
                    rb2D.velocity = Vector2.ClampMagnitude(rb2D.velocity, swimming);


                AddState(EntityState.Swimming, 0.1f);
                if (rb2D.transform.position.y < collider.transform.position.y + 0.5f)
                {
                    AddState(EntityState.Swimming, 0.1f);
                }
            }
    }
}

public enum EntityState : int
{
    OnFire, Paralisis, Drowning, Swimming
}