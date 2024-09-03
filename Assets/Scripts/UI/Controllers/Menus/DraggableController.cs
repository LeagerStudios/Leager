using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableController : MonoBehaviour, IDragHandler
{
    public GameObject target;
    IDraggable iTarget;
    Vector2 speed;

    void OnValidate()
    {
        Validate();
    }

    void Validate()
    {
        if (target != null)
            if (target.GetComponent<IDraggable>() != null)
            {
                iTarget = target.GetComponent<IDraggable>();
            }
            else
            {
                Debug.LogWarning(gameObject.name + " has not an IDraggable target basing on " + target.name);
            }
    }

    void Start()
    {
        Validate();
    }

    void Update()
    {
        if(iTarget != null)
        {
            if (iTarget.CanDrag)
            {
                if (speed.magnitude > 0.01f)
                {
                    GetComponent<RectTransform>().anchoredPosition += speed;
                    speed *= 0.9f;
                    iTarget.Drag();
                }
            }
            else
            {
                speed = Vector2.zero;
            }
        }
    }

    public void OnDrag(PointerEventData data)
    {
        speed = data.delta / MenuController.menuController.canvas.scaleFactor;
        GetComponent<RectTransform>().anchoredPosition += speed;
        iTarget.Drag();
    }
}

public interface IDraggable
{
    bool CanDrag { get; }
    void Drag();
}
