using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class ButtonToMouse : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public int key;

    public void OnPointerDown(PointerEventData eventData)
    {
        GInput.PressMouse(key);
        //SOYELGOFREDEBJGIERJEIJIWGIEJIGEJ
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        GInput.ReleaseMouse(key);
    }

    private void OnDisable()
    {
        GInput.ReleaseMouse(key);
    }
}
