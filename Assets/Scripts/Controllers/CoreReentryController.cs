using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreReentryController : MonoBehaviour
{
    public static CoreReentryController self;

    private void Awake()
    {
        self = this;
    }

    public void StartAnimation(int core)
    {
        StartCoroutine(Animation(core));
    }

    public IEnumerator Animation(int core)
    {
        if(core == 96)
        {

        }
        else if (core == 97)
        {

        }
        else if (core == 100)
        {

        }

        yield return new WaitForEndOfFrame();
    }
}
