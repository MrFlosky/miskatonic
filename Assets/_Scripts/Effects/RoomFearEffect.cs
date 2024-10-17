using UnityEngine;

[CreateAssetMenu(fileName = "RoomFearEffect", menuName = "ScriptableObjects/RoomFearEffect", order = 1)]
public class RoomFearEffect : ScriptableObject
{
    public string roomType;
    public FearEffect fearEffect;

    public enum FearEffect
    {
        Increase,
        Decrease,
        None
    }
}