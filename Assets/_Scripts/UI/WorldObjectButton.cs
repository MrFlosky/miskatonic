using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class WorldObjectButton : MonoBehaviour, 
    IPointerDownHandler, 
    IPointerUpHandler,
    IPointerClickHandler, 
    IPointerEnterHandler, 
    IPointerExitHandler
{
    [Header("Events")]
    public UnityEvent OnClick;
    public UnityEvent OnPointerEnterEvent;
    public UnityEvent OnPointerExitEvent;

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPointerEnterEvent?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExitEvent?.Invoke();
    }
}
