using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour, IDamager {

    [SerializeField] float MaxSpeed;
    [SerializeField] float AccelerationInGround;
    [SerializeField] float AccelerationInAir;
    [SerializeField] float JumpForce;
    [SerializeField] HealthBarController healthBar;
    [SerializeField] GameObject particle;
    [SerializeField] LayerMask[] tileType;
    [SerializeField] public DeathScreenController deathScreenController;
    Animator animations;
    Rigidbody2D rb2D;
    MainSoundController soundController;
    bool spawned = false;
    public float damagedCooldown = 0.5f;
    public float handOffset = 0.062f;
    public float regenTime = 0f;
    public bool killing = false;
    public bool alive = true;
    public bool onControl = true;
    public int HP;
    public int MaxHP;
    GameManager gameManager;
    public float falling = 0f;
    public int skin = 0;

    void Start() //obtener referencias
    {
        rb2D = GetComponent<Rigidbody2D>();
        animations = GetComponent<Animator>();
        soundController = GameObject.Find("Audio").GetComponent<MainSoundController>();
        gameManager = GameManager.gameManagerReference;
    }

    void Update () {
        if (spawned)
        {
            if (alive && gameManager.InGame)
            {
                if (onControl)
                    PlayerControl();
                LecterAI();
                Camera.main.transform.eulerAngles = Vector3.zero;
            }

            if (!alive && !killing && gameManager.InGame)
            {
                Camera.main.transform.eulerAngles += Vector3.forward * Time.deltaTime;
                Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - 0.05f * Time.deltaTime, 2f, 5f);
            }
        }
        else
        {
            transform.position = new Vector2(0, 0);
        }
	}

    private void LateUpdate()
    {
        if (skin != 0)
        {
            GetComponent<SpriteRenderer>().sprite = gameManager.playerSkins[skin].skin[System.Array.IndexOf(gameManager.playerSkins[0].skin, GetComponent<SpriteRenderer>().sprite)];
            transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = gameManager.playerSkins[skin].skin[System.Array.IndexOf(gameManager.playerSkins[0].skin, transform.GetChild(2).GetComponent<SpriteRenderer>().sprite)];
        }
    }

    public void Hit(int damage, EntityCommonScript procedence, bool ignoreImunity = false, float knockback = 1f, bool penetrate = false)
    {
        if(procedence.EntityFamily != "yellow")
        {
            LoseHp(damage, ignoreImunity, knockback, penetrate);
        }
    }

    public void Respawn(float x,float y)
    {
        Time.timeScale = 1f;
        damagedCooldown = 0.5f;
        MaxHP = 20;//Vida
        HP = MaxHP;//Vida
        falling = 0;
        killing = false;
        alive = true;
        rb2D = GetComponent<Rigidbody2D>();
        animations = GetComponent<Animator>();
        transform.GetChild(4).GetComponent<DamagersCollision>().target = this;
        healthBar.SetMaxHealth(HP);
        healthBar.SetHealth(HP);
        animations.SetBool("killed", false);
        GetComponent<SpriteRenderer>().enabled = true;
        for (int i = 0; i < 3; i++)
        {
            transform.GetChild(1).GetChild(i).GetComponent<SpriteRenderer>().enabled = true;
        }
        transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
        transform.GetChild(2).GetComponent<SpriteRenderer>().enabled = true;
        rb2D.bodyType = RigidbodyType2D.Dynamic;
        transform.position = new Vector2(x, y);
        Camera.main.orthographicSize = 5f;
        spawned = true;//confirmación final
    }



    void PlayerControl()
    {
        CapsuleCollider2D collider2D = GetComponent<CapsuleCollider2D>();

        if (GetComponent<SpriteRenderer>().flipX)
        {
            collider2D.offset = new Vector2(0.075f, 0);
            transform.GetChild(2).localPosition = new Vector2(handOffset, transform.GetChild(2).localPosition.y);
        }
        else
        {
            collider2D.offset = new Vector2(-0.075f, 0);
            transform.GetChild(2).localPosition = new Vector2(-handOffset, transform.GetChild(2).localPosition.y);
        }
        transform.GetChild(2).GetComponent<SpriteRenderer>().flipX = GetComponent<SpriteRenderer>().flipX;

        float raycastDistance = 0.7f;
        bool Grounded = false;
        Vector2 pos = new Vector2(transform.position.x + collider2D.offset.x, transform.position.y);

        if (Physics2D.Raycast(pos, Vector2.down, raycastDistance, tileType[0])) Grounded = true;
        if (Physics2D.Raycast(pos, Vector2.down, raycastDistance, tileType[0]))
        {
            Debug.DrawRay(pos, Vector3.down * raycastDistance, Color.green);
        }
        else
        {
            Debug.DrawRay(pos, Vector3.down * raycastDistance, Color.red);
        }

        pos = new Vector2(transform.position.x + collider2D.size.x / 2.1f + collider2D.offset.x, pos.y);

        if (Physics2D.Raycast(pos, Vector2.down, raycastDistance, tileType[0])) Grounded = true;
        if (Physics2D.Raycast(pos, Vector2.down, raycastDistance, tileType[0]))
        {
            Debug.DrawRay(pos, Vector3.down * raycastDistance, Color.green);
        }
        else
        {
            Debug.DrawRay(pos, Vector3.down * raycastDistance, Color.red);
        }

        pos = new Vector2(transform.position.x, transform.position.y);
        pos = new Vector2(transform.position.x - collider2D.size.x / 2.1f + collider2D.offset.x, pos.y);

        if (Physics2D.Raycast(pos, Vector2.down, raycastDistance, tileType[0])) Grounded = true;
        if (Physics2D.Raycast(pos, Vector2.down, raycastDistance, tileType[0]))
        {
            Debug.DrawRay(pos, Vector3.down * raycastDistance, Color.green);
        }
        else
        {
            Debug.DrawRay(pos, Vector3.down * raycastDistance, Color.red);
        }

        pos = new Vector2();

        animations.SetBool("walking", false);

        if (GInput.GetKey(KeyCode.A))
        {
            if (rb2D.velocity.x > (0 - MaxSpeed))
            {
                if (Grounded)
                {
                    rb2D.AddForce(new Vector2((0 - AccelerationInGround) * Time.deltaTime, 0));
                }
                else
                {
                    rb2D.AddForce(new Vector2((0 - AccelerationInAir) * Time.deltaTime, 0));
                }
                GetComponent<SpriteRenderer>().flipX = true;
                animations.SetBool("walking", true);
            }
        }

        if (GInput.GetKey(KeyCode.D))
        {
            if (rb2D.velocity.x < MaxSpeed)
            {
                if (Grounded)
                {
                    rb2D.AddForce(new Vector2(AccelerationInGround * Time.deltaTime, 0));
                }
                else
                {
                    rb2D.AddForce(new Vector2(AccelerationInAir * Time.deltaTime, 0));
                }
                GetComponent<SpriteRenderer>().flipX = false;
                animations.SetBool("walking", true);
            }
        }

        if (GetComponent<SpriteRenderer>().flipX)
        {
            transform.GetChild(2).localPosition = new Vector2(handOffset, transform.GetChild(2).localPosition.y);
        }
        else
        {
            transform.GetChild(2).localPosition = new Vector2(-handOffset, transform.GetChild(2).localPosition.y);
        }
        transform.GetChild(2).GetComponent<SpriteRenderer>().flipX = GetComponent<SpriteRenderer>().flipX;

        if ((GInput.GetKeyDown(KeyCode.A) || GInput.GetKeyDown(KeyCode.D)) && Grounded)
        {
            SoundOndaSpawner.MakeSoundOnda(new Vector2(transform.position.x, transform.position.y - 0.65f));
        }

        if (GInput.GetKey(KeyCode.S))
        {
            gameManager.DropOn(transform.position - new Vector3(0f, 0.6f, 0f), 0.5f);
        }

        if (Grounded && alive)
        {
            if (Mathf.Round(falling) >= 5f)
            {
                int HpLose = Mathf.RoundToInt(falling);
                LoseHp(HpLose, true, 0f, true);
                falling = 0;

                if (HP <= 0)
                {
                    deathScreenController.InstaKill();
                }
            }
        }

        if (rb2D.velocity.y < 0)
        {
            falling -= rb2D.velocity.y * Time.deltaTime;
        }
        else falling = 0f;

        if (GInput.GetKeyDown(KeyCode.W) && Grounded && rb2D.velocity.y >= 0)
        {
            rb2D.velocity = Vector2.right * rb2D.velocity.x;
            rb2D.AddForce(new Vector2(0, JumpForce));
        }

        //if (GInput.GetKey(KeyCode.W))
        //{
        //    gameManager.FloatOn(transform.position - new Vector3(0f, 0.6f, 0f), 0.5f);
        //}

        if (GInput.GetKey(KeyCode.Space))
        {

        }




        if ((rb2D.velocity.y > 0.1f && GInput.GetKey(KeyCode.W)) || rb2D.velocity.y > 2f)
        {
            animations.SetBool("falling", false);
            animations.SetBool("jumping", true);
        }
        else if(rb2D.velocity.y < 0.1f && rb2D.velocity.y > -0.1f)
        {
            animations.SetBool("falling", false);
            animations.SetBool("jumping", false);
        }
        else
        {
            animations.SetBool("falling", true);
            animations.SetBool("jumping", false);
        }

        if(transform.position.y < -30f)
        {
            if(!killing) StartCoroutine(KillAllLives(1));
            killing = true;
        }

        if (GInput.GetKeyDown(KeyCode.Q) && !StackBar.stackBarController.InventoryDeployed)
        {
            PlayerRelativeDrop(StackBar.stackBarController.StackBarGrid[StackBar.stackBarController.idx], 1);
            StackBar.LoseItem();
        }

        if (GInput.GetMouseButtonDown(0) && gameManager.usingArm && !gameManager.cancelPlacing)
        {
            StartCoroutine(ArmAnimation(gameManager.armUsing));

            if (gameManager.armUsing == "bow")
            {
                if (StackBar.Search(64, 1) != -1)
                {
                    StackBar.LoseItem(StackBar.Search(64, 1));
                    Vector2 vector = gameManager.mouseCurrentPosition - transform.position;
                    vector.x = vector.x * -1;
                    PROJECTILE_Arrow.StaticSpawn(ManagingFunctions.PointToPivotUp(Vector2.zero, vector), transform.position, gameManager.ToolEfficency[StackBar.stackBarController.StackBarGrid[StackBar.stackBarController.idx]], GetComponent<EntityCommonScript>());
                }
                else if(InventoryBar.Search(64, 1) != -1)
                {
                    InventoryBar.LoseItem(InventoryBar.Search(64, 1));
                    Vector2 vector = gameManager.mouseCurrentPosition - transform.position;
                    vector.x = vector.x * -1;
                    PROJECTILE_Arrow.StaticSpawn(ManagingFunctions.PointToPivotUp(Vector2.zero, vector), transform.position, gameManager.ToolEfficency[StackBar.stackBarController.StackBarGrid[StackBar.stackBarController.idx]], GetComponent<EntityCommonScript>());
                }
            }
        }

        if (damagedCooldown > 0f)
        {
            damagedCooldown -= Time.deltaTime;
            animations.SetBool("damaged", true);
        }
        else
        {
            animations.SetBool("damaged", false);
            damagedCooldown = 0f;
        }

        if (regenTime < 0f)
        {
            regenTime = 1f;
            if (HP < MaxHP) LoseHp(-1);
        }
        else
        {
            regenTime -= Time.deltaTime;
        }

        for(int i = 0; i < 3; i++)
        {
            transform.GetChild(1).GetChild(i).GetComponent<SpriteRenderer>().color = gameManager.rawColor[gameManager.equipedArmor[i]];
            transform.GetChild(1).GetChild(i).GetComponent<SpriteRenderer>().flipX = GetComponent<SpriteRenderer>().flipX;
        }

        transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().color = gameManager.rawColor[gameManager.equipedArmor[1]];
        transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().flipX = GetComponent<SpriteRenderer>().flipX;
    }

    void LecterAI()
    {
        if (GetComponent<EntityCommonScript>().entityStates.Contains(EntityState.OnFire))
        {
            transform.GetChild(3).GetComponent<SpriteRenderer>().enabled = true;
            LoseHp(1, false, 0);
        }
        else
        {
            transform.GetChild(3).GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    public void UseToolAnim()
    {
        if (gameManager.usingTool && transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite == gameManager.tiles[0])
        {
            StartCoroutine(ArmAnimation("tool"));
        }
    }

    public void PlayerRelativeDrop(int item, int amount)
    {
        GameObject newDroppedItem = ManagingFunctions.DropItem(item, transform.position, amount, 3);

        if (newDroppedItem != null)
        {
            if (!GetComponent<SpriteRenderer>().flipX)
            {
                newDroppedItem.GetComponent<Rigidbody2D>().AddForce(new Vector2(350f, 100f + (rb2D.velocity.y * 50)));
            }
            else
            {
                newDroppedItem.GetComponent<Rigidbody2D>().AddForce(new Vector2(-350f, 100f + (rb2D.velocity.y * 50)));
            }
        }
    }

    public void LoseHp(int hpLost, bool ignoreImunity = false, float knockback = 1f, bool penetrate = false)
    {
        soundController.PlaySfxSound(SoundName.damage);

        if (gameManager.InGame && alive)
        {
            if (hpLost > 0)
            {
                if (!penetrate)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        hpLost -= gameManager.ToolEfficency[gameManager.equipedArmor[i]];
                    }
                }

                if (hpLost < 1)
                {
                    hpLost = 1;
                }
            }

            if (damagedCooldown < 0.1f | ignoreImunity)
            {
                HP -= hpLost;
                healthBar.SetHealth(HP);

                if (HP < 1)
                {
                    healthBar.SetHealth(0);

                    if (GetComponent<SpriteRenderer>().flipX)
                    {
                        rb2D.AddForce(new Vector2((AccelerationInGround * 0.5f) * knockback, (JumpForce * 0.6f) * knockback));
                    }
                    else
                    {
                        rb2D.AddForce(new Vector2((AccelerationInGround * -0.5f) * knockback, (JumpForce * 0.6f) * knockback));
                    }

                    StartCoroutine(Kill());
                }
                else if (hpLost > 0)
                {
                    if (GetComponent<SpriteRenderer>().flipX)
                    {
                        rb2D.AddForce(new Vector2((AccelerationInGround * 0.5f) * knockback, (JumpForce * 0.6f) * knockback));
                    }
                    else
                    {
                        rb2D.AddForce(new Vector2((AccelerationInGround * -0.5f) * knockback, (JumpForce * 0.6f) * knockback));
                    }
                    damagedCooldown = 0.6f;
                    regenTime = 3.5f;
                }
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!alive && killing)
        {
            DeathFase();
        }
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    Debug.Log("heya", collision.gameObject);
    //}

    IEnumerator Kill()
    {
        alive = false;
        killing = true;
        deathScreenController.StartDeath();
        animations.SetBool("killed", true);
        Time.timeScale = 0.75f;
        yield return new WaitForSeconds(2);
        if (GetComponent<SpriteRenderer>().enabled)
        {
            DeathFase();
        }
    }

    void DeathFase()
    {
        Time.timeScale = 1f;
        killing = false;
        GetComponent<SpriteRenderer>().enabled = false;
        for (int i = 0; i < 3; i++)
        {
            transform.GetChild(1).GetChild(i).GetComponent<SpriteRenderer>().enabled = false;
        }
        transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        transform.GetChild(2).GetComponent<SpriteRenderer>().enabled = false;
        transform.GetChild(3).GetComponent<SpriteRenderer>().enabled = false;

        rb2D.bodyType = RigidbodyType2D.Static;
        for (int i = 0; i < 20; i++)
        {
            GameObject g = Instantiate(particle, transform.position, Quaternion.identity);
            g.GetComponent<ParticleController>().Spawn();
        }
        for(int i = 0;i < gameManager.equipedArmor.Length; i++)
        {
            if(gameManager.equipedArmor[i] > 0)
            {
                GameObject newDroppedArmorItem = ManagingFunctions.DropItem(gameManager.equipedArmor[i], transform.position + Vector3.up);
                newDroppedArmorItem.GetComponent<Rigidbody2D>().AddForce(Vector2.right * (i - 1) * 250);
                gameManager.equipedArmor[i] = 0;
            }
        }
    }

    IEnumerator KillAllLives(float secs)
    {
        killing = true;

        float secsWait = secs / (HP / 5);
        int liveLost = Mathf.FloorToInt(HP / 5);

        while (alive)
        {
            LoseHp(liveLost, true);
            yield return new WaitForSeconds(secsWait);
        }
    }

    public IEnumerator ArmAnimation(string armType)
    {
        transform.GetChild(0).eulerAngles = Vector3.zero;
        transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = gameManager.tiles[StackBar.stackBarController.StackBarGrid[StackBar.stackBarController.idx]];
        transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().flipY = false;

        if (armType == "sword")
        {
            if (Vector2.Distance(transform.position, gameManager.mouseCurrentPosition) < 2.5f) PROJECTILE_SwordParticle.StaticSpawn(gameManager.mouseCurrentPosition, gameManager.armUsingDamageDeal, GetComponent<EntityCommonScript>());
            transform.GetChild(0).GetChild(0).eulerAngles = new Vector3(0, 0, 45);
            float armRotation = 180 / 10 * ManagingFunctions.ParseBoolToInt(GetComponent<SpriteRenderer>().flipX, false);
            for (int i = 0;i < 10; i++)
            {
                transform.GetChild(0).eulerAngles = new Vector3(0, 0, transform.GetChild(0).eulerAngles.z + armRotation);
                yield return new WaitForSeconds(0.016f);
            }
        }
        if(armType == "bow")
        {
            transform.GetChild(0).GetChild(0).eulerAngles = new Vector3(0, 0, -45);
            transform.GetChild(0).eulerAngles = new Vector3(0, 0, ManagingFunctions.PointToPivotUp(Vector2.zero, gameManager.mouseCurrentPosition - transform.position));

            yield return new WaitForSeconds(0.1f);
        }

        if (armType == "tool")
        {
            if (!GetComponent<SpriteRenderer>().flipX)
            {
                transform.GetChild(0).GetChild(0).eulerAngles = new Vector3(0, 0, 135);
                transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().flipY = true;
            }
            else
            {
                transform.GetChild(0).GetChild(0).eulerAngles = new Vector3(0, 0, 45);
                
            }
            
            float armRotation = 180 / 12 * ManagingFunctions.ParseBoolToInt(GetComponent<SpriteRenderer>().flipX, false);
            for (int i = 0; i < 12; i++)
            {
                transform.GetChild(0).eulerAngles = new Vector3(0, 0, transform.GetChild(0).eulerAngles.z + armRotation);
                yield return new WaitForSeconds(0.016f);
            }
        }
        transform.GetChild(0).eulerAngles = Vector3.zero;
        transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = gameManager.tiles[0];
        transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().flipY = false;
    }

}
