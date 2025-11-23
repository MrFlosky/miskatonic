using UnityEngine;
using System.Collections;
using System;

public class TempCardAnim : MonoBehaviour
{
    [Header("Settings")]
    public float raiseHeight = 0.2f;
    public float tiltAngle = 10f;
    public float duration = 0.25f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private Coroutine currentRoutine;
    private enum Movement {tilt, move, none};
    
    Movement movementState = Movement.none;
    [SerializeField]
    private Transform destTransform;

    [SerializeField]
    private CardSelectionHandler cardSelectionHandler;

    private void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
    }

    private void Update()
    {
    }

    public void RaiseAndTilt()
    {
        if(cardSelectionHandler.GetSelectedCard() != null) return;
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(RaiseTiltRoutine());
        movementState = Movement.tilt;
    }

    public void ResetTiltTransform()
    {
        if(movementState == Movement.move || cardSelectionHandler.GetSelectedCard() != null) return;
        if (currentRoutine != null)  StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(ResetRoutine());
    }
    public void MoveToDest()
    {
        if(cardSelectionHandler.GetSelectedCard() != null) return;
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(MoveToDestRoutine()); 
        movementState = Movement.move;   
    }

    public void ResetDestTransform()
    {
        if(movementState == Movement.tilt) return;
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(ResetMoveToDestRoutine());
    }

    private IEnumerator MoveToDestRoutine()
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 targetPos = destTransform.position;

        // Rotate forward along local X axis
        Quaternion targetRot = destTransform.rotation;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;

            transform.position = Vector3.Lerp(startPos, targetPos, progress);
            transform.rotation = Quaternion.Slerp(startRot, targetRot, progress);

            yield return null;
        }
    }

    private IEnumerator ResetMoveToDestRoutine()
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;

            transform.position = Vector3.Lerp(startPos, originalPosition, progress);
            transform.rotation = Quaternion.Slerp(startRot, originalRotation, progress);

            yield return null;
        }
    }



    private IEnumerator RaiseTiltRoutine()
    {
        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;

        Vector3 targetPos = originalLocalPosition + Vector3.up * raiseHeight;

        // Rotate forward along local X axis
        Quaternion targetRot = originalLocalRotation * Quaternion.Euler(-tiltAngle, 0f, 0f);

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

            transform.localPosition = Vector3.Lerp(startPos, originalLocalPosition, progress);
            transform.localRotation = Quaternion.Slerp(startRot, originalLocalRotation, progress);

            yield return null;
        }
    }
}
