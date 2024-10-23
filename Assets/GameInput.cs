using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public enum GameDevice
    {
        KeyboardMouse,
        Gamepad,
    }
    
    public static GameInput Instance { get; private set; }
    
    private GameDevice activeGameDevice;

    public event EventHandler OnGameDeviceChanged;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        InputSystem.onActionChange += InputSystem_OnActionChange;
    }

    private void InputSystem_OnActionChange(object arg1, InputActionChange inputActionChange)
    {
        if (inputActionChange == InputActionChange.ActionPerformed && arg1 is InputAction inputAction)
        {
            // Skip virtual mouse events
            if (inputAction.activeControl.device.displayName == "VirtualMouse")
            {
                return;
            }

            // Determine if the active device is a gamepad or keyboard/mouse
            if (inputAction.activeControl.device is Gamepad)
            {
                if (activeGameDevice != GameDevice.Gamepad)
                {
                    ChangeActiveGameDevice(GameDevice.Gamepad);
                }
            }
            else
            {
                if (activeGameDevice != GameDevice.KeyboardMouse)
                {
                    ChangeActiveGameDevice(GameDevice.KeyboardMouse);
                }
            }
        }
    }

    private void ChangeActiveGameDevice(GameDevice activeGameDevice)
    {
        this.activeGameDevice = activeGameDevice;
        Debug.Log("New Active Game Device: " + activeGameDevice);
        
        Cursor.visible = activeGameDevice == GameDevice.KeyboardMouse;
        
        OnGameDeviceChanged?.Invoke(this, EventArgs.Empty);
    }

    public GameDevice GetActiveGameDevice()
    {
        return activeGameDevice;
    }
}
