using UnityEngine;
using System.Collections;

public class TempCardAnim : MonoBehaviour
{
    [Header("Settings")]
    public float raiseHeight = 0.2f;
    public float tiltAngle = 10f;
    public float duration = 0.25f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Coroutine currentRoutine;

    private void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }

    public void RaiseAndTilt()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(RaiseTiltRoutine());
    }

    public void ResetTransform()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(ResetRoutine());
    }

    private IEnumerator RaiseTiltRoutine()
    {
        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;

        Vector3 targetPos = originalPosition + Vector3.up * raiseHeight;

        // Rotate forward along local X axis
        Quaternion targetRot = originalRotation * Quaternion.Euler(-tiltAngle, 0f, 0f);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;

            transform.localPosition = Vector3.Lerp(startPos, targetPos, progress);
            transform.localRotation = Quaternion.Slerp(startRot, targetRot, progress);

            yield return null;
        }
    }

    private IEnumerator ResetRoutine()
    {
        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;

            transform.localPosition = Vector3.Lerp(startPos, originalPosition, progress);
            transform.localRotation = Quaternion.Slerp(startRot, originalRotation, progress);

            yield return null;
        }
    }
}
