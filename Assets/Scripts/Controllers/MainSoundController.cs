using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSoundController : MonoBehaviour {

    [SerializeField] public AudioClip[] Sounds;
    [SerializeField] public AudioClip[] Themes;
    byte sfxPlayed = 0;

    void Start ()
    {
        
	}
	
	void Update ()
    {
        sfxPlayed = 0;
	}

    public void PlaySfxSound(SoundName s)
    {
        if (sfxPlayed <= 10)
        {
            transform.GetChild(0).GetComponent<AudioSource>().PlayOneShot(Sounds[(int)s], 1f);
            sfxPlayed++;
        }       
    }

    public void PlaySfxSound(AudioClip audioClip)
    {
        if (sfxPlayed <= 10)
        {
            transform.GetChild(0).GetComponent<AudioSource>().PlayOneShot(audioClip, 1f);
            sfxPlayed++;
        }
    }

    public void PlayMusicSound(ThemeName t)
    {
        transform.GetChild(1).GetComponent<AudioSource>().Stop();
        transform.GetChild(1).GetComponent<AudioSource>().clip = null;
        transform.GetChild(1).GetComponent<AudioSource>().clip = Themes[(int)t];
        transform.GetChild(1).GetComponent<AudioSource>().Play();
    }
}

public enum SoundName : int
{
    damage = 0,
    grabSound = 1,
    select = 2,
}

public enum ThemeName : int
{
    korenzMeadows = 0
}
