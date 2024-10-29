using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameButton : MonoBehaviour
{
    Button button;

    [SerializeField] ClickAI clickAI;
    
    private void Awake()
    {
        button = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        button.interactable = !PlayerController.Instance.activeSkill;
    }
}
