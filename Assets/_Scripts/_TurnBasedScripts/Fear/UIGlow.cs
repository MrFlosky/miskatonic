using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGlowPulse : MonoBehaviour
{
    private Image image;
    private Coroutine pulseRoutine;

    [Header("Glow Settings")]
    public Color glowColor = new Color(1f, 0f, 1f, 0.8f); // purple glow
    public float pulseDuration = 0.5f;

    private Color originalColor;

    void Awake()
    {
        image = GetComponent<Image>();
        originalColor = image.color;
    }

    public void Pulse(float intensity = 1f)
    {
        // Prevent pulsing when the fear meter is hidden
        if (!AIFearMeter.IsVisible())
        {
            image.color = originalColor;
            return;
        }

        // Prevent pulsing when this is not the active meter
        if (!AIFearMeter.IsCurrentMeter(this))
        {
            image.color = originalColor;
            return;
        }

        if (pulseRoutine != null)
            StopCoroutine(pulseRoutine);

        image.color = originalColor;

        pulseRoutine = StartCoroutine(PulseRoutine(intensity));
    }

    private System.Collections.IEnumerator PulseRoutine(float intensity)
    {
        float t = 0f;
        Color targetColor = new Color(
            glowColor.r,
            glowColor.g,
            glowColor.b,
            glowColor.a * intensity
        );


        // Flash toward glow
        while (t < pulseDuration)
        {
            float lerp = t / pulseDuration;
            image.color = Color.Lerp(originalColor, targetColor, lerp);
            t += Time.deltaTime;
            yield return null;
        }

        // Flash back
        t = 0f;
        while (t < pulseDuration)
        {
            float lerp = t / pulseDuration;
            image.color = Color.Lerp(targetColor, originalColor, lerp);
            t += Time.deltaTime;
            yield return null;
        }

        image.color = originalColor;
    }
}