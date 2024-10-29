using UnityEngine;
using UnityEngine.EventSystems;

public class InfoLogic : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject targetObject;

    void Start()
    {
        
        if (targetObject != null)
            targetObject.SetActive(false);
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetObject != null)
            targetObject.SetActive(true);
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetObject != null)
            targetObject.SetActive(false);
    }
}
