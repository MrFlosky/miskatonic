using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillShowPaths : Skill
{
    [Header("Path Preview Settings")]
    [Tooltip("How long the paths remain visible (in seconds). Set to 0 for permanent until agent turn starts.")]
    public float displayDuration;
    
    private bool pathsCurrentlyShown;
    private float showTimer;
    
    private void Awake()
    {
        skillSystem = GetComponent<SkillSystem>();
    }

    private void Start()
    {
        enabled = false;
    }
    
    private void Update()
    {
        if (pathsCurrentlyShown && displayDuration > 0)
        {
            showTimer -= Time.deltaTime;
            if (showTimer <= 0)
            {
                HidePathsAndEnd();
            }
        }
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        // Not needed for this skill
    }

    public void OnConfirm(InputAction.CallbackContext context)
    {
        // Not needed for this skill
    }

    public void OnRightClick(InputAction.CallbackContext context)
    {
        if (pathsCurrentlyShown)
        {
            Debug.Log("Hiding paths early via right-click");
            HidePathsAndEnd();
        }
        else
        {
            ExitInProgress();
        }
    }

    public override void ActivateSkill()
    {
        if (usagesRemaining >= usageDeduction)
        {
            base.ActivateSkill();
            
            if (!TurnBasedSystem.IsPlayerTurn())
            {
                Debug.LogWarning("Cannot show paths - not player turn");
                usagesRemaining += usageDeduction;
                return;
            }
            
            ShowAgentPaths();
            
            this.enabled = true;
        }
    }
    
    private void ShowAgentPaths()
    {
        if (PathVisualizer.Instance == null)
        {
            Debug.LogError("PathVisualizer.Instance not found!");
            return;
        }
        
        Debug.Log("Showing agent path predictions");
        PathVisualizer.Instance.ShowPaths();
        pathsCurrentlyShown = true;
        
        if (displayDuration > 0)
        {
            showTimer = displayDuration;
        }
        
        AudioManager.PlaySFX(SFXType.MouseButtonHit);
    }
    
    private void HidePathsAndEnd()
    {
        if (PathVisualizer.Instance != null && pathsCurrentlyShown)
        {
            Debug.Log("Hiding agent path predictions");
            PathVisualizer.Instance.HidePaths();
        }
        
        pathsCurrentlyShown = false;
        showTimer = 0f;
        
        PlayerController.Instance.SetActiveSkill(null);
        this.enabled = false;
        EndSkill();
    }

    public override void EndSkill()
    {
        base.EndSkill();
        
        if (pathsCurrentlyShown && PathVisualizer.Instance != null)
        {
            PathVisualizer.Instance.HidePaths();
            pathsCurrentlyShown = false;
        }
        
        this.enabled = false;
    }
}