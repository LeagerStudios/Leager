using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour {

    float secondsLess;
    string[] mgs;
    ITimerCall timerCall;
    bool asigned = false;

	public void InvokeTimer(float seconds, string[] message, ITimerCall endCall)
    {
        secondsLess = seconds;
        mgs = message;
        timerCall = endCall;
        asigned = true;
    }
	

	void Update ()
    {
        
        if (asigned && GameManager.gameManagerReference.InGame)
        {
            secondsLess -= Time.deltaTime;
            if(secondsLess < 0f)
            {
                timerCall.TimerCall(mgs);
                Destroy(this);
            }
        }
	}
}

public interface ITimerCall
{
    void TimerCall(string[] msg);
}