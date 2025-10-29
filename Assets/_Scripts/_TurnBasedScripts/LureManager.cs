using UnityEngine;

public class LureManager : MonoBehaviour
{
    public static LureManager Instance;
    
    private Vector2Int? activeLurePosition;
    private GameObject lureVisual;
    private int lureRoundsRemaining;
    
    [Header("Lure Visual")]
    [SerializeField] private GameObject lurePrefab;
    [SerializeField] private float lureHeight = 0.5f;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void PlaceLure(Vector2Int gridPosition, int rounds)
    {
        RemoveLure();
        
        activeLurePosition = gridPosition;
        lureRoundsRemaining = rounds;
        
        if (lurePrefab != null)
        {
            Vector3 worldPos = GetWorldPosition(gridPosition);
            lureVisual = Instantiate(lurePrefab, worldPos, Quaternion.identity);
        }
        
        Debug.Log($"LureManager: Lure placed at {gridPosition} for {rounds} rounds");
        
        MapChangeEvents.InvokeBarrierChanged();
    }

    public void RemoveLure()
    {
        if (activeLurePosition.HasValue)
        {
            Debug.Log($"LureManager: Lure removed from {activeLurePosition.Value}");
            activeLurePosition = null;
            lureRoundsRemaining = 0;
            
            if (lureVisual != null)
            {
                Destroy(lureVisual);
                lureVisual = null;
            }
            
            MapChangeEvents.InvokeBarrierChanged();
        }
    }
    
    public void DecreaseLureDuration()
    {
        if (activeLurePosition.HasValue)
        {
            lureRoundsRemaining--;
            Debug.Log($"LureManager: Lure duration decreased to {lureRoundsRemaining} rounds");
            
            if (lureRoundsRemaining <= 0)
            {
                RemoveLure();
            }
        }
    }
    
    public bool IsLureActive()
    {
        return activeLurePosition.HasValue && lureRoundsRemaining > 0;
    }
    
    public Vector2Int? GetLurePosition()
    {
        return activeLurePosition;
    }
    
    public int GetLureRoundsRemaining()
    {
        return lureRoundsRemaining;
    }
    
    private Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null");
            return Vector3.zero;
        }
        
        return new Vector3(
            gridPosition.x * GameManager.Instance.CellSize,
            lureHeight,
            gridPosition.y * GameManager.Instance.CellSize
        );
    }
}