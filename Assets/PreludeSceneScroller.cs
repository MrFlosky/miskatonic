using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;

public class PreludeSceneScroller : MonoBehaviour
{
    public List<GameObject> screens;
    public int currentScreen = 0;

    public void ProgressScreen()
    {
        if (currentScreen == screens.Count - 1)
        {
            StartCoroutine(FadeScreen());
            return;
        } 
        screens[currentScreen].SetActive(false);
        currentScreen++;
        screens[currentScreen].SetActive(true);
    }

    public IEnumerator FadeScreen()
    {
        TextMeshProUGUI text = screens[currentScreen].GetComponentInChildren<TextMeshProUGUI>();

        for (int i = 0; i < 10; i++)
        {
            text.color = new Color(255, 255, 255, 1 - i);
            yield return new WaitForSeconds(.1f);
        }

        SceneManager.LoadScene("NEW_MAIN");
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        ProgressScreen();
    }
}
