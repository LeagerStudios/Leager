using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BLOCK_NodeTowerT1 : MonoBehaviour
{
    public Transform upperPart;
    public Transform leftAntenna;
    public Transform rightAntenna;
    public Transform down;
    public Transform feet;

    public float scale = 0f;
    public bool deploying = true;
    public float speed = 0f;

    private void Start()
    {
        if(GameManager.gameManagerReference.TileCollisionType[GameManager.gameManagerReference.GetTileAt(ManagingFunctions.CreateIndex(Vector2Int.RoundToInt(transform.position)) - 1)] == 1)
        {
            deploying = false;
        }
    }

    void Update()
    {
        if (deploying)
        {
            speed -= Time.deltaTime * Physics2D.gravity.y;
            scale += speed * Time.deltaTime;

            Recalculate();

            if (GameManager.gameManagerReference.TileCollisionType[GameManager.gameManagerReference.GetTileAt(ManagingFunctions.CreateIndex(Vector2Int.RoundToInt(feet.position)))] == 1)
            {
                scale = Mathf.RoundToInt(scale);
                speed = 0f;

                Recalculate();
            }
        }
    }

    void Recalculate()
    {
        float antennaLenght = Mathf.Clamp(scale / 3, 0, 2f);
        leftAntenna.localPosition = new Vector2(-0.281f - antennaLenght * 0.5625f, 0.3751f);
        rightAntenna.localPosition = new Vector2(0.281f + antennaLenght * 0.5625f, 0.3751f);
        upperPart.localScale = new Vector3(antennaLenght + 1f, 1f, 1f);
        down.localScale = new Vector3(1f, scale, 1f);
        feet.localPosition = new Vector2(0f, -scale - 0.5f);
    }
}
