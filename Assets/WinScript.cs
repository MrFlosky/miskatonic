using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinScript : MonoBehaviour
{
    public Light lightToDim;
    public float lightFadeDuration = 2f;
    public string winSceneName;
    public TMP_Text mainText;          // First text element
    public TMP_Text reasonText;         // Second text element explaining the win
    public float textFadeDuration = 1f;
    public float canvasFadeDuration = .5f;
    public float waitAfterTextFade = 2f;
    public float waitAfterParticle = 5f;
    
    public CanvasGroup canvasGroup;

    public GameObject textParent;

    public ParticleSystem winParticles;

    private string reasonWon = "";
    private bool won = false;
    
    private bool started = false;
    
    public void Win(string reason)
    {
        won = true;
        reasonWon = reason;
    }

    private void Update()
    {
        if (!(TurnBasedSystem.IsPlayerTurn() && won)) return;
        
        if (lightToDim != null)
        {
            if (started) return;
            started = true;
            StartCoroutine(DimLightAndLoadScene(reasonWon));
        }
        else
        {
            Debug.LogWarning("Light reference is missing.");
        }
    }

    private IEnumerator DimLightAndLoadScene(string reason)
    {
        textParent.SetActive(true);
        mainText.text = "You Win";
        reasonText.text = reason;
        
        float elapsedTime = 0f;

        while (elapsedTime < canvasFadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / canvasFadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0f;  // Ensure the light is fully off
        
        
        float initialIntensity = lightToDim.intensity;
        elapsedTime = 0f;

        while (elapsedTime < lightFadeDuration)
        {
            lightToDim.intensity = Mathf.Lerp(initialIntensity, 1f, elapsedTime / lightFadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        lightToDim.intensity = 1f;  // Ensure the light is fully off
        
        winParticles.Play();
        
        yield return new WaitForSeconds(waitAfterParticle);
        
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
