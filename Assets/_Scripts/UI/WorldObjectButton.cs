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
    public UnityEvent OnLeftClick;
    public UnityEvent OnRightClick;
    public UnityEvent OnMiddleClick;
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
        Debug.Log(eventData.button);
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            OnLeftClick?.Invoke();
        }
        if(eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick?.Invoke();
        }
        if(eventData.button == PointerEventData.InputButton.Middle)
        {
            OnMiddleClick?.Invoke();
        }
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
