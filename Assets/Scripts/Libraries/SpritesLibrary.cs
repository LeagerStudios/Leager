using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpritesLibrary : MonoBehaviour
{
    public Sprite[] Sprites;

    void Start()
    {
        LoadIcons();
    }


    void LoadIcons()
    {

        object[] loadedIcons = Resources.LoadAll("", typeof(Sprite));

        Sprites = new Sprite[loadedIcons.Length];

        for (int x = 0; x < loadedIcons.Length; x++)
        {
            Sprites[x] = (Sprite)loadedIcons[x];
        }

    }
}
