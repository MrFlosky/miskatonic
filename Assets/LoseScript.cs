using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoseScript : MonoBehaviour
{
    public Light lightToDim;
    public float dimmingDuration = 2f;
    public string winSceneName;
    public TMP_Text mainText;          // First text element
    public TMP_Text reasonText;         // Second text element explaining the win
    public float textFadeDuration = 1f;
    public float waitAfterTextFade = 2f;

    public void Lose(string reason)
    {
        if (lightToDim != null)
        {
            StartCoroutine(DimLightAndLoadScene(reason));
        }
        else
        {
            Debug.LogWarning("Light reference is missing.");
        }
    }

    private IEnumerator DimLightAndLoadScene(string reason)
    {
        reasonText.text = reason;

        float initialIntensity = lightToDim.intensity;
        float elapsedTime = 0f;

        while (elapsedTime < dimmingDuration)
        {
            lightToDim.intensity = Mathf.Lerp(initialIntensity, 0f, elapsedTime / dimmingDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        lightToDim.intensity = 0f;  // Ensure the light is 0


        elapsedTime = 0f;
        Color initialColor = mainText.color;
        Color targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 1f);

        while (elapsedTime < textFadeDuration)
        {
            mainText.color = Color.Lerp(initialColor, targetColor, elapsedTime / textFadeDuration);
            reasonText.color = Color.Lerp(initialColor, targetColor, elapsedTime / textFadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        mainText.color = targetColor;  // Ensure main text is fully visible
        reasonText.color = targetColor;  // Ensure reason text is fully visible


        yield return new WaitForSeconds(waitAfterTextFade);
        SceneManager.LoadScene(winSceneName);
    }
}
