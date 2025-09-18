using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CultistManager : MonoBehaviour
{
    List<GameObject> AvailableOptionPrefabs;
    public float optionOffset = 200f;


    public void PresentOptions()
    {
        // Create 3 random options from the list of available options
        int i = Random.Range(0, AvailableOptionPrefabs.Count);
        GameObject option1 = Instantiate(AvailableOptionPrefabs[i], this.transform);
        option1.GetComponent<RectTransform>().anchoredPosition = new Vector2(-optionOffset, 0);
        AvailableOptionPrefabs.RemoveAt(i);

        i = Random.Range(0, AvailableOptionPrefabs.Count);
        GameObject option2 = Instantiate(AvailableOptionPrefabs[i], this.transform);
        AvailableOptionPrefabs.RemoveAt(i);

        i = Random.Range(0, AvailableOptionPrefabs.Count);
        GameObject option3 = Instantiate(AvailableOptionPrefabs[i], this.transform);
        option1.GetComponent<RectTransform>().anchoredPosition = new Vector2(optionOffset, 0);
        AvailableOptionPrefabs.RemoveAt(i);
    }
}
