using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBlockAnim", menuName = "Block/Block Anim")]
public class BlockAnimationData : ScriptableObject {

    public int rootTile;
    public Sprite[] frames;
    public float timePerFrame;
    public bool loop;
    public bool playOnStart;
    public bool startOnRandomFrame;
}
