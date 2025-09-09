using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.InputSystem;


/// <summary>
/// Handles the starting and stopping of the idol minigame
/// </summary>
public class MinigameManager : MonoBehaviour
{

    public static MinigameManager Instance { get; private set; }
    public BoardFlip board;
    public GameObject pawnPrefab;
    public GameObject idolPrefab;
    public bool ending;
    public float spawnCooldown = 1f;
    public float minRotation = -30f;
    public float maxRotation = 30f;
    public bool gameRunning = false;
    public float gameEndDelay = 3f;
    private float lastSpawnTime = 0;
    public Vector3[] edge1Spawnpoints;
    public Vector3[] edge2Spawnpoints;
    public Vector3[] edge3Spawnpoints;
    public Vector3[] edge4Spawnpoints;
    private List<Vector3[]> spawnpoints = new List<Vector3[]>();

    private PlayerInputActions inputActions;

    IdolPoints idolPoints;
    WorshipPoints worshipPoints;

    private void Awake()
    {

        inputActions = new PlayerInputActions();
        if (Instance != null) Debug.LogError("There are two MinigameManagers in your scene!");
        else Instance = this;
    }


    void Start()
    {
        idolPoints = GetComponent<IdolPoints>();
        worshipPoints = GetComponent<WorshipPoints>();

        spawnpoints.Add(edge1Spawnpoints);
        spawnpoints.Add(edge2Spawnpoints);
        spawnpoints.Add(edge3Spawnpoints);
        spawnpoints.Add(edge4Spawnpoints);
        // give a free idol for testing purposes
        //ResourceManager.Instance.AddIdols(1);

    }


    private void OnEnable()
    {
        inputActions.Player.Click.performed += OnClick; // Subscribe to the Click action
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Click.performed -= OnClick; // Unsubscribe to prevent memory leaks
        inputActions.Disable();
    }

    public void StartMinigame()
    {
        if (idolPoints.remainingPoints <= 0)
        {
            Debug.LogWarning("No idols to play minigame with!"); // Replace with UI (popup, or gray out the flip board button)
            return;
        }

        board.FlipBoard();
        
    }

    public void EndMinigame()
    {
        ending = true;
        StartCoroutine(nameof(EndingRoutine));
        
    }


    void Update()
    {
        if (!gameRunning || ending) return;

        if (Time.time >= lastSpawnTime + spawnCooldown) SpawnPawn();
        

    }

    IEnumerator EndingRoutine()
    {
        yield return new WaitForSeconds(gameEndDelay);
        board.FlipBoard();
        ending = false;
    }



    void SpawnPawn()
    {
        int edge = Random.Range(0, 4);
        Vector3[] sp = spawnpoints[edge];
        int s = Random.Range(0, 11);
        Vector3 spawnPos = sp[s];

        GameObject pawn = Instantiate(pawnPrefab);

        pawn.transform.position = spawnPos;

        // Rotate based on edge
        pawn.transform.Rotate(new Vector3(0, (edge + 1) * 90, 0));

        float rot = Random.Range(minRotation, maxRotation);
        pawn.transform.Rotate(new Vector3(0, rot, 0));


        lastSpawnTime = Time.time;
    }

    void SpawnIdol(Vector3 position)
    {
        if (idolPoints.remainingPoints == 0)
        {
            EndMinigame();
        }
        ;
        GameObject go = Instantiate(idolPrefab);
        go.transform.position = position;
        idolPoints.DEDUCT_POINT();
        go.GetComponent<Idol>().Capture();
    }



    public void OnClick(InputAction.CallbackContext context)
    {
        Vector2 mousePos = Vector2.zero;
        mousePos = GameInput.Instance.GetActiveGameDevice() == GameInput.GameDevice.KeyboardMouse ? Mouse.current.position.ReadValue() : VirtualMouseUI.Instance.GetVirtualMousePosition();
        
        // TODO: create main camera ref somewhere for all scripts, GameManager?
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject.CompareTag("BoardBottom"))
            {
                SpawnIdol(hit.point);
            }
        }

        
    }
    
}
