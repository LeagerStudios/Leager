using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ENTITY_KrotekController : EntityBase, ISoundHearer, IDamager, ICollisionNotifier
{
    public GameObject leftAnt;
    public GameObject rightAnt;
    public GameObject head;
    public GameObject body;

    public GameObject leftLeg;
    public GameObject rightLeg;
    public GameObject leftKnee;
    public GameObject rightKnee;
    public GameObject leftFoot;
    public GameObject rightFoot;
    public GameObject footCollider;

    public GameObject leftArmHumerus;
    public GameObject leftRadius;
    public GameObject leftHand;

    public GameObject rightArmHumerus;
    public GameObject rightRadius;
    public GameObject rightHand;

    public AudioSource sound;
    public AudioClip[] sounds;

    [SerializeField] Animator[] fingers;
    public Animator animator;
    float HpMax = 3600f;
    float HP = 3600f;

    public override int Hp
    {
        get { return Mathf.FloorToInt(HP); }

        set
        {
            if (value > HpMax)
            {
                HP = HpMax;
            }
            else if (value <= 0f)
            {
                HP = 0;
                Kill(null);
            }
            else
            {
                HP = value;
            }
        }
    }

    public override EntityCommonScript EntityCommonScript => null;

    float leftLegPosition = -0.445f;
    float leftKneePosition = -0.85f;
    float rightLegPosition = 0.445f;
    float rightKneePosition = 0.85f;
    Vector2 leftFootStandartPos;
    Vector2 rightFootStandartPos;

    public float stress = 3;
    public float framesToTakeStep = 60;
    bool started = false;
    bool killedPlayer = false;
    public List<Vector2> rute = new List<Vector2>();
    bool followingPlayer = false;
    float ruteStatus = 0f;
    int ruteSectionIdx = 0;

    [SerializeField] SpriteRenderer face;
    [SerializeField] SpriteRenderer eyesLight;
    [SerializeField] Sprite[] headDirections;
    [SerializeField] Sprite[] headLights;
    public string direction = "front";
    bool rotatingFace = false;

    //for AI
    bool hasIntercepted = false;
    bool walking = false;
    bool grounded = false;
    bool groundedTwice = false;
    bool leftGrounded = false;
    bool rightGrounded = false;
    bool leftCollision = false;
    bool rightCollision = false;
    [SerializeField] LayerMask blockMask;

    public float[] armsRotationOnStress = { 100f, 90f, 75f };
    public float[] armsRotationOnCalm = { 5f, 10f, 20f };

    public float walkSpeed = 0;
    public float fallSpeed;


    void Update()
    {
        if (GameManager.gameManagerReference.InGame && !killedPlayer) AiFrame();
        if (killedPlayer)
        {
            direction = "front";
            if(GameManager.gameManagerReference.player.alive)
            GameManager.gameManagerReference.player.GetComponent<Rigidbody2D>().MovePosition(Vector2.Lerp(GameManager.gameManagerReference.player.transform.position, head.transform.position, 0.1f));
        }
    }

    public override string[] GenerateArgs()
    {
        return null;
    }

    void LateUpdate()
    {
        SetPositions();
        CheckGrounded(true);
    }

    public void Hit(int damageDeal, EntityCommonScript procedence, bool ignoreImunity = false, float knockback = 1f, bool penetrate = false)
    {
        Hp = Hp - damageDeal;
    }


    public override void AiFrame()
    {
        if (!followingPlayer)
        {
            ruteStatus += Time.deltaTime;
            if (ruteStatus > 10f || Mathf.Abs(transform.position.x - rute[ruteSectionIdx].x) < 1)
            {
                if (ruteSectionIdx  >= rute.Count - 1)
                    ruteSectionIdx = 0;
                else
                    ruteSectionIdx++;

                ruteStatus = 0f;
            }
            
            if (transform.position.x < rute[ruteSectionIdx].x)
            {
                walkSpeed = 4f;
            }
            else
            {
                walkSpeed = -4;
            }

            if(transform.position.y + leftFootStandartPos.y - 1 > rute[ruteSectionIdx].y)
            {
                GameManager.gameManagerReference.DropOn(transform.position + (Vector3)leftFootStandartPos, 0.5f);
                GameManager.gameManagerReference.DropOn(transform.position + (Vector3)rightFootStandartPos, 0.5f);
            }

            if (transform.position.y + leftFootStandartPos.y + 1 < rute[ruteSectionIdx].y && grounded)
            {
                fallSpeed = 13f;
                transform.Translate(Vector2.up * Time.deltaTime * fallSpeed);
            }
        }
        

        if (walkSpeed > 0.1f)
        {
            direction = "right";
        }
        else if (walkSpeed < -0.1f)
        {
            direction = "left";
        }
        else
        {
            direction = "front";
        }

        if (direction == "front")
        {
            face.sprite = headDirections[0];
            eyesLight.sprite = headLights[0];

            leftAnt.transform.parent.transform.eulerAngles = Vector3.zero;
            leftAnt.transform.parent.transform.localPosition = Vector3.forward;
        }
        if (direction == "back")
        {
            face.sprite = headDirections[1];
            eyesLight.sprite = headLights[1];
            leftAnt.transform.parent.transform.eulerAngles = new Vector3(0, 180, 0);
            leftAnt.transform.parent.transform.localPosition = Vector3.forward;
        }
        if (direction == "left")
        {
            face.sprite = headDirections[2];
            eyesLight.sprite = headLights[2];
            leftAnt.transform.parent.transform.eulerAngles = new Vector3(0, 40, 0);
            leftAnt.transform.parent.transform.localPosition = Vector3.zero;
        }
        if (direction == "right")
        {
            face.sprite = headDirections[3];
            eyesLight.sprite = headLights[3];
            leftAnt.transform.parent.transform.eulerAngles = new Vector3(0, -40, 0);
            leftAnt.transform.parent.transform.localPosition = Vector3.zero;
        }

        eyesLight.color = new Color(1f, 1f, 1f, 1f - GameManager.gameManagerReference.dayLuminosity);

        if (Mathf.Abs(walkSpeed) > 0.1f)
            transform.Translate(walkSpeed * Vector2.right * Time.deltaTime);
        animator.SetFloat("walk", walkSpeed);

        CheckGrounded(true);

        if (!grounded)
        {
            fallSpeed += Physics2D.gravity.y * Time.deltaTime;
            transform.Translate(Vector3.up * fallSpeed * Time.deltaTime);
        }
        else
        {
            fallSpeed = 0;
            float translated = 0f;
            while (translated < 0.1f && grounded)
            {
                transform.Translate(Vector3.up * Time.deltaTime);
                translated += 1f * Time.deltaTime;
                CheckGrounded(false);
            }
            transform.Translate(Vector3.down * Time.deltaTime);
        }


        if (direction == "right")
        {
            ExitCollision("right");
            ExitCollision("left");
        }
        else
        {
            ExitCollision("left");
            ExitCollision("right");
        }

        footCollider.transform.localPosition = Vector2.zero;
    }

    public void ExitCollision(string dir)
    {
        if(dir == "left")
        {
            if (leftCollision)
            {
                while (leftCollision)
                {
                    transform.Translate(Vector3.right * Time.deltaTime);
                    CheckGrounded(false);
                }
                transform.Translate(Vector3.left * Time.deltaTime);
            }
        }
        if(dir == "right")
        {
            if (rightCollision)
            {
                while (rightCollision)
                {
                    transform.Translate(Vector3.left * Time.deltaTime);
                    CheckGrounded(false);
                }
                transform.Translate(Vector3.right * Time.deltaTime);
            }
        }
    }

    public void OnHitEnter(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!killedPlayer)
            {
                sound.Stop();
                killedPlayer = true;
                PlaySound(2);
                animator.SetFloat("walk", 0);
                Invoke("KillPlayer", 0.6f);
            }
        }
    }

    void KillPlayer()
    {
        GameManager.gameManagerReference.player.LoseHp(1000, GetComponent<EntityCommonScript>(), true, 0);
        DeathScreenController.deathScreenController.InstaKill();
        Invoke("Despawn", 2f);
    }


    public void ReceivedSound(Vector2 soundOrigin, string ID)
    {
        if (soundOrigin.x < head.transform.position.x)
        {
            StartCoroutine(HearSound("left"));
        }
        else if (soundOrigin.x > head.transform.position.x)
        {
            StartCoroutine(HearSound("right"));
        }


        if (!rute.Contains(soundOrigin))
        {
            rute.Add(soundOrigin);
            ruteSectionIdx = rute.Count - 1;
        }

        if(rute.Count > 5)
        {
            rute.Remove(rute[0]);
            if (ruteSectionIdx > 4)
                ruteSectionIdx = 4;
        }
    }

    public void CheckGrounded(bool draw)
    {
        leftGrounded = false;
        rightGrounded = false;
        leftCollision = false;
        rightCollision = false;

        Vector3 leftFootPos = leftFootStandartPos;
        leftFootPos.Set(leftFoot.transform.localPosition.x, leftFootPos.y, leftFootPos.z);
        Vector3 rightFootPos = rightFootStandartPos;
        rightFootPos.Set(rightFoot.transform.localPosition.x, rightFootPos.y, rightFootPos.z);


        if (Physics2D.Raycast((transform.position + leftFootPos) + Vector3.up, Vector2.down, 1.15f, blockMask))
        {
            if (draw) Debug.DrawRay((transform.position + leftFootPos), Vector2.down * 0.15f, Color.green);
            leftGrounded = true;
        }
        else
        {
            if (draw) Debug.DrawRay((transform.position + leftFootPos), Vector2.down * 0.15f, Color.red);
        }

        if (Physics2D.Raycast((transform.position + rightFootPos) + Vector3.up, Vector2.down, 1.15f, blockMask))
        {
            if (draw) Debug.DrawRay((transform.position + rightFootPos), Vector2.down * 0.15f, Color.green);
            rightGrounded = true;
        }
        else
        {
            if (draw) Debug.DrawRay((transform.position + rightFootPos), Vector2.down * 0.15f, Color.red);
        }

        RaycastHit2D leftRay = Physics2D.Raycast(new Vector2(leftFoot.transform.position.x, leftLeg.transform.position.y + 1f) + Vector2.right, Vector2.left, 1.40f, blockMask);
        RaycastHit2D rightRay = Physics2D.Raycast(new Vector2(rightFoot.transform.position.x, rightLeg.transform.position.y + 1f) + Vector2.left, Vector2.right, 1.40f, blockMask);
        bool leftIsPlatform = false;
        bool rightIsPlatform = false;

        if (leftRay)
            if (leftRay.transform.parent != null)
            {
                if (leftRay.transform.parent.GetComponent<ChunkController>() != null)
                {
                    ChunkController chunkController = leftRay.transform.parent.GetComponent<ChunkController>();
                    leftIsPlatform = GameManager.gameManagerReference.TileCollisionType[chunkController.TileGrid[System.Array.IndexOf(chunkController.TileObject, leftRay.transform.gameObject)]] == 2;
                }
            }

        if (rightRay)
            if (rightRay.transform.parent != null)
            {
                if (rightRay.transform.parent.GetComponent<ChunkController>() != null)
                {
                    ChunkController chunkController = rightRay.transform.parent.GetComponent<ChunkController>();
                    rightIsPlatform = GameManager.gameManagerReference.TileCollisionType[chunkController.TileGrid[System.Array.IndexOf(chunkController.TileObject, rightRay.transform.gameObject)]] == 2;
                }
            }

        if (rightFoot.transform.position.x > leftFoot.transform.position.x)
        {
            if (leftRay && !leftIsPlatform)
            {
                if (draw) Debug.DrawRay(new Vector2(leftFoot.transform.position.x, leftLeg.transform.position.y + 1f), Vector2.left * 0.40f, Color.green);
                leftCollision = true;
            }
            else
            {
                if (draw) Debug.DrawRay(new Vector2(leftFoot.transform.position.x, leftLeg.transform.position.y + 1f), Vector2.left * 0.40f, Color.red);
            }

            if (rightRay && !rightIsPlatform)
            {
                if (draw) Debug.DrawRay(new Vector2(rightFoot.transform.position.x, rightLeg.transform.position.y + 1f), Vector2.right * 0.40f, Color.green);
                rightCollision = true;
            }
            else
            {
                if (draw) Debug.DrawRay(new Vector2(rightFoot.transform.position.x, rightLeg.transform.position.y + 1f), Vector2.right * 0.40f, Color.red);
            }
        }
        else
        {
            if (rightRay && !rightIsPlatform)
            {
                if (draw) Debug.DrawRay(new Vector2(rightFoot.transform.position.x, rightLeg.transform.position.y + 1f), Vector2.left * 0.40f, Color.green);
                leftCollision = true;
            }
            else
            {
                if (draw) Debug.DrawRay(new Vector2(rightFoot.transform.position.x, rightLeg.transform.position.y + 1f), Vector2.left * 0.40f, Color.red);
            }

            if (leftRay && !leftIsPlatform)
            {
                if (draw) Debug.DrawRay(new Vector2(leftFoot.transform.position.x, leftLeg.transform.position.y + 1f), Vector2.right * 0.40f, Color.green);
                rightCollision = true;
            }
            else
            {
                if (draw) Debug.DrawRay(new Vector2(leftFoot.transform.position.x, leftLeg.transform.position.y + 1f), Vector2.right * 0.10f, Color.red);
            }
        }

        if (leftGrounded || rightGrounded)
        {
            grounded = true;
        }
        else
        {
            grounded = false;
        }
        if (leftGrounded && rightGrounded)
        {
            groundedTwice = true;
        }
        else
        {
            groundedTwice = false;
        }
    }

    public static EntityBase StaticSpawn(string[] args, Vector2 spawnPos)
    {
        return Instantiate(GameManager.gameManagerReference.EntitiesGameObject[(int)Entities.KrotekBoss], spawnPos, Quaternion.identity).GetComponent<ENTITY_KrotekController>().Spawn(args, spawnPos);
    }

    public override EntityBase Spawn(string[] args, Vector2 spawnPos)
    {
        transform.SetParent(GameManager.gameManagerReference.entitiesContainer.transform, true);
        leftFootStandartPos = leftFoot.transform.localPosition;
        rightFootStandartPos = rightFoot.transform.localPosition;

        //body.GetComponent<ObjectCollision>().target = this;
        leftKnee.GetComponent<ObjectCollision>().target = this;
        rightKnee.GetComponent<ObjectCollision>().target = this;
        leftHand.GetComponent<ObjectCollision>().target = this;
        rightHand.GetComponent<ObjectCollision>().target = this;

        face = head.GetComponent<SpriteRenderer>();
        leftAnt.GetComponent<Animator>().speed = Random.Range(0.7f, 1.5f);
        rightAnt.GetComponent<Animator>().speed = Random.Range(0.7f, 1.5f);
        animator.SetLayerWeight(1, 1);
        animator.SetLayerWeight(2, 1);

        foreach (Animator animator in fingers)
        {
            animator.speed = Random.Range(0.5f, 1.7f);
        }
        head.GetComponent<SoundHearer>().atached = this;
        head.GetComponent<DamagersCollision>().target = this;
        eyesLight.color = new Color(1f, 1f, 1f, 1f - GameManager.gameManagerReference.dayLuminosity);
        rute.Add(transform.position);
        rute.Add(GameManager.gameManagerReference.player.transform.position);

        started = true;
        return this;
    }

    public override void Despawn()
    {
        Destroy(gameObject);
    }

    public override void Kill(string[] args)
    {
        Despawn();
    }

    IEnumerator HearSound(string side)
    {
        if(side == "left")
        {
            leftAnt.GetComponent<Animator>().SetBool("hearedSound", true);
            yield return new WaitForSeconds(0.016f);
            leftAnt.GetComponent<Animator>().SetBool("hearedSound", false);
            if (!rotatingFace) StartCoroutine(LookTo("left"));
        }
        else
        {
            rightAnt.GetComponent<Animator>().SetBool("hearedSound", true);
            yield return new WaitForSeconds(0.016f);
            rightAnt.GetComponent<Animator>().SetBool("hearedSound", false);
            if (!rotatingFace) StartCoroutine(LookTo("right"));
        }
    }

    IEnumerator LookTo(string dir)
    {
        rotatingFace = true;
        while (direction != dir)
        {
            if (dir == "front")
            {
                if (direction == "back")
                {
                    if (Random.Range(0, 2) == 0)
                    {
                        direction = "left";
                    }
                    else
                    {
                        direction = "right";
                    }
                }
                else if (direction == "right" || direction == "left")
                {
                    direction = "front";
                }
            }
            if (dir == "back")
            {
                if (direction == "front")
                {
                    if (Random.Range(0, 2) == 0)
                    {
                        direction = "left";
                    }
                    else
                    {
                        direction = "right";
                    }
                }
                else if (direction == "right" || direction == "left")
                {
                    direction = "back";
                }
            }
            if (dir == "left")
            {
                if (direction == "right")
                {
                    direction = "front";
                }
                else if (direction == "front" || direction == "back")
                {
                    direction = "left";
                }
            }
            if (dir == "right")
            {
                if (direction == "left")
                {
                    direction = "front";
                }
                else if (direction == "front" || direction == "back")
                {
                    direction = "right";
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
        rotatingFace = false;

    }

    IEnumerator TakeStep(string dir)
    {
        walking = true;

        if (dir == "left")
        {
            rightKnee.GetComponent<Rigidbody2D>().simulated = false;
            for(int i = 0;i < framesToTakeStep / 2; i++)
            {
                rightKnee.transform.Translate(new Vector2(-(rightKneePosition * 3) /framesToTakeStep, 1f / framesToTakeStep));
                yield return new  WaitForSeconds(0.016f);
            }
            for (int i = 0; i < framesToTakeStep / 2; i++)
            {
                rightKnee.transform.Translate(new Vector2(-(rightKneePosition * 3) / framesToTakeStep, -1f / framesToTakeStep));
                
                yield return new WaitForSeconds(0.016f);
            }
            if (rightGrounded) PlaySound(0);
            rightKnee.GetComponent<Rigidbody2D>().simulated = true;

            leftKnee.GetComponent<Rigidbody2D>().simulated = false;
            for (int i = 0; i < framesToTakeStep / 2; i++)
            {
                leftKnee.transform.Translate(new Vector2(-(rightKneePosition * 3) / framesToTakeStep, 1f / framesToTakeStep));
                yield return new WaitForSeconds(0.016f);
            }
            for (int i = 0; i < framesToTakeStep / 2; i++)
            {
                leftKnee.transform.Translate(new Vector2(-(rightKneePosition * 3) / framesToTakeStep, -1f / framesToTakeStep));
                
                yield return new WaitForSeconds(0.016f);
            }
            if (leftGrounded) PlaySound(1);
            leftKnee.GetComponent<Rigidbody2D>().simulated = true;
        }
        if(dir == "right")
        {
            leftKnee.GetComponent<Rigidbody2D>().simulated = false;
            for (int i = 0; i < framesToTakeStep / 2; i++)
            {
                leftKnee.transform.Translate(new Vector2(rightKneePosition * 3 / framesToTakeStep, 2f / framesToTakeStep));
                yield return new WaitForSeconds(0.016f);
            }
            for (int i = 0; i < framesToTakeStep / 2; i++)
            {
                leftKnee.transform.Translate(new Vector2(rightKneePosition * 3 / framesToTakeStep, -2f / framesToTakeStep));
                
                yield return new WaitForSeconds(0.016f);
            }
            if (leftGrounded) PlaySound(0);
            leftKnee.GetComponent<Rigidbody2D>().simulated = true;

            rightKnee.GetComponent<Rigidbody2D>().simulated = false;
            for (int i = 0; i < framesToTakeStep / 2; i++)
            {
                rightKnee.transform.Translate(new Vector2(rightKneePosition * 3 / framesToTakeStep, 2f / framesToTakeStep));
                yield return new WaitForSeconds(0.016f);
            }
            for (int i = 0; i < framesToTakeStep / 2; i++)
            {
                rightKnee.transform.Translate(new Vector2(rightKneePosition * 3 / framesToTakeStep, -2f / framesToTakeStep));
                
                yield return new WaitForSeconds(0.016f);
            }
            if (rightGrounded) PlaySound(1);
            rightKnee.GetComponent<Rigidbody2D>().simulated = true;
        }
        yield return new WaitForSeconds(0.016f);
        walking = false;
    }

    void PlaySound(int idx)
    {
        sound.PlayOneShot(sounds[idx], ManagingFunctions.VolumeDistance(Vector2.Distance(transform.position, GameManager.gameManagerReference.player.transform.position), 32));
    }

    void SetPositions()
    {
        //if (false)
        //{
        //    float higestKnee = 0f;
        //    if (leftKnee.transform.position.y > rightKnee.transform.position.y)
        //    {
        //        higestKnee = leftKnee.transform.position.y;
        //    }
        //    else
        //    {
        //        higestKnee = rightKnee.transform.position.y;
        //    }
        //    //body.transform.position = Vector2.Lerp(body.transform.position, new Vector2(Mathf.Lerp(leftKnee.transform.position.x, rightKnee.transform.position.x, 0.5f), higestKnee + 2.2f), 0.4f);
        //}
        //else
        //{
        //    float lowestKnee = 0f;
        //    if (leftKnee.transform.position.y < rightKnee.transform.position.y)
        //    {
        //        lowestKnee = leftKnee.transform.position.y;
        //    }
        //    else
        //    {
        //        lowestKnee = rightKnee.transform.position.y;
        //    }
        //    body.transform.position = Vector2.Lerp(body.transform.position, new Vector2(body.transform.position.x, lowestKnee + 2.2f), 0.4f);
        //}

        Vector2 vector2 = new Vector2(0, 0);
        Vector3 vector3 = new Vector3(0, 0, 0);
        vector2.Set(body.transform.position.x, body.transform.position.y + 2.365f);
        head.transform.position = vector2;

        //vector2.Set(body.transform.position.x + -0.8f, body.transform.position.y + 1.5f);
        //leftArmHumerus.transform.position = vector2;

        //vector2.Set(body.transform.position.x + 0.8f, body.transform.position.y + 1.5f);
        //rightArmHumerus.transform.position = vector2;

        vector3.Set(1, Vector2.Distance(leftLeg.transform.position, leftKnee.transform.position), 1);
        vector2.Set(Mathf.Clamp(leftKnee.transform.position.x, body.transform.position.x + leftLegPosition, body.transform.position.x + rightLegPosition), body.transform.position.y - 1.5f);
        leftLeg.transform.localScale = vector3;
        leftLeg.transform.position = vector2;
        leftLeg.transform.localEulerAngles = Vector3.forward * ManagingFunctions.PointToPivotDown(leftLeg.transform.position, leftKnee.transform.position);
        vector2.Set(leftKnee.transform.position.x, leftKnee.transform.position.y - 0.955f);
        leftFoot.transform.position = vector2;

        vector3.Set(1, Vector2.Distance(rightLeg.transform.position, rightKnee.transform.position), 1);
        vector2.Set(Mathf.Clamp(rightKnee.transform.position.x, body.transform.position.x + leftLegPosition, body.transform.position.x + rightLegPosition), body.transform.position.y - 1.5f);
        rightLeg.transform.localScale = vector3;
        rightLeg.transform.position = vector2;
        rightLeg.transform.localEulerAngles = Vector3.forward * ManagingFunctions.PointToPivotDown(rightLeg.transform.position, rightKnee.transform.position);
        vector2.Set(rightKnee.transform.position.x, rightKnee.transform.position.y - 0.955f);
        rightFoot.transform.position = vector2;

        //if (!walking) GameManager.gameManagerReference.DropOn(leftFoot.transform.position, 0.5f);
        //if (!walking)GameManager.gameManagerReference.DropOn(rightFoot.transform.position, 0.5f);

        //SetArmsRotation();
    }
}
