using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamager
{

    [SerializeField] float MaxSpeed;
    [SerializeField] float AccelerationInGround;
    [SerializeField] float AccelerationInAir;
    [SerializeField] float JumpForce;
    [SerializeField] HealthBarController healthBar;
    [SerializeField] GameObject particle;
    [SerializeField] LayerMask solidTiles;
    [SerializeField] LayerMask tileLayer;
    [SerializeField] LayerMask nodeLayer;
    [SerializeField] public DeathScreenController deathScreenController;
    Animator animations;
    Rigidbody2D rb2D;
    public EntityCommonScript entityScript;
    public CameraController mainCamera;
    MainSoundController soundController;
    bool spawned = false;
    public float damagedCooldown = 0.5f;
    public float handOffset = 0.062f;
    public float regenTime = 0f;
    public float drownTime = 5f;
    public bool killing = false;
    public bool alive = true;
    public bool onControl = true;
    public bool isMain = true;
    public int HP;
    public int MaxHP;
    GameManager gameManager;
    public float falling = 0f;
    public int skin = 0;
    public int FlyTime
    {
        get
        {
            return flyTime;
        }

        set
        {
            flyTime = value;

            int val = Mathf.Clamp((flyTime - 300) * -1, 0, 300);
            HealthBarManager.self.UpdateHealthBar(transform, val, 300, Vector2.up);
        }
    }
    public int flyTime = 0;
    public bool flyMode = false;
    public float wingTime = 0f;
    public float armCooldown = 0f;

    [SerializeField] AudioClip hitSound;
    [SerializeField] AudioClip regenerationSound;
    [SerializeField] AudioClip deathSound;
    [SerializeField] AudioClip jetpack;
    [SerializeField] AudioClip swing;

    public Transform accesories;

    EndPointNode endPointNode;

    void Start() //obtener referencias
    {
        rb2D = GetComponent<Rigidbody2D>();
        animations = GetComponent<Animator>();
        soundController = GameObject.Find("Audio").GetComponent<MainSoundController>();
        gameManager = GameManager.gameManagerReference;
        mainCamera = Camera.main.GetComponent<CameraController>();
        endPointNode = new EndPointNode
        {
            position = transform.position,
            index = new Vector3Int(-1, -transform.GetSiblingIndex(), 0)
        };
        endPointNode = (EndPointNode)NodeManager.self.RegisterNode(endPointNode);
    }

    void Update()
    {
        if (isMain)
        {
            if (spawned)
            {
                if (alive && gameManager.InGame)
                {
                    transform.GetChild(2).GetComponent<SpriteRenderer>().flipX = GetComponent<SpriteRenderer>().flipX;
                    if (onControl)
                        PlayerControl();
                    LecterAI();
                    mainCamera.transform.eulerAngles = Mathf.LerpAngle(mainCamera.transform.eulerAngles.z, 0, 10f * Time.deltaTime) * Vector3.forward;

                    List<string> list = new List<string>(ManagingFunctions.ConvertIntToStringArray(gameManager.equipedArmor));
                    for (int i = 0; i < accesories.childCount; i++)
                    {
                        Transform child = accesories.GetChild(i);
                        child.gameObject.SetActive(list.Contains(child.name));
                    }
                }

                if (!alive && !killing && gameManager.InGame && !CommandController.commandController.showcaseMode)
                {
                    mainCamera.transform.eulerAngles += Vector3.forward * Time.deltaTime;
                    mainCamera.GetComponent<Camera>().orthographicSize = Mathf.Clamp(mainCamera.GetComponent<Camera>().orthographicSize - 0.05f * Time.deltaTime, 2f, 5f);
                }

                accesories.gameObject.SetActive(alive);
                accesories.localScale = new Vector3(GetComponent<SpriteRenderer>().flipX ? -1f : 1f, 1f, 1f);
            }
            else
            {
                transform.position = new Vector2(0, 0);
            }

            float y = transform.position.y;
            if (entityScript.entityStates.Contains(EntityState.Drowning))
            {
                mainCamera.currentBackground = "Water";
            }
            else if (y < gameManager.WorldHeight * 0.1f)
            {
                mainCamera.currentBackground = "Lava";
            }
            else if (y < gameManager.WorldHeight * 0.5f)
            {
                mainCamera.currentBackground = "Rock";
                gameManager.PlayOST("cave");
            }
            else
            {
                mainCamera.currentBackground = "Air";
                gameManager.PlayOST("surface");
            }
        }
        else
        {
            transform.GetChild(2).GetComponent<SpriteRenderer>().flipX = GetComponent<SpriteRenderer>().flipX;
        }
    }

    private void LateUpdate()
    {
        if (skin != 0)
        {
            try
            {
                GetComponent<SpriteRenderer>().sprite = gameManager.playerSkins[skin].skin[System.Array.IndexOf(gameManager.playerSkins[0].skin, GetComponent<SpriteRenderer>().sprite)];
                transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = gameManager.playerSkins[skin].skin[System.Array.IndexOf(gameManager.playerSkins[0].skin, transform.GetChild(2).GetComponent<SpriteRenderer>().sprite)];
            }
            catch
            {

            }
        }
    }

    public void Hit(int damage, EntityCommonScript procedence, bool ignoreImunity = false, float knockback = 1f, bool penetrate = false)
    {
        if (procedence.EntityFamily != "yellow")
        {
            LoseHp(damage, procedence, ignoreImunity, knockback, penetrate);
        }
    }

    public void Respawn(float x, float y)
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
        entityScript = GetComponent<EntityCommonScript>();
        transform.GetChild(1).localScale = new Vector3(0, 1, 1);
        transform.GetChild(2).localScale = new Vector3(0, 1, 1);
        rb2D.bodyType = RigidbodyType2D.Dynamic;
        entityScript.entityStates = new List<EntityState>();
        transform.position = new Vector2(x, y);
        Camera.main.orthographicSize = 5f;
        Camera.main.GetComponent<CameraController>().focus = gameObject;
        rb2D.freezeRotation = true;
        transform.eulerAngles = Vector3.zero;
        spawned = true;//confirmación final
    }

    void PlayerControl()
    {
        CapsuleCollider2D collider2D = GetComponent<CapsuleCollider2D>();

        float offsetIDX = Mathf.Abs(Mathf.Sin(transform.eulerAngles.z * Mathf.Deg2Rad));

        //0.5 (1.05 / 2.1f)
        //1.5

        float raycastDistance = 0.98f;
        bool Grounded = false;

        float offset = Mathf.Lerp(0.26f, 0.8f, offsetIDX);

        Vector2 pos = new Vector2(transform.position.x, transform.position.y);

        if (Physics2D.Raycast(pos, Vector2.down, raycastDistance, solidTiles)) Grounded = true;
        if (Physics2D.Raycast(pos, Vector2.down, raycastDistance, solidTiles))
        {
            Debug.DrawRay(pos, Vector3.down * raycastDistance, Color.green);
        }
        else
        {
            Debug.DrawRay(pos, Vector3.down * raycastDistance, Color.red);
        }

        pos = new Vector2(transform.position.x + offset, pos.y);

        if (Physics2D.Raycast(pos, Vector2.down, raycastDistance, solidTiles)) Grounded = true;
        if (Physics2D.Raycast(pos, Vector2.down, raycastDistance, solidTiles))
        {
            Debug.DrawRay(pos, Vector3.down * raycastDistance, Color.green);
        }
        else
        {
            Debug.DrawRay(pos, Vector3.down * raycastDistance, Color.red);
        }

        pos = new Vector2(transform.position.x - offset, transform.position.y);

        if (Physics2D.Raycast(pos, Vector2.down, raycastDistance, solidTiles)) Grounded = true;
        if (Physics2D.Raycast(pos, Vector2.down, raycastDistance, solidTiles))
        {
            Debug.DrawRay(pos, Vector3.down * raycastDistance, Color.green);
        }
        else
        {
            Debug.DrawRay(pos, Vector3.down * raycastDistance, Color.red);
        }

        animations.SetBool("walking", false);

        if (FlyTime != 0)
            if (Grounded && !Input.GetKey(KeyCode.W))
                FlyTime = 0;

        if (!flyMode)
        {
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
        }

        if (GInput.GetKey(KeyCode.S))
        {
            gameManager.DropOn(transform.position - new Vector3(0f, 0.6f, 0f), 0.5f);


        }

        if (gameManager.equipedArmor[4] != 124 && gameManager.equipedArmor[4] != 123)
            if (GInput.GetKeyDown(KeyCode.W) && Grounded && rb2D.velocity.y >= 0)
            {
                rb2D.velocity = new Vector2(rb2D.velocity.x, JumpForce);
            }

        if (GInput.GetKey(KeyCode.W))
        {
            if (entityScript.entityStates.Contains(EntityState.Swimming))
            {
                rb2D.velocity = new Vector2(rb2D.velocity.x, 4f);
                entityScript.swimming = 6f;
            }
        }
        else if (GInput.GetKey(KeyCode.S))
        {
            if (entityScript.entityStates.Contains(EntityState.Swimming))
            {
                rb2D.velocity = new Vector2(rb2D.velocity.x, -5f);
                entityScript.swimming = 5f;
            }
        }
        else
        {
            entityScript.swimming = 0f;
        }

        if (gameManager.equipedArmor[4] == 123)
        {
            Transform backflip = accesories.Find("123").GetChild(3);
            ParticleSystem particles = accesories.Find("123").GetChild(4).GetComponent<ParticleSystem>();

            if (Grounded || entityScript.entityStates.Contains(EntityState.Swimming))
            {
                if (flyMode)
                {
                    flyMode = false;
                    falling = 0;

                    LoseHp((int)wingTime * 5, entityScript);

                    if (HP <= 0)
                    {
                        rb2D.freezeRotation = false;
                    }
                }
            }
            else if (!Grounded && GInput.GetKeyDown(KeyCode.W))
            {
                flyMode = true;
                wingTime = 0f;
                if (GInput.GetKey(KeyCode.A))
                    transform.eulerAngles = Vector3.forward * 90;
                if (GInput.GetKey(KeyCode.D))
                    transform.eulerAngles = Vector3.forward * -90;
            }

            if (!flyMode && GInput.GetKey(KeyCode.W) && FlyTime < 300)
            {
                rb2D.AddForce(new Vector2(0, 40));
                accesories.Find("123").GetChild(0).gameObject.SetActive(true);
                mainCamera.transform.eulerAngles = Random.Range(-1.5f, 1.5f) * Vector3.forward;
                if (FlyTime % 8 == 0)
                {
                    gameManager.soundController.PlaySfxSound(jetpack);
                }
                if (gameManager.addedFrameThisFrame)
                    FlyTime++;
            }
            else
            {
                accesories.Find("123").GetChild(0).gameObject.SetActive(false);
            }

            if (!flyMode)
            {
                if (!particles.isStopped)
                    particles.Stop();
                backflip.localScale = new Vector2(Mathf.MoveTowards(backflip.localScale.x, 0, 2f * Time.deltaTime), 1f);
                transform.eulerAngles = Vector3.forward * Mathf.MoveTowardsAngle(transform.eulerAngles.z, 0, 720 * Time.deltaTime);
            }
            else
            {
                if (!particles.isPlaying)
                    particles.Play();

                ParticleSystem.MainModule main = particles.main;
                main.startSpeed = rb2D.velocity.magnitude;

                backflip.localScale = new Vector2(Mathf.MoveTowards(backflip.localScale.x, 1, 2f * Time.deltaTime), 1f);
                if (GInput.GetKey(KeyCode.A))
                    transform.eulerAngles = Vector3.forward * Mathf.MoveTowardsAngle(transform.eulerAngles.z, transform.eulerAngles.z + 90, 180 * Time.deltaTime);
                if (GInput.GetKey(KeyCode.D))
                    transform.eulerAngles = Vector3.forward * Mathf.MoveTowardsAngle(transform.eulerAngles.z, transform.eulerAngles.z - 90, 180 * Time.deltaTime);

                float sin = Mathf.Sin((transform.eulerAngles.z) * Mathf.Deg2Rad) * -1;
                float cos = Mathf.Cos((transform.eulerAngles.z) * Mathf.Deg2Rad);
                float maxVelocity = 16;
                wingTime -= cos * Time.deltaTime;
                wingTime = Mathf.Clamp(wingTime, 0, 1000f);
                Vector2 targetVelocity = new Vector2(sin * maxVelocity, cos * maxVelocity);
                rb2D.velocity = Vector2.Lerp(rb2D.velocity, targetVelocity, wingTime / 3f);
                GetComponent<SpriteRenderer>().flipX = sin < 0f;
            }

            if (transform.position.y > gameManager.WorldHeight)
            {
                gameManager.equipedArmor[4] = 0;
                ManagingFunctions.DropItem(123, transform.position, new Vector2(rb2D.velocity.x * 3, 0), 1, 4);
            }
        }
        else
        {
            flyMode = false;
            wingTime = 0f;
            transform.eulerAngles = Vector3.forward * Mathf.MoveTowardsAngle(transform.eulerAngles.z, 0, 720 * Time.deltaTime);
            accesories.Find("123").GetChild(3).localScale = Vector3.zero;

            if (gameManager.equipedArmor[4] == 124)
            {
                if (GInput.GetKey(KeyCode.W) && FlyTime < 300)
                {
                    rb2D.AddForce(new Vector2(0, 40));
                    accesories.Find("124").GetChild(0).gameObject.SetActive(true);
                    mainCamera.transform.eulerAngles = Random.Range(-1.5f, 1.5f) * Vector3.forward;
                    if (FlyTime % 8 == 0)
                    {
                        gameManager.soundController.PlaySfxSound(jetpack);
                    }
                    if (gameManager.addedFrameThisFrame)
                        FlyTime++;
                }
                else
                {
                    accesories.Find("124").GetChild(0).gameObject.SetActive(false);
                }

                if (transform.position.y > gameManager.WorldHeight)
                {
                    gameManager.equipedArmor[4] = 0;
                    ManagingFunctions.DropItem(124, transform.position, new Vector2(rb2D.velocity.x * 3, 0), 1, 4);
                }
            }
        }



        if (GInput.GetKey(KeyCode.Space))
        {
            entityScript.ladderVelocity = 6f;
        }
        else
        {
            entityScript.ladderVelocity = 0f;
        }




        if ((rb2D.velocity.y > 0.1f && GInput.GetKey(KeyCode.W)) || rb2D.velocity.y > 2f)
        {
            animations.SetBool("falling", false);
            animations.SetBool("jumping", true);
        }
        else if (rb2D.velocity.y < 0.1f && rb2D.velocity.y > -0.1f)
        {
            animations.SetBool("falling", false);
            animations.SetBool("jumping", false);
        }
        else
        {
            animations.SetBool("falling", true);
            animations.SetBool("jumping", false);
        }

        if (transform.position.y < -30f)
        {
            if (!killing) StartCoroutine(KillAllLives(1));
            killing = true;
        }

        if (GInput.GetKeyDown(KeyCode.Q) && !StackBar.stackBarController.InventoryDeployed)
        {
            PlayerRelativeDrop(StackBar.stackBarController.StackBarGrid[StackBar.stackBarController.idx], 1);
            StackBar.LoseItem();
        }

        if (GInput.GetMouseButtonDown(0) && gameManager.usingArm && armCooldown <= 0f && transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite == gameManager.tiles[0])
        {
            if (!gameManager.cancelPlacing)
            {
                StartCoroutine(ArmAnimation(gameManager.armUsing));
                armCooldown = gameManager.tileSize[StackBar.stackBarController.currentItem].z;
                healthBar.MaxCooldown = armCooldown;
                healthBar.cooldown = armCooldown;

                if (gameManager.armUsing == "bow")
                {
                    if (StackBar.Search(64, 1) != -1)
                    {
                        StackBar.LoseItem(StackBar.Search(64, 1));
                        Vector2 vector = gameManager.mouseCurrentPosition - transform.position;
                        vector.x = vector.x * -1;
                        PROJECTILE_Arrow.StaticSpawn(ManagingFunctions.PointToPivotUp(Vector2.zero, vector), transform.position, gameManager.ToolEfficency[StackBar.stackBarController.currentItem], entityScript);
                    }
                    else if (InventoryBar.Search(64, 1) != -1)
                    {
                        InventoryBar.LoseItem(InventoryBar.Search(64, 1));
                        Vector2 vector = gameManager.mouseCurrentPosition - transform.position;
                        vector.x = vector.x * -1;
                        PROJECTILE_Arrow.StaticSpawn(ManagingFunctions.PointToPivotUp(Vector2.zero, vector), transform.position, gameManager.ToolEfficency[StackBar.stackBarController.currentItem], entityScript);
                    }
                }
                else if (gameManager.armUsing == "plasmabomb")
                {
                    DestroyerBomb bomb = Instantiate(gameManager.ProjectilesGameObject[(int)Projectiles.PlasmaBomb], transform.position, Quaternion.identity).GetComponent<DestroyerBomb>();
                    bomb.destroyer = entityScript;
                    bomb.transform.parent = GameManager.gameManagerReference.entitiesContainer.transform;
                    bomb.GetComponent<Rigidbody2D>().velocity = Vector2.ClampMagnitude((gameManager.mouseCurrentPosition - transform.position) * 10, 20);
                    StackBar.LoseItem();
                    bomb.makeBoom = false;
                }
            }
        }
        else if (armCooldown > 0f)
        {
            armCooldown -= Time.deltaTime;
            healthBar.cooldown = Mathf.Clamp(armCooldown, 0, 9999f);
        }

        if (Grounded && alive)
        {
            if (Mathf.Round(falling) >= 5f)
            {
                if (!entityScript.entityStates.Contains(EntityState.Swimming))
                {
                    int HpLose = Mathf.RoundToInt(falling);
                    LoseHp(Mathf.CeilToInt((float)HpLose / (int)gameManager.gameDifficulty), entityScript, true, 0f, true);
                    falling = 0;

                    if (HP <= 0)
                    {
                        deathScreenController.InstaKill();
                    }
                }
            }
        }

        if(!gameManager.cancelPlacing)
        if (GInput.GetMouseButtonDown(0))
            if (gameManager.usingTool)
                if (gameManager.toolUsing == "nodeConnector")
                    UseToolAnim();

        if (rb2D.velocity.y < 0)
        {
            falling -= rb2D.velocity.y * Time.deltaTime;
        }
        else falling = 0f;

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
            regenTime = 2f * (int)gameManager.gameDifficulty;
            if (HP < MaxHP) LoseHp(-1, entityScript);
            gameManager.soundController.PlaySfxSound(regenerationSound);
        }
        else
        {
            if (!entityScript.entityStates.Contains(EntityState.Drowning) && drownTime >= 6f && HP != MaxHP)
                regenTime -= Time.deltaTime;
        }

        drownTime = Mathf.Clamp(drownTime, 0, 6);
        if (entityScript.entityStates.Contains(EntityState.Drowning))
        {
            drownTime -= Time.deltaTime;

            if (drownTime <= 0)
            {
                LoseHp(1, entityScript, true, 0, true);
                drownTime = 1f;
            }
        }
        else
        {
            drownTime += Time.deltaTime;
        }

        for (int i = 0; i < 3; i++)
        {
            transform.GetChild(1).GetChild(i).GetComponent<SpriteRenderer>().color = gameManager.rawColor[gameManager.equipedArmor[i]];
            transform.GetChild(1).GetChild(i).GetComponent<SpriteRenderer>().flipX = GetComponent<SpriteRenderer>().flipX;
        }

        transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().color = gameManager.rawColor[gameManager.equipedArmor[1]];
        transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<SpriteRenderer>().flipX = GetComponent<SpriteRenderer>().flipX;

        transform.GetChild(2).GetComponent<SpriteRenderer>().flipX = GetComponent<SpriteRenderer>().flipX;
    }

    void LecterAI()
    {
        if (!entityScript.entityStates.Contains(EntityState.FireResistance))
            if (entityScript.entityStates.Contains(EntityState.Burning))
            {
                transform.GetChild(3).GetComponent<SpriteRenderer>().enabled = true;
                LoseHp(5, entityScript, false, 0, true);
            }
            else if (entityScript.entityStates.Contains(EntityState.OnFire))
            {
                transform.GetChild(3).GetComponent<SpriteRenderer>().enabled = true;
                LoseHp(1, entityScript, false, 0, true);
            }
            else
            {
                transform.GetChild(3).GetComponent<SpriteRenderer>().enabled = false;
            }
        else
        {
            transform.GetChild(3).GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    public void UseToolAnim()
    {
        if (gameManager.usingTool)
        {
            StartCoroutine(ArmAnimation("tool"));
        }
    }

    public void PlayerRelativeDrop(int item, int amount)
    {
        ManagingFunctions.DropItem(item, transform.position, new Vector2(5f * ManagingFunctions.ParseBoolToInt(!GetComponent<SpriteRenderer>().flipX) + rb2D.velocity.x * 1.5f, 5f + rb2D.velocity.y), amount: amount, imunityGrab: 3);
    }

    public void LoseHp(int hpLost, EntityCommonScript procedence, bool ignoreImunity = false, float knockback = 1f, bool penetrate = false)
    {
        if (gameManager.InGame && alive)
        {
            if (hpLost > 0)
            {
                hpLost *= (int)gameManager.gameDifficulty;
                soundController.PlaySfxSound(SoundName.damage);

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
                if (hpLost > 0) gameManager.soundController.PlaySfxSound(hitSound);
                healthBar.SetHealth(HP);

                if (HP < 1)
                {
                    healthBar.SetHealth(0);

                    if (knockback != 0f)
                        if (procedence.transform.position.x < transform.position.x)
                        {
                            rb2D.velocity = new Vector2(9f * knockback, JumpForce * 0.6f * knockback);
                        }
                        else
                        {
                            rb2D.velocity = new Vector2(-9f * knockback, JumpForce * 0.6f * knockback);
                        }

                    StartCoroutine(Kill(procedence));
                }
                else if (hpLost > 0)
                {
                    mainCamera.Turn();

                    if (knockback != 0f)
                        if (procedence.transform.position.x < transform.position.x)
                        {
                            rb2D.velocity = new Vector2(9f * knockback, JumpForce * 0.6f * knockback);
                        }
                        else
                        {
                            rb2D.velocity = new Vector2(-9f * knockback, JumpForce * 0.6f * knockback);
                        }
                    damagedCooldown = 0.6f;
                    regenTime = 3.5f * (int)gameManager.gameDifficulty;
                }
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!alive && killing)
        {
            if (collision.collider.gameObject.layer == 8 && collision.collider.gameObject.GetComponent<PlatformEffector2D>() == null)
                DeathFase();
        }
    }

    IEnumerator Kill(EntityCommonScript procedence)
    {
        gameManager.soundController.PlaySfxSound(deathSound);
        alive = false;
        killing = true;
        entityScript.ladderVelocity = 0f;
        deathScreenController.StartDeath(procedence);
        animations.SetBool("killed", true);
        Time.timeScale = 0.5f;
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

        for (int i = 0; i < gameManager.equipedArmor.Length; i++)
        {
            if (gameManager.equipedArmor[i] > 0)
            {
                ManagingFunctions.DropItem(gameManager.equipedArmor[i], transform.position + Vector3.up, Vector2.right * (i - 1));
                gameManager.equipedArmor[i] = 0;
            }
        }

        if ((int)gameManager.gameDifficulty > 1)
        {
            for (int i = 0; i < StackBar.stackBarController.StackBarGrid.Length; i++)
            {
                int tile = StackBar.stackBarController.StackBarGrid[i];
                if (tile > 0)
                {
                    if (gameManager.tileType[tile] != "tool" && tile != 16)
                    {
                        ManagingFunctions.DropItem(tile, transform.position + Vector3.up, Vector2.right * Random.Range(-5f, 5f) + Vector2.up * 2, StackBar.stackBarController.StackItemAmount[i]);
                        StackBar.AsignNewStack(i, 0, 0);
                    }
                }
            }

            for (int i = 0; i < InventoryBar.inventoryBarController.InventoryBarGrid.Length; i++)
            {
                int tile = InventoryBar.inventoryBarController.InventoryBarGrid[i];
                if (tile > 0)
                {
                    if (gameManager.tileType[tile] != "tool" && tile != 16)
                    {
                        ManagingFunctions.DropItem(tile, transform.position + Vector3.up, Vector2.right * Random.Range(-5f, 5f) + Vector2.up * 2, InventoryBar.inventoryBarController.InventoryItemAmount[i]);
                        InventoryBar.AsignNewStack(i, 0, 0);
                    }
                }
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
            LoseHp(liveLost, entityScript, true);
            yield return new WaitForSeconds(secsWait);
        }
    }

    public IEnumerator ArmAnimation(string armType)
    {
        if (transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite == gameManager.tiles[0])
        {
            int tile = StackBar.stackBarController.currentItem;
            Vector4 size = gameManager.tileSize[tile];
            transform.GetChild(0).eulerAngles = Vector3.zero;
            transform.GetChild(0).localScale = new Vector3(size.x * 0.7f, size.y * 0.7f, 1);
            transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = gameManager.tiles[tile];
            transform.GetChild(0).GetChild(0).localPosition = new Vector2(0, 1.1f);
            transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TrailRenderer>().Clear();
            transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TrailRenderer>().emitting = false;
            transform.GetChild(0).GetChild(0).GetChild(0).localPosition = new Vector2(0.435f, GetComponent<SpriteRenderer>().flipX ? -0.435f : 0.435f);

            //1.2

            if (armType == "sword")
            {
                gameManager.soundController.PlaySfxSound(swing);
                transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TrailRenderer>().emitting = true;
                if (GetComponent<SpriteRenderer>().flipX)
                {
                    transform.GetChild(0).GetChild(0).eulerAngles = new Vector3(0, 0, 135);
                    transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().flipY = true;
                }
                else
                {
                    transform.GetChild(0).GetChild(0).eulerAngles = new Vector3(0, 0, 45);
                    transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().flipY = false;
                }

                PROJECTILE_SwordParticle swordParticle = (PROJECTILE_SwordParticle)PROJECTILE_SwordParticle.StaticSpawn(gameManager.mouseCurrentPosition, gameManager.armUsingDamageDeal, GetComponent<EntityCommonScript>());
                float armRotation = 180 / 10 * ManagingFunctions.ParseBoolToInt(GetComponent<SpriteRenderer>().flipX);
                for (int i = 0; i < 10; i++)
                {
                    transform.GetChild(0).eulerAngles = new Vector3(0, 0, transform.GetChild(0).eulerAngles.z + armRotation);
                    if (swordParticle != null)
                    {
                        swordParticle.transform.position = transform.GetChild(0).GetChild(0).GetChild(0).position;
                    }
                    yield return new WaitForSeconds(0.016f);
                }
                if (gameManager.tileSize[StackBar.stackBarController.currentItem].z > 0.25f)
                {
                    yield return new WaitForSeconds(0.1f);

                    if (swordParticle == null)
                        swordParticle = (PROJECTILE_SwordParticle)PROJECTILE_SwordParticle.StaticSpawn(gameManager.mouseCurrentPosition, gameManager.armUsingDamageDeal, GetComponent<EntityCommonScript>());

                    for (int i = 0; i < 20; i++)
                    {
                        transform.GetChild(0).eulerAngles = new Vector3(0, 0, transform.GetChild(0).eulerAngles.z - armRotation * 0.5f);
                        if (swordParticle != null)
                        {
                            swordParticle.transform.position = transform.GetChild(0).GetChild(0).GetChild(0).position;
                        }
                        yield return new WaitForSeconds(0.016f);
                    }
                }

                if (swordParticle != null)
                    swordParticle.Despawn();
            }

            if (armType == "bow")
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
                    transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().flipY = false;
                }

                if (gameManager.toolUsing != "nodeConnector")
                {
                    float armRotation = 180 / 12 * ManagingFunctions.ParseBoolToInt(GetComponent<SpriteRenderer>().flipX);
                    for (int i = 0; i < 12; i++)
                    {
                        transform.GetChild(0).eulerAngles = new Vector3(0, 0, transform.GetChild(0).eulerAngles.z + armRotation);
                        yield return new WaitForSeconds(0.016f);
                    }
                }
                else
                {
                    transform.GetChild(0).eulerAngles = Vector3.forward * ManagingFunctions.PointToPivotUp(transform.position, gameManager.mouseCurrentPosition);
                    transform.GetChild(0).GetChild(0).GetChild(0).localPosition = new Vector2(0.435f, GetComponent<SpriteRenderer>().flipX ? 0.435f : -0.435f);
                    transform.GetChild(0).GetChild(0).localPosition = Vector2.zero;

                    Collider2D raycastHit;
                    int loopback = 0;
                    bool allowedExit = false;

                    do
                    {
                        raycastHit = null;
                        for (int i = 0; loopback < 20 && i < 99; i++)
                        {
                            raycastHit = Physics2D.OverlapCircle(transform.GetChild(0).GetChild(0).GetChild(0).position, 0.1f, nodeLayer);
                            if (raycastHit != null)
                            {
                                i = 9999;
                            }


                            transform.GetChild(0).GetChild(0).localPosition += Vector3.up * 0.05f;
                            endPointNode.position = transform.GetChild(0).GetChild(0).GetChild(0).position;
                            loopback++;

                            yield return new WaitForSeconds(0.016f);
                        }

                        if (raycastHit != null)
                        {
                            NodeInstance nodeInstance = raycastHit.GetComponentInParent<NodeInstance>();
                            Node node = nodeInstance.nodes[System.Array.IndexOf(nodeInstance.nodeObjects, raycastHit.gameObject)];
                            Node previousNode = null;
                            if (endPointNode.connections.Count > 0)
                                previousNode = endPointNode.connections[0];

                            if (previousNode != node)
                                if (previousNode != null)
                                {
                                    if (node.GetType() != NodeManager.self.sourceNode)
                                    {
                                        previousNode.RemoveConnectionRecursive(endPointNode);
                                        previousNode.AddConnectionRecursive(node);
                                        if (node.GetType() != NodeManager.self.endPointNode)
                                            endPointNode.AddConnectionRecursive(node);
                                    }
                                }
                                else
                                {
                                    if ((node.GetType() == NodeManager.self.sourceNode || node.connections.Count > 0) && node.GetType() != NodeManager.self.endPointNode)
                                        node.AddConnectionRecursive(endPointNode);
                                }

                        }

                        if(loopback > 10)
                        {
                            for (int i = 0; loopback >= 10; i++)
                            {
                                transform.GetChild(0).GetChild(0).localPosition -= Vector3.up * 0.05f;
                                endPointNode.position = transform.GetChild(0).GetChild(0).GetChild(0).position;
                                loopback--;
                                yield return new WaitForSeconds(0.016f);
                            }
                        }
                        else if(loopback < 10)
                        {
                            for (int i = 0; loopback < 10; i++)
                            {
                                transform.GetChild(0).GetChild(0).localPosition += Vector3.up * 0.05f;
                                endPointNode.position = transform.GetChild(0).GetChild(0).GetChild(0).position;
                                loopback++;
                                yield return new WaitForSeconds(0.016f);
                            }
                        }

                        if (endPointNode.connections.Count > 0)
                            while ((!GInput.GetMouseButton(0) && !gameManager.cancelPlacing) && gameManager.usingTool && gameManager.toolUsing == "nodeConnector" && !allowedExit)
                            {
                                transform.GetChild(0).eulerAngles = Vector3.forward * Mathf.MoveTowardsAngle(transform.GetChild(0).eulerAngles.z, ManagingFunctions.PointToPivotUp(transform.position, gameManager.mouseCurrentPosition), 180f * Time.deltaTime);
                                endPointNode.position = transform.GetChild(0).GetChild(0).GetChild(0).position;
                                if (Vector2.Distance(endPointNode.connections[0].position, endPointNode.position) > 15) allowedExit = true;

                                yield return new WaitForEndOfFrame();
                            }

                    } while (endPointNode.connections.Count > 0 && gameManager.usingTool && gameManager.toolUsing == "nodeConnector" && !allowedExit);

                    while(endPointNode.connections.Count > 0)
                    {
                        endPointNode.connections[0].RemoveConnectionRecursive(endPointNode);
                    }

                    for (int i = 0; loopback >= 0; i++)
                    {
                        transform.GetChild(0).GetChild(0).localPosition -= Vector3.up * 0.05f;
                        endPointNode.position = transform.GetChild(0).GetChild(0).GetChild(0).position;
                        loopback--;
                        yield return new WaitForSeconds(0.016f);
                    }

                    transform.GetChild(0).localPosition = Vector2.zero;
                }
            }

            transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TrailRenderer>().emitting = false;
            transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = gameManager.tiles[0];
            transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().flipY = false;
        }
    }
}
