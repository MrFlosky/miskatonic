using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible for the hop and rotation animations of the bust.
/// </summary>
public class BustMoveJuice : MonoBehaviour
{
    [Header("Hop Animation Settings")]
    [SerializeField] private float hopHeight = 0.3f;
    [SerializeField] private float hopDurationPerCell = 0.3f;
    [SerializeField] private float tiltAngle = 15f;
    
    [Header("Rotation Animation Settings")]
    [SerializeField] private float rotationDuration = 0.4f;
    
    /// <summary>
    /// Variables for controlling hopping animation.
    /// </summary>
    private bool isHopping = false;
    private bool isFinishingHop = false; 
    private float hopTimer = 0f;
    private Vector3 startPosition;
    
    /// <summary>
    /// Variables for controlling facing direction rotation.
    /// </summary>
    private bool isRotating = false;
    private float rotationTimer = 0f;
    private Quaternion startRotation;
    private Quaternion targetRotation;
    private Quaternion currentBaseRotation;
    
    /// <summary>
    /// Used to detect movement change and sub to facing direction change event.
    /// </summary>
    private AgentNavigation agentNav;
    private bool wasMovingLastFrame = false;

    void Awake()
    {
        agentNav = transform.parent.GetComponent<AgentNavigation>();
        agentNav.facingDirectionChanged += AnimateBustFacing;
        startPosition = transform.localPosition;
        currentBaseRotation = transform.localRotation;
    }

    /// <summary>
    /// Incase this destroyed before agent nav for whatever reason.
    /// </summary>
    void OnDestroy()
    {
        if (agentNav != null)
        {
            agentNav.facingDirectionChanged -= AnimateBustFacing;
        }
    }

    /// <summary>
    /// Set target rotation for bust to animate to, given a facing direction.
    /// This function is subscribed to event in agent nav that detects if a facing direction
    /// change occured.
    /// </summary>
    void AnimateBustFacing(Vision facing)
    {
        Vector3 targetEuler = Vector3.zero;
        
        switch (facing)
        {
            case Vision.Up:
                targetEuler = new Vector3(0f, 0f, 0f);
                break;
            case Vision.Down:
                targetEuler = new Vector3(0f, 180f, 0f);
                break;
            case Vision.Left:
                targetEuler = new Vector3(0f, -90f, 0f);
                break;
            case Vision.Right:
                targetEuler = new Vector3(0f, 90f, 0f);
                break;
        }
        
        startRotation = currentBaseRotation;
        targetRotation = Quaternion.Euler(targetEuler);
        isRotating = true;
        rotationTimer = 0f;
    }

    void Update()
    {
        // Detect movement changes
        bool isMovingNow = agentNav.IsMoving();
        
        if (isMovingNow && !wasMovingLastFrame)
        {
            // Started moving, start hopping
            isHopping = true;
            isFinishingHop = false;
            hopTimer = 0f;
            startPosition = transform.localPosition;
        }
        else if (!isMovingNow && wasMovingLastFrame)
        {
            // Stopped moving, finish the current hop cycle
            isHopping = false;
            isFinishingHop = true;
            // Don't reset position yet - let animation finish
        }
        
        wasMovingLastFrame = isMovingNow;
        
        UpdateHopAnimation();
        UpdateRotationAnimation();
    }
    
    /// <summary>
    /// Called every frame the bust moves to animate hopping.
    /// </summary>
    void UpdateHopAnimation()
    {
        if (!isHopping && !isFinishingHop) return;

        hopTimer += Time.deltaTime;
        
        // Continuous hopping
        float hopCycleProgress = (hopTimer % hopDurationPerCell) / hopDurationPerCell;
        float hopProgress = Mathf.Sin(hopCycleProgress * Mathf.PI);
        Vector3 hopOffset = Vector3.up * (hopProgress * hopHeight);
        transform.localPosition = startPosition + hopOffset;
        
        // Check if we've completed the finishing cycle
        if (isFinishingHop && hopCycleProgress < 0.05f && hopTimer > hopDurationPerCell * 0.5f)
        {
            // Completed the cycle, fully stop
            isFinishingHop = false;
            transform.localPosition = startPosition;
            
            // Sync currentBaseRotation when fully stopped
            if (!isRotating)
            {
                currentBaseRotation = transform.localRotation;
            }
        }
    }
    
    /// <summary>
    /// Called every frame, handles both bust sway while hopping and 
    /// facing direction animation.
    /// </summary>
    void UpdateRotationAnimation()
    {
        // Only control rotation when animating, hopping, or finishing hop
        bool shouldControlRotation = isRotating || isHopping || isFinishingHop;
        
        if (!shouldControlRotation)
        {
            // Not controlling rotation, let other scripts handle it
            return;
        }
        
        // Update base rotation (Y-axis turn)
        if (isRotating)
        {
            rotationTimer += Time.deltaTime;
            float progress = rotationTimer / rotationDuration;
            
            if (progress >= 1f)
            {
                isRotating = false;
                currentBaseRotation = targetRotation;
            }
            else
            {
                float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
                currentBaseRotation = Quaternion.Slerp(startRotation, targetRotation, easedProgress);
            }
        }
        
        // Add tilt during hop (X-axis forward lean)
        float tiltAmount = 0f;
        if (isHopping || isFinishingHop)
        {
            // Sine wave gives 0 -> peak -> 0, matching the hop arc
            float hopCycleProgress = (hopTimer % hopDurationPerCell) / hopDurationPerCell;
            float tiltProgress = Mathf.Sin(hopCycleProgress * Mathf.PI);
            tiltAmount = tiltProgress * tiltAngle;
        }
        
        // Apply: base rotation + tilt
        Quaternion tiltRotation = Quaternion.Euler(tiltAmount, 0f, 0f);
        transform.localRotation = currentBaseRotation * tiltRotation;
    }
}