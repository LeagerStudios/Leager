using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class ButtonToKey : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public KeyCode key;

    public void OnPointerDown(PointerEventData eventData)
    {
        GInput.PressKey(key);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        GInput.ReleaseKey(key);
    }

    private void OnDisable()
    {
        GInput.ReleaseKey(key);
    }
}
