using System;
using UnityEngine;

public static class MapChangeEvents
{
    public static event Action OnBarrierChanged;
    
    public static event Action OnRoomsSwapped;
    
    public static event Action OnMapChanged;
    
    public static void InvokeBarrierChanged()
    {
        Debug.Log("MapChangeEvents: Barrier changed - invalidating agent paths");
        OnBarrierChanged?.Invoke();
        OnMapChanged?.Invoke();
    }
    
    public static void InvokeRoomsSwapped()
    {
        Debug.Log("MapChangeEvents: Rooms swapped - invalidating agent paths");
        OnRoomsSwapped?.Invoke();
        OnMapChanged?.Invoke();
    }
    
    public static void ClearAllSubscriptions()
    {
        OnBarrierChanged = null;
        OnRoomsSwapped = null;
        OnMapChanged = null;
    }
}