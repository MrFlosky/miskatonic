using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoseScript : MonoBehaviour
{
    public Light lightToDim;
    public float lightFadeDuration = 2f;
    public string winSceneName;
    public TMP_Text mainText;          // First text element
    public TMP_Text reasonText;         // Second text element explaining the win
    public float textFadeDuration = 1f;
    public float waitAfterTextFade = 2f;
    public float agentFloatDuration = 1f; // Duration of agents floating up
    public float agentFloatDistance = 2f; // Distance agents will float up
    
    public float canvasFadeDuration = .5f;
    public CanvasGroup canvasGroup;

    private bool started = false;
    
    public void Lose(string reason)
    {
        if (lightToDim != null)
        {
            if (started) return;
            started = true;
            StartCoroutine(DimLightAndLoadScene(reason));
        }
        else
        {
            Debug.LogWarning("Light reference is missing.");
        }
    }

    private IEnumerator DimLightAndLoadScene(string reason)
    {
        mainText.text = "You Lose";
        reasonText.text = reason;
        
        float elapsedTime = 0f;

        while (elapsedTime < canvasFadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / canvasFadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0f;  // Ensure the light is fully off
        
        elapsedTime = 0f;
        
        AgentNavigation[] agents = FindObjectsOfType<AgentNavigation>();
        // Agents float up and fade out
        Vector3[] initialPositions = new Vector3[agents.Length];
        MeshRenderer[] agentRenderers = new MeshRenderer[agents.Length];
        Color[] initialColors = new Color[agents.Length];

        for (int i = 0; i < agents.Length; i++)
        {
            initialPositions[i] = agents[i].transform.position;
            agentRenderers[i] = agents[i].GetComponentInChildren<MeshRenderer>();
            initialColors[i] = agentRenderers[i].material.color;
        }

        while (elapsedTime < agentFloatDuration)
        {
            for (int i = 0; i < agents.Length; i++)
            {
                // Floating up
                agents[i].transform.position = Vector3.Lerp(initialPositions[i], initialPositions[i] + Vector3.up * agentFloatDistance, elapsedTime / agentFloatDuration);
                
                // Fading out
                Color fadedColor = Color.Lerp(initialColors[i], new Color(initialColors[i].r, initialColors[i].g, initialColors[i].b, 0f), elapsedTime / agentFloatDuration);
                agentRenderers[i].material.color = fadedColor;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure all agents are in final positions
        for (int i = 0; i < agents.Length; i++)
        {
            agents[i].transform.position = initialPositions[i] + Vector3.up * agentFloatDistance;
            agentRenderers[i].material.color = new Color(initialColors[i].r, initialColors[i].g, initialColors[i].b, 0f);
        }
        

        
        float initialIntensity = lightToDim.intensity;
        elapsedTime = 0f;

        while (elapsedTime < lightFadeDuration)
        {
            lightToDim.intensity = Mathf.Lerp(initialIntensity, 0f, elapsedTime / lightFadeDuration);
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
