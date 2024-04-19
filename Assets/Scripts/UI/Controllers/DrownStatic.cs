using UnityEngine;
using UnityEngine.UI;

public class DrownStatic : MonoBehaviour
{
    Image image;

    void Start()
    {
        image = GetComponent<Image>();
    }

    void Update()
    {
        float drownTime = GameManager.gameManagerReference.player.drownTime;
        Color color = Color.black;

        if(drownTime >= 5)
        {
            color.a = 0;
        }
        else
        {
            color.a = (5f - drownTime - 1f) / 5f;
        }

        image.color = Color.Lerp(image.color, color, Time.deltaTime);
    }
}
