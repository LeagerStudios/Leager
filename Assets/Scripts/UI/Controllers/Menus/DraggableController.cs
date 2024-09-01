using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableController : MonoBehaviour, IDragHandler
{
    public GameObject target;
    IDraggable iTarget;

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

    public void OnDrag(PointerEventData data)
    {
        GetComponent<RectTransform>().anchoredPosition += data.delta / MenuController.menuController.canvas.scaleFactor;
        iTarget.Drag();
    }
}

public interface IDraggable
{
    bool CanDrag { get; }
    void Drag();
}
