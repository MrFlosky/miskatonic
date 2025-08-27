using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn;
using Yarn.Unity;
public class DialogueBank : MonoBehaviour
{
    DialogueRunner DR;

    public List<int> DialogueNum;

    void Start()
    {
        DR = FindObjectOfType<DialogueRunner>();

        for (int i = 0; i < DialogueNum.Count; i++)
        {
            DialogueNum[i] = i;


        }


    }

    


}
