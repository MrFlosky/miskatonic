using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIShake : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector2 originalPos;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 5f;
    private float shakeFrequency = 25f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPos = rectTransform.anchoredPosition;
    }

    void Update()
    {
        if (shakeDuration > 0)
        {
            float x = (Mathf.PerlinNoise(Time.time * shakeFrequency, 0f) - 0.5f) * shakeMagnitude;
            float y = (Mathf.PerlinNoise(0f, Time.time * shakeFrequency) - 0.5f) * shakeMagnitude;

            rectTransform.anchoredPosition = originalPos + new Vector2(x, y);

            shakeDuration -= Time.deltaTime;
            if (shakeDuration <= 0)
            {
                rectTransform.anchoredPosition = originalPos;
            }
        }
    }

    public void Shake(float duration, float magnitude = 5f)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }
}