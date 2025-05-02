using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using TMPro;

public class ARBlockStackingGame : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;
    
    [Header("Game Objects")]
    [SerializeField] private GameObject concreteBasePrefab; // Concrete base block
    [SerializeField] private GameObject woodenBoxPrefab;    // Wooden box prefab for stacking
    [SerializeField] private Transform blockContainer;
    [SerializeField] private float blockSize = 0.1f; // Size of each cube
    
    [Header("UI Elements")]
    [SerializeField] private Button placeButton;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Button restartButton;
    
    // Game state
    private List<GameObject> placedBlocks = new List<GameObject>();
    private GameObject currentBlock;
    private bool gameStarted = false;
    private bool gameOver = false;
    private int score = 0;
    private Vector3 lastPlacedPosition;
    private bool surfaceDetected = false;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    // Starting position for the first block
    private Vector3 firstBlockPosition;
    
    void Start()
    {
        // Initialize UI
        if (placeButton != null)
            placeButton.onClick.AddListener(PlaceBlock);
            
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
            
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        // Create block container if not assigned
        if (blockContainer == null)
        {
            GameObject container = new GameObject("BlockContainer");
            blockContainer = container.transform;
        }
    }
    
    void Update()
    {
        // Only detect surfaces before game starts
        if (!gameStarted)
        {
            DetectSurface();
        }
        
        // Check for fallen blocks once game has started
        if (gameStarted && !gameOver)
        {
            CheckForFallenBlocks();
        }
    }
    
    void DetectSurface()
    {
        // Cast a ray from the center of the screen
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        
        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            // Surface detected
            surfaceDetected = true;
            
            // Visual indication that surface is detected
            if (placeButton != null)
                placeButton.interactable = true;
                
            // Preview the first block position
            Pose hitPose = hits[0].pose;
            firstBlockPosition = hitPose.position + new Vector3(0, blockSize / 2, 0); // Position on surface plus half height
        }
        else
        {
            surfaceDetected = false;
            if (placeButton != null)
                placeButton.interactable = false;
        }
    }
    
    public void PlaceBlock()
    {
        if (!gameStarted)
        {
            // Start the game with first block
            StartGame();
        }
        else if (!gameOver)
        {
            // Place next block
            SpawnNextBlock();
        }
    }
    
    void StartGame()
    {
        if (!surfaceDetected) return;
        
        // Hide AR planes after game starts
        if (planeManager != null)
        {
            planeManager.enabled = false;
            foreach (var plane in planeManager.trackables)
            {
                plane.gameObject.SetActive(false);
            }
        }
        
        // Create concrete base first
        currentBlock = Instantiate(concreteBasePrefab, firstBlockPosition, Quaternion.identity, blockContainer);
        
        // Set up Rigidbody for physics
        Rigidbody rb = currentBlock.GetComponent<Rigidbody>();
        if (rb == null) rb = currentBlock.AddComponent<Rigidbody>();
        rb.isKinematic = true; // Concrete base is static (won't move)
        
        placedBlocks.Add(currentBlock);
        lastPlacedPosition = firstBlockPosition;
        
        // Update UI and game state
        gameStarted = true;
        UpdateScore(0);
        
        // Change the button text to "Stack Box"
        if (placeButton != null && placeButton.GetComponentInChildren<TextMeshProUGUI>() != null)
        {
            placeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Stack Box";
        }
    }
    
    void SpawnNextBlock()
    {
        // Calculate position for next block (stacked on top of last one)
        Vector3 newPosition = lastPlacedPosition + new Vector3(0, blockSize, 0);
        
        // Create new wooden box
        currentBlock = Instantiate(woodenBoxPrefab, newPosition, Quaternion.identity, blockContainer);
        
        // Set up physics
        Rigidbody rb = currentBlock.GetComponent<Rigidbody>();
        if (rb == null) rb = currentBlock.AddComponent<Rigidbody>();
        rb.isKinematic = false; // Wooden boxes will be affected by physics
        
        // Add to our list
        placedBlocks.Add(currentBlock);
        lastPlacedPosition = newPosition;
        
        // Increment score
        UpdateScore(++score);
    }
    
    void CheckForFallenBlocks()
    {
        // Check if any block fell below the surface level (accounting for block size)
        float surfaceY = firstBlockPosition.y - (blockSize / 2) - 0.05f; // A little threshold
        
        foreach (GameObject block in placedBlocks)
        {
            if (block != null && block.transform.position.y < surfaceY)
            {
                GameOver();
                return;
            }
        }
    }
    
    void GameOver()
    {
        gameOver = true;
        
        // Show game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            // Update final score display if there's a text component for it
            TextMeshProUGUI finalScoreText = gameOverPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (finalScoreText != null)
            {
                finalScoreText.text = "Final Score: " + score;
            }
        }
        
        // Disable place button
        if (placeButton != null)
            placeButton.gameObject.SetActive(false);
    }
    
    void RestartGame()
    {
        // Clean up existing blocks
        foreach (GameObject block in placedBlocks)
        {
            Destroy(block);
        }
        placedBlocks.Clear();
        
        // Reset game state
        gameStarted = false;
        gameOver = false;
        score = 0;
        
        // Re-enable AR plane detection
        if (planeManager != null)
        {
            planeManager.enabled = true;
        }
        
        // Reset UI
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        if (placeButton != null)
        {
            placeButton.gameObject.SetActive(true);
            placeButton.interactable = false;
            
            // Reset button text
            if (placeButton.GetComponentInChildren<TextMeshProUGUI>() != null)
            {
                placeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Place";
            }
        }
        
        UpdateScore(0);
    }
    
    void UpdateScore(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + newScore;
        }
    }
}