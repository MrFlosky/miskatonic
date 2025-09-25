using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DispatchOption : MonoBehaviour
{
    public int DispatchCost = 1;

    public virtual void ChooseOption()
    {
        CultistManager.Instance.OptionChosen();
        // add effect functionality
    }

}
