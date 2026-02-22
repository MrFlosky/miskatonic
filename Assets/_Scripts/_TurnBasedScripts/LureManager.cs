using UnityEngine;


/// <summary>
/// This class is responsible for tracking lure lifetime.
/// This should be handled in TurnBasedSystem along with other skill lifetimes,
/// but alas it is not.
/// </summary>
public class LureManager : MonoBehaviour
{
    public static LureManager Instance;
    
    private Vector2Int? activeLurePosition;
    private GameObject lureVisual;
    private int lureRoundsRemaining;
    
    [Header("Lure Visual")]
    [SerializeField] private GameObject lurePrefab;
    [SerializeField] private float lureHeight = 0.5f;

    /// <summary>
    /// Ideally this script does not exist and skill lure manages the lure entirely.
    /// Reference here for telling skill lure when to lock up and disallow more than
    /// one lure placement.
    /// </summary>
    private SkillLure skillLure;
    public void SetSkillLure(SkillLure s)
    {
        skillLure = s;
    }
    
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
                skillLure.ExpirationUnlock(); // allow another lure placement since this one gone
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