using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaideonSpawner : MonoBehaviour
{
    bool activated = false;
    float cooldown = 0f;
    public int raideonLimit = 3;
    public LineRenderer lightning;
    public List<ENTITY_Raideon> raideons = new List<ENTITY_Raideon>();
    public AudioClip lightningSfx;

    void Start()
    {
        
    }

    void Update()
    {
        if (GameManager.gameManagerReference.InGame)
        {
            if (activated)
                if (!GameManager.gameManagerReference.isNetworkClient)
                    if (raideons.Count < raideonLimit && cooldown <= 0f)
                    {
                        AddOne();
                        cooldown = 1f;
                        Lightning();
                    }
                    else if (cooldown > 0f)
                    {
                        cooldown -= Time.deltaTime;
                    }
                    else
                    {
                        cooldown = 0f;
                    }

            if (!activated)
            {
                if (Vector2.Distance(GameManager.gameManagerReference.player.transform.position, transform.position) < 3)
                {
                    activated = true;
                }
            }
            else
            {
                if (Vector2.Distance(GameManager.gameManagerReference.player.transform.position, transform.position) > 30)
                {
                    activated = false;
                }
            }

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(cooldown, 0.0f), new GradientAlphaKey(cooldown, 1.0f) }
            );
            lightning.colorGradient = gradient;
        }
    }

    public void AddOne()
    {
        ENTITY_Raideon newRaideon = (ENTITY_Raideon)GameManager.gameManagerReference.SpawnEntity(Entities.Raideon, null, transform.position + Vector3.up);
        newRaideon.associatedSpawner = this;
        raideons.Add(newRaideon);
    }

    public void LoseOne(ENTITY_Raideon lose)
    {
        raideons.Remove(lose);
    }

    public void Lightning()
    {
        lightning.colorGradient.alphaKeys[0].alpha = 1;
        lightning.colorGradient.alphaKeys[1].alpha = 1;
        GameManager.gameManagerReference.soundController.PlaySfxSound(lightningSfx, ManagingFunctions.VolumeDistance(Vector2.Distance(GameManager.gameManagerReference.player.transform.position, transform.position), 70));

        float y = transform.position.y;
        List<Vector3> positions = new List<Vector3>();
        positions.Add(transform.position);
        y += Random.Range(3f, 7f);

        while (y < GameManager.gameManagerReference.WorldHeight + 8f)
        {
            positions.Add(new Vector3(transform.position.x + Random.Range(-4f, 4f), y));
            y += Random.Range(3f, 7f);
        }

        lightning.positionCount = positions.Count;
        lightning.SetPositions(positions.ToArray());
    }
}
