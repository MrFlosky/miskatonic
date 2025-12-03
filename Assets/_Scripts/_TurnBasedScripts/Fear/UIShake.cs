using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIShake : MonoBehaviour
{
    private RectTransform rectTransform;
    private Vector2 originalPos;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 12f;
    private float shakeFrequency = 40f;

    private float totalShakeTime = 0f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPos = rectTransform.anchoredPosition;
    }

    void Update()
    {
        if (shakeDuration > 0)
        {
            float elapsed = totalShakeTime - shakeDuration;

            // Impact on first frame
            if (elapsed < 0.05f)
            {
                rectTransform.anchoredPosition = originalPos + new Vector2(0, shakeMagnitude * 1.2f);
            }
            else
            {
                float damping = shakeDuration / totalShakeTime;

                float y = (Mathf.PerlinNoise(0f, Time.time * shakeFrequency) - 0.5f)
                          * shakeMagnitude * damping;

                rectTransform.anchoredPosition = originalPos + new Vector2(0, y);
            }



            shakeDuration -= Time.deltaTime;
            if (shakeDuration <= 0)
            {
                rectTransform.anchoredPosition = originalPos;
            }
        }
    }

    public void Shake(float duration, float magnitude = 5f)
    {
        if (!AIFearMeter.IsVisible() || !AIFearMeter.IsCurrentMeter(this))
        {
              shakeDuration = 0f;
              shakeMagnitude = 5f;
              return;
        }
        shakeDuration = duration;
        totalShakeTime = duration;
        shakeMagnitude = magnitude;
    }
}