using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillLure : Skill
{
    [Header("Lure Settings")]
    public Material highlightMat;
    public int roundsLast = 3;
    
    private MazeCellObject selectedCellObj;
    private Material originalFloorMat;
    
    private void Start()
    {
        enabled = false;
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        Vector2 mousePos = Vector2.zero;
        mousePos = GameInput.Instance.GetActiveGameDevice() == GameInput.GameDevice.KeyboardMouse 
            ? Mouse.current.position.ReadValue() 
            : VirtualMouseUI.Instance.GetVirtualMousePosition();
            
        Ray ray = skillSystem.GET_CAM().ScreenPointToRay(mousePos);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, raycastLayerMask))
        {
            MazeCellObject cellHit = hit.transform.GetComponentInParent<MazeCellObject>();
            
            if (cellHit != null)
            {
                if (selectedCellObj != null)
                {
                    ResetCellHighlight();
                }
                
                selectedCellObj = cellHit;
                originalFloorMat = cellHit.roomFloorMaterial;
                
                cellHit.roomFloorMaterial = highlightMat;
                cellHit.RefreshMaterial();
                
                AudioManager.PlaySFX(SFXType.MouseButtonHit);
                Debug.Log($"Selected cell at ({cellHit.GetXCoord()}, {cellHit.GetYCoord()}) for lure placement");
            }
        }
    }

    public void OnConfirm(InputAction.CallbackContext context)
    {
        if (selectedCellObj != null)
        {
            PlayerController.Instance.SetActiveSkill(null);
            AudioManager.PlaySFX(SFXType.SkillBarrier); // Use appropriate sound
            
            int x = selectedCellObj.GetXCoord();
            int y = selectedCellObj.GetYCoord();
            Vector2Int lurePos = new Vector2Int(x, y);
            
            // Place the lure
            if (LureManager.Instance != null)
            {
                LureManager.Instance.PlaceLure(lurePos, roundsLast);
            }
            else
            {
                Debug.LogError("LureManager instance not found!");
            }
            
            Debug.Log($"Lure placed at ({x}, {y})");
            
            ResetCellHighlight();
            this.enabled = false;
            EndSkill();
        }
    }

    public void OnRightClick(InputAction.CallbackContext context)
    {
        if (selectedCellObj != null)
        {
            Debug.Log("Cancelling lure selection");
            ResetCellHighlight();
            AudioManager.PlaySFX(SFXType.MouseButtonHit);
        }
        else
        {
            Debug.Log("Exiting lure skill");
            ExitInProgress();
        }
    }

    public override void EndSkill()
    {
        base.EndSkill();
        ResetCellHighlight();
        this.enabled = false;
    }

    private void ResetCellHighlight()
    {
        if (selectedCellObj != null)
        {
            selectedCellObj.roomFloorMaterial = originalFloorMat;
            selectedCellObj.RefreshMaterial();
            selectedCellObj = null;
        }
    }

    public override void ActivateSkill()
    {
        if (usagesRemaining >= usageDeduction)
        {
            base.ActivateSkill();
            this.enabled = true;
        }
    }
}