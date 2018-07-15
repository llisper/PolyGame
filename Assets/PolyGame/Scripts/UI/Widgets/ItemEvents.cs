using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class ItemEvents : MonoBehaviour, IPointerClickHandler
{
    public Action<GameObject> onClicked;

    public void OnPointerClick(PointerEventData eventData)
    {
        onClicked?.Invoke(gameObject);
    }
}
