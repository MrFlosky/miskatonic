//MIT License
//Copyright (c) 2023 DA LAB (https://www.youtube.com/@DA-LAB)
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class OutlineHighlight : MonoBehaviour
{
    public InputActionAsset inputActionAsset; // Reference to the InputActionAsset
    private InputAction pointerPositionAction;
    private Transform highlight;
    private RaycastHit raycastHit;

    [SerializeField]
    private float outlineWidth = 10f;
    
    [SerializeField]
    private Color outlineColor = Color.black;

    private void Awake()
    {
        // Get the InputAction for pointer position
        pointerPositionAction = inputActionAsset.FindAction("Player/PointerPosition"); // Make sure this path matches your InputAction setup
    }

    private void OnEnable()
    {
        // Enable the InputAction
        pointerPositionAction.Enable();
    }

    private void OnDisable()
    {
        // Disable the InputAction
        pointerPositionAction.Disable();
    }

    private void Update()
    {
        if (PlayerController.Instance.clickedAI)
        {
            Outline outline = highlight.gameObject.GetComponent<Outline>();
            outline.enabled = false;
            return;
        }
        
        // Get the pointer position from the InputAction
        Vector2 pointerPosition = pointerPositionAction.ReadValue<Vector2>();
        Ray ray = Camera.main.ScreenPointToRay(pointerPosition);

        // Disable previous highlight
        if (highlight != null)
        {
            highlight.gameObject.GetComponent<Outline>().enabled = false;
            highlight = null;
        }

        // Perform raycast to detect objects
        if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out raycastHit))
        {
            Debug.Log(raycastHit.collider.gameObject.name);
            highlight = raycastHit.transform;
            if (highlight.tag is "Selectable" or "AI")
            {
                // Enable outline component or add one if it doesn't exist
                Outline outline = highlight.gameObject.GetComponent<Outline>();
                if (outline == null)
                {
                    Debug.Log("Added Outline to " + highlight.name);
                    outline = highlight.gameObject.AddComponent<Outline>();
                    outline.OutlineColor = Color.black;
                    outline.OutlineWidth = outlineWidth;
                }
                outline.enabled = true;
            }
            else
            {
                highlight = null;
            }
        }
    }
}
