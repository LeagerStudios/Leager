using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundHearer : MonoBehaviour {

    public bool isHearing = true;
    public ISoundHearer atached;
    public string ID = "default";
	
    public void Notify(Vector2 soundOrigin)
    {
        atached.ReceivedSound(soundOrigin, ID);
    }
}

public interface ISoundHearer
{
    void ReceivedSound(Vector2 soundOrigin, string ID);
}