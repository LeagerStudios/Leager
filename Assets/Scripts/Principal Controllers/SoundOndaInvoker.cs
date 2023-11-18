using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundOndaInvoker : MonoBehaviour
{
    [SerializeField] GameObject soundOndaPrefab;

	void Start () {
        SoundOndaSpawner.soundOndaPrefab = soundOndaPrefab;
        SoundOndaSpawner.soundOndaClassRef = gameObject;
	}
	

	void Update () {
		
	}
}

public static class SoundOndaSpawner
{
    public static GameObject soundOndaPrefab;
    public static GameObject soundOndaClassRef;

    public static void MakeSoundOnda(Vector2 originPosition)
    {
        SoundHearer[] soundHearers = soundOndaClassRef.transform.GetComponentsInChildren<SoundHearer>();

        foreach(SoundHearer soundHearer in soundHearers)
        {
            if(Vector2.Distance(originPosition, soundHearer.transform.position) <= 16f && soundHearer.isHearing)
            {
                SoundOndaController soundOnda = GameObject.Instantiate(soundOndaPrefab, originPosition, Quaternion.identity).GetComponent<SoundOndaController>();
                soundOnda.transform.localEulerAngles = new Vector3(0f, 0f, ManagingFunctions.PointToPivotUp(originPosition, soundHearer.transform.position));
                soundOnda.InvokeOnda(originPosition, soundHearer, soundOnda.transform.localEulerAngles.z);
            }
        }
    }

}
