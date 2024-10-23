using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;

public class VirtualMouseUI : MonoBehaviour
{
    public static VirtualMouseUI Instance { get; private set; }
    
    [SerializeField] private RectTransform virtualMouse;
    [SerializeField] private RectTransform canvasRectTransform;
    private VirtualMouseInput _virtualMouseInput;

    private void Awake()
    {
        Instance = this;
        
        _virtualMouseInput = GetComponent<VirtualMouseInput>();
    }

    private void Start()
    {
        GameInput.Instance.OnGameDeviceChanged += GameInput_OnGameDeviceChanged;
    }

    private void GameInput_OnGameDeviceChanged(object sender, EventArgs e)
    {
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (GameInput.Instance.GetActiveGameDevice() == GameInput.GameDevice.Gamepad)
        {
            ResetMouseToCenter();
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
    
    private void Hide()
    {
        try
        {
            gameObject.SetActive(false);
        }
        catch (Exception e)
        {
            Debug.LogError("VirtualCursorUI Error: " + e);
        }
    }

    private void ResetMouseToCenter()
    {
        virtualMouse.anchoredPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
    }

    public Vector2 GetVirtualMousePosition()
    {
        return virtualMouse.anchoredPosition;
    }

    
    private void Update()
    {
        transform.localScale = Vector3.one * (1f / canvasRectTransform.localScale.x);
        transform.SetAsLastSibling();
    }

    private void LateUpdate()
    {
        // Only update the virtual mouse position if using a gamepad
        if (GameInput.Instance.GetActiveGameDevice() == GameInput.GameDevice.Gamepad)
        {
            Vector2 virtualMousePosition = _virtualMouseInput.virtualMouse.position.value;
            virtualMousePosition.x = Mathf.Clamp(virtualMousePosition.x, 0f, Screen.width);
            virtualMousePosition.y = Mathf.Clamp(virtualMousePosition.y, 0f, Screen.height);
        
            InputState.Change(_virtualMouseInput.virtualMouse.position, virtualMousePosition);
        }
    }

}
