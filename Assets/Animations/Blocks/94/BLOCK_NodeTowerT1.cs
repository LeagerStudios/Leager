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
    public float speed = 0f;

    void Update()
    {
        if (GameManager.gameManagerReference.InGame)
        {
            speed -= Mathf.Clamp(Time.deltaTime * Physics2D.gravity.y, 0f, 0.1f);
            scale += speed * Time.deltaTime;
            scale = Mathf.Clamp(scale, 0, 300);

            Recalculate();

            int tile = GameManager.gameManagerReference.GetTileAt(ManagingFunctions.CreateIndex(Vector2Int.RoundToInt(feet.position)));
            int topTile = 0;
            if(scale > 1)
            {
                topTile = GameManager.gameManagerReference.GetTileAt(ManagingFunctions.CreateIndex(Vector2Int.RoundToInt(feet.position)) + 1);
            }

            if (tile != 0 && speed >= 0)
            {
                scale = Mathf.RoundToInt(scale);
                speed = 0f;

                Recalculate();
            }

            if (topTile != 0)
            {
                speed = -9f;

                Recalculate();
            }

        }
    }

    void Recalculate()
    {
        float antennaLenght = Mathf.Clamp(scale / 3, 0, 2f);
        leftAntenna.localPosition = new Vector2(-0.28f - antennaLenght, 0.3751f);
        rightAntenna.localPosition = new Vector2(0.28f + antennaLenght, 0.3751f);
        upperPart.localScale = new Vector3(0.56f + antennaLenght * 2, 1f, 1f);
        down.localScale = new Vector3(1f, scale, 1f);
        feet.localPosition = new Vector2(0f, -scale - 0.5f);
    }
}
