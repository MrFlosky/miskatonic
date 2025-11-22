using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CardSelectionHandler : MonoBehaviour
{

    [SerializeField]
    private GameObject selectedCard = null;

    [SerializeField]
    private UnityEvent OnCardActivation;

    public void ActivateCard(GameObject card)
    {
        if (selectedCard != null && selectedCard.GetComponent<MissionDisplay>().SelectCard())
        {
            selectedCard.GetComponent<TempCardAnim>().ResetDestTransform();
            UnselectCard();
            OnCardActivation?.Invoke();
        }
        else if(selectedCard == null)
        {
            SetSelectedCard(card);
        }
        else
        {
            //Probably want to show the player that they don't have enough cultists in some fasion
        }
    }

    public void SetSelectedCard(GameObject card)
    {
        if(selectedCard != null) return;
        selectedCard = card;
    }

    public void UnselectCard()
    {
        selectedCard = null;
    }

    public GameObject GetSelectedCard()
    {
        return selectedCard;
    }
}
