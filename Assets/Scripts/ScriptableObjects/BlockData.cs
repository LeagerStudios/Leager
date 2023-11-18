using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBlock", menuName = "Block/Block Data")]

public class BlockData : ScriptableObject
{

    public Sprite tile;
    public Sprite buildPreview;
    public string type;
    public string collisionType;
    public string mainProperty;
    public string breakTool;
    public string properties;
    public int efficency;
    public string tileName;
    public Color color;
    public BlockAnimationData animationData;

}
