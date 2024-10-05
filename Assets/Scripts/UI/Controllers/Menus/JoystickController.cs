using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickController : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public float speed = 0f;
    public float maxMagnitude = 32f;
    public float minimumThreshold = 1f;
    RectTransform rectTransform;

    public bool dragging;
    public Vector2 inputVector;

    public KeyCode up;
    public KeyCode left;
    public KeyCode down;
    public KeyCode right;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (!dragging)
            if (speed > 0f)
            {
                speed -= rectTransform.anchoredPosition.magnitude * Time.deltaTime * 5;
                rectTransform.anchoredPosition = Vector2.ClampMagnitude(rectTransform.anchoredPosition, speed);
            }
            else
                speed = 0f;
    }

    public void OnDrag(PointerEventData data)
    {
        if (dragging)
        {
            inputVector += data.delta / MenuController.menuController.canvas.scaleFactor;
            rectTransform.anchoredPosition = inputVector;
            rectTransform.anchoredPosition = Vector2.ClampMagnitude(rectTransform.anchoredPosition, maxMagnitude);
            speed = rectTransform.anchoredPosition.magnitude;

            if (inputVector.x > minimumThreshold)
            {
                GInput.PressKey(right);
            }
            else
            {
                GInput.ReleaseKey(right);
            }

            if(inputVector.x < -minimumThreshold)
            {
                GInput.PressKey(left);
            }
            else
            {
                GInput.ReleaseKey(left);
            }

            if (inputVector.y > minimumThreshold)
            {
                GInput.PressKey(up);
            }
            else
            {
                GInput.ReleaseKey(up);
            }

            if (inputVector.y < -minimumThreshold)
            {
                GInput.PressKey(down);
            }
            else
            {
                GInput.ReleaseKey(down);
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {

        inputVector = Vector2.zero;
        dragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        dragging = false;
        inputVector = Vector2.zero;
        ReleaseAll();
    }

    private void OnDisable()
    {
        dragging = false;
        inputVector = Vector2.zero;
        ReleaseAll();
        rectTransform.anchoredPosition = inputVector;
    }

    private void ReleaseAll()
    {
        GInput.ReleaseKey(up);
        GInput.ReleaseKey(down);
        GInput.ReleaseKey(left);
        GInput.ReleaseKey(right);
    }

}