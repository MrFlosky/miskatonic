using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface class whose functions must be implemented in deriving classes.
/// </summary>
public interface ITurnObserver
{
    void EnemyTurnInitiated();

    void PlayerTurnInitiated();

}
