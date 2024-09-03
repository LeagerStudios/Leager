using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickController : MonoBehaviour, IDragHandler
{
    public float speed = 0f;
    public float maxMagnitude = 32f;
    public float minimumThreshold = 1f;
    RectTransform rectTransform;

    public bool dragging;
    public Vector2 inputVector;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (speed > 0f)
        {
            speed -= rectTransform.anchoredPosition.magnitude * Time.deltaTime * 3;
            rectTransform.anchoredPosition = Vector2.ClampMagnitude(rectTransform.anchoredPosition, speed);
        }
        else
            speed = 0f;
    }

    public void OnDrag(PointerEventData data)
    {
        rectTransform.anchoredPosition += data.delta / MenuController.menuController.canvas.scaleFactor;
        rectTransform.anchoredPosition = Vector2.ClampMagnitude(rectTransform.anchoredPosition, maxMagnitude);
        speed = rectTransform.anchoredPosition.magnitude;
    }
}