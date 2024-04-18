using UnityEngine;
using UnityEngine.UI;

public class DamageStatic : MonoBehaviour
{
    Image image;

    void Start()
    {
        image = GetComponent<Image>();
    }

    void Update()
    {
        int hp = GameManager.gameManagerReference.player.HP;
        Color color = Color.white;

        if(hp > 5)
        {
            color.a = 0;
        }
        else
        {
            color.a = (5f - hp + 1f) / 5f;
        }

        image.color = Color.Lerp(image.color, color, Time.deltaTime);
    }
}
