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
    public float height = 1f;
    public float width = 0.35f;
    public bool raycastHeight = false;

    [Header("Secondary")]
    public bool saveToFile = false;
    public bool canBeNetworked = true;
    public Rigidbody2D rb2D;

    [Header("States")]
    public List<EntityState> entityStates = new List<EntityState>();
    public List<float> stateDuration = new List<float>();
    public bool affectedByLiquids = true;
    public bool affectedBySolids = true;
    public float swimming = 0;
    public float ladderVelocity = 0;
    public LayerMask blockMask;



    private void OnValidate()
    {
        if (damagerObject == null)
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

        if (raycastHeight)
        {
            Debug.DrawLine(transform.position, transform.position + Vector3.down * height, Color.blue, 1f);
        }
    }

    private void Start()
    {
        if (canBeNetworked)
            if (GameManager.gameManagerReference.isNetworkClient || GameManager.gameManagerReference.isNetworkHost)
            {
                gameObject.AddComponent<NetworkEntity>();
            }

        blockMask = LayerMask.GetMask(new string[] { "Block" });
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
        if (GameManager.gameManagerReference.InGame)
            for (int i = 0; i < entityStates.Count; i++)
            {
                if(entityStates[i] == EntityState.OnFire)
                {
                    if (entityStates.Contains(EntityState.Swimming))
                    {
                        entityStates.RemoveAt(i);
                        stateDuration.RemoveAt(i);
                        continue;
                    }
                }
                
                stateDuration[i] = stateDuration[i] - Time.deltaTime;
                if (stateDuration[i] <= 0)
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
                    rb2D.velocity = Vector2.ClampMagnitude(rb2D.velocity, 4);
                else
                    rb2D.velocity = Vector2.ClampMagnitude(rb2D.velocity, swimming);


                AddState(EntityState.Swimming, 0.1f);
                if (rb2D.transform.position.y + height < collider.transform.position.y + 0.5f)
                {
                    AddState(EntityState.Drowning, 0.1f);
                }

                SpriteRenderer renderer = collider.gameObject.GetComponent<SpriteRenderer>();

                if (renderer != null)
                {
                    if(renderer.sprite == GameManager.gameManagerReference.tiles[21])
                    {
                        AddState(EntityState.Burning, 5f);
                    }
                }
            }

        if (affectedBySolids)
        {
            SlopePhysics(collider);
        }

        if (ladderVelocity != 0)
            if (collider.gameObject.layer == 20)
            {
                if (!Physics2D.Raycast(transform.position, Vector2.up, height + 0.01f, blockMask) &&
                    !Physics2D.Raycast(transform.position + Vector3.right * width * 0.5f, Vector2.up, height + 0.01f, blockMask) &&
                    !Physics2D.Raycast(transform.position - Vector3.right * width * 0.5f, Vector2.up, height + 0.01f, blockMask))
                {
                    if (collider.transform.position.y + 1f > transform.position.y)
                        rb2D.velocity = new Vector2(rb2D.velocity.x, Mathf.Clamp(rb2D.velocity.y + ladderVelocity * Time.deltaTime * 3, rb2D.velocity.y, ladderVelocity));
                }
                else
                {
                    rb2D.velocity = new Vector2(rb2D.velocity.x, 0f);
                }
            }
    }

    private void SlopePhysics(Collider2D collider)
    {
        if (collider.gameObject.layer == 17)
        {
            float head = transform.position.y + height;
            float relativex = transform.position.x + width - collider.transform.position.x + 0.6f;
            float ladderPoint = Mathf.Clamp(relativex, -0.1f, 1) + collider.transform.position.y - 0.5f;
            float feet = transform.position.y - height;

            if (feet > ladderPoint + Mathf.Clamp(rb2D.velocity.y, -99f, 0f) * 2 - 0.05f)
            {
                Debug.Log(relativex + " - " + ladderPoint);
                if (feet < ladderPoint)
                {
                    transform.position = new Vector2(transform.position.x, ladderPoint + height);
                    rb2D.velocity = new Vector2(rb2D.velocity.x * 0.9f, 0f);
                }
            }
            else
            {
                if(head < ladderPoint)
                {
                    transform.position = new Vector2(transform.position.x, collider.transform.position.y - 0.5f - height);
                    rb2D.velocity = Vector2.zero;
                }
                else if(transform.position.x < collider.transform.position.x - 0.5f)
                {
                    transform.position = new Vector2(collider.transform.position.x - 0.5f - width, transform.position.y);
                    rb2D.velocity = new Vector2(0f, rb2D.velocity.y);
                }
                else
                {
                    transform.position = new Vector2(collider.transform.position.x + 0.5f + width, transform.position.y);
                    rb2D.velocity = new Vector2(0f, rb2D.velocity.y);
                }
            }



        }
        
    }
}



public enum EntityState : int
{
    OnFire, Paralisis, Drowning, Swimming, Burning, FireResistance,
}