using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("AR Components")]
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;
    
    [Header("Game Objects")]
    public GameObject blockPrefab;
    public GameObject ghostBlockPrefab;
    public Transform blocksParent;
    
    [Header("UI Components")]
    public GameObject mainMenuPanel;
    public GameObject gameplayPanel;
    public GameObject gameOverPanel;
    public GameObject settingsPanel;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI comboText;
    public Slider timerSlider;
    public Toggle soundToggle;
    
    [Header("Game Settings")]
    public float placeDelay = 0.5f;
    public float initialTimerValue = 10f;
    public float comboTimerBonus = 2f;
    public int comboThreshold = 5;
    
    // Game state variables
    private bool isGameActive = false;
    private bool canPlace = false;
    private int score = 0;
    private int comboCount = 0;
    private float timer;
    private bool isSoundOn = true;
    private GameObject currentGhostBlock;
    private List<GameObject> placedBlocks = new List<GameObject>();
    private bool gameStarted = false;
    private Vector3 lastPlacedPosition;
    
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
    
    private void Start()
    {
        // Initialize game state
        ShowMainMenu();
    }
    
    private void Update()
    {
        if (!isGameActive) return;
        
        // Update timer
        if (gameStarted)
        {
            timer -= Time.deltaTime;
            timerSlider.value = timer / initialTimerValue;
            timerText.text = timer.ToString("F1");
            
            if (timer <= 0)
            {
                GameOver();
            }
        }
        
        // Update ghost block position using raycast
        UpdateGhostBlock();
    }
    
    private void UpdateGhostBlock()
    {
        if (currentGhostBlock == null || !canPlace) return;
        
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2);
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);
        RaycastHit hit;
        List<ARRaycastHit> arHits = new List<ARRaycastHit>();
        bool hasValidPosition = false;
        
        // First, try to raycast against existing blocks
        if (gameStarted && placedBlocks.Count > 0 && Physics.Raycast(ray, out hit))
        {
            // Check if we hit any game object (not just the last placed block)
            GameObject hitObject = hit.collider.gameObject;
            
            // If it's one of our placed blocks, position ghost on top of it
            if (placedBlocks.Contains(hitObject))
            {
                // Position ghost block at the hit point, not directly on top
                // This gives player more control over placement
                Vector3 topPosition = hit.point + new Vector3(0, blockPrefab.transform.localScale.y / 2, 0);
                currentGhostBlock.transform.position = topPosition;
                hasValidPosition = true;
            }
        }
        
        // If no block was hit OR if we want to allow placing on any surface, fall back to AR raycast
        if (!hasValidPosition && raycastManager.Raycast(screenCenter, arHits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = arHits[0].pose;
            currentGhostBlock.transform.position = hitPose.position;
        }
    }
    
    // Game flow methods
    public void StartGame()
    {
        mainMenuPanel.SetActive(false);
        gameplayPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        settingsPanel.SetActive(false);
        
        // Reset game state
        score = 0;
        comboCount = 0;
        timer = initialTimerValue;
        isGameActive = true;
        gameStarted = false;
        
        // Clear any existing blocks
        foreach (GameObject block in placedBlocks)
        {
            Destroy(block);
        }
        placedBlocks.Clear();
        
        // Create ghost block
        if (currentGhostBlock != null)
        {
            Destroy(currentGhostBlock);
        }
        currentGhostBlock = Instantiate(ghostBlockPrefab);
        
        // Update UI
        UpdateScoreUI();
        UpdateComboUI();
        
        // Enable AR systems
        planeManager.enabled = true;
        canPlace = true;
    }
    
    public void GameOver()
    {
        isGameActive = false;
        gameStarted = false;
        canPlace = false;
        
        gameplayPanel.SetActive(false);
        gameOverPanel.SetActive(true);
        
        // Display final score on game over panel
        GameObject.Find("FinalScoreText").GetComponent<TextMeshProUGUI>().text = "Score: " + score;
        
        // Hide ghost block
        if (currentGhostBlock != null)
        {
            currentGhostBlock.SetActive(false);
        }
    }
    
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        gameplayPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        settingsPanel.SetActive(false);
        
        // Disable AR systems when in menu
        if (planeManager != null)
        {
            planeManager.enabled = false;
        }
    }
    
    public void ShowSettings()
    {
        settingsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        
        // Initialize sound toggle
        soundToggle.isOn = isSoundOn;
    }
    
    public void BackToMainMenu()
    {
        ShowMainMenu();
    }
    
    public void RestartGame()
    {
        StartGame();
    }
    
    public void ToggleSound(bool isOn)
    {
        isSoundOn = isOn;
        AudioListener.volume = isOn ? 1 : 0;
    }
    
    public void PlaceBlock()
    {
        if (!canPlace || currentGhostBlock == null) return;
        
        // Temporarily disable placing
        canPlace = false;
        
        // Create real block at ghost position
        GameObject newBlock = Instantiate(blockPrefab, currentGhostBlock.transform.position, Quaternion.identity, blocksParent);
        
        // Start game on first block placement
        if (!gameStarted)
        {
            gameStarted = true;
            lastPlacedPosition = newBlock.transform.position;
            placedBlocks.Add(newBlock);
        }
        else
        {
            // Check if block is properly stacked (on top of any existing block)
            bool isProperlyStacked = false;
            
            // Use a short downward raycast from the new block to check if it's above another block
            Ray downRay = new Ray(newBlock.transform.position, Vector3.down);
            RaycastHit hitDown;
            
            if (Physics.Raycast(downRay, out hitDown, blockPrefab.transform.localScale.y * 1.5f))
            {
                if (placedBlocks.Contains(hitDown.collider.gameObject))
                {
                    isProperlyStacked = true;
                }
            }
            
            if (!isProperlyStacked)
            {
                // Block is misplaced, handle falling
                StartCoroutine(BlockFalling(newBlock));
                // Don't add to placedBlocks list
                return;
            }
            
            // Block is properly stacked
            placedBlocks.Add(newBlock);
            
            // Update last position for next comparison
            lastPlacedPosition = newBlock.transform.position;
        }
        
        // Make sure the block is frozen in place
        BlockBehavior blockBehavior = newBlock.GetComponent<BlockBehavior>();
        if (blockBehavior != null)
        {
            blockBehavior.FreezeBlock();
        }
        
        // Increment score and combo
        score++;
        comboCount++;
        
        // Check for combo
        if (comboCount >= comboThreshold)
        {
            // Add bonus time
            timer += comboTimerBonus;
            if (timer > initialTimerValue)
            {
                timer = initialTimerValue;
            }
            
            // Remove the 4 most recently placed blocks (excluding the newest one)
            // We keep the newest block (which just completed the combo) and remove the previous 4
            if (placedBlocks.Count >= comboThreshold)
            {
                // Keep track of blocks to remove
                List<GameObject> blocksToRemove = new List<GameObject>();
                
                // Get the 4 blocks before the newest one
                for (int i = placedBlocks.Count - comboThreshold; i < placedBlocks.Count - 1; i++)
                {
                    blocksToRemove.Add(placedBlocks[i]);
                }
                
                // Remove them from the list and destroy them with visual effect
                foreach (GameObject block in blocksToRemove)
                {
                    placedBlocks.Remove(block);
                    StartCoroutine(DestroyBlockWithEffect(block));
                }
            }
            
            // Reset combo
            comboCount = 0;
            
            // Visual effect for combo
            StartCoroutine(ComboEffect());
        }
        
        // Update UI
        UpdateScoreUI();
        UpdateComboUI();
        
        // Re-enable placing after delay
        StartCoroutine(EnablePlacingAfterDelay());
    }
    
    private IEnumerator EnablePlacingAfterDelay()
    {
        yield return new WaitForSeconds(placeDelay);
        canPlace = true;
    }
    
    private IEnumerator BlockFalling(GameObject block)
    {
        // Get rigidbody component or add one if it doesn't exist
        Rigidbody rb = block.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = block.AddComponent<Rigidbody>();
        }
        
        // Make sure this block is not in our placed blocks list
        if (placedBlocks.Contains(block))
        {
            placedBlocks.Remove(block);
        }
        
        // Enable physics
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;
        
        // Wait for block to fall
        yield return new WaitForSeconds(2f);
        
        // Game over
        GameOver();
    }
    
    private IEnumerator DestroyBlockWithEffect(GameObject block)
    {
        // Add visual effect before destroying
        Renderer renderer = block.GetComponent<Renderer>();
        Material originalMaterial = renderer.material;
        Material flashMaterial = new Material(originalMaterial);
        flashMaterial.color = Color.yellow;
        
        // Flash effect
        for (int i = 0; i < 3; i++)
        {
            renderer.material = flashMaterial;
            yield return new WaitForSeconds(0.05f);
            renderer.material = originalMaterial;
            yield return new WaitForSeconds(0.05f);
        }
        
        // Scale down effect
        Vector3 originalScale = block.transform.localScale;
        float duration = 0.3f;
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            block.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            yield return null;
        }
        
        // Destroy the block
        Destroy(block);
    }
    
    private IEnumerator ComboEffect()
    {
        // Simple combo effect - flash the combo text
        for (int i = 0; i < 3; i++)
        {
            comboText.color = Color.yellow;
            yield return new WaitForSeconds(0.1f);
            comboText.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
        
        // Show combo message
        StartCoroutine(ShowTemporaryMessage("COMBO! +Time"));
    }
    
    private IEnumerator ShowTemporaryMessage(string message)
    {
        // Create temporary text at center of screen
        GameObject tempTextObj = new GameObject("TempMessage");
        tempTextObj.transform.SetParent(gameplayPanel.transform, false);
        TextMeshProUGUI tempText = tempTextObj.AddComponent<TextMeshProUGUI>();
        
        // Set text properties
        tempText.text = message;
        tempText.fontSize = 36;
        tempText.color = Color.yellow;
        tempText.alignment = TextAlignmentOptions.Center;
        
        // Position in center of screen
        RectTransform rectTransform = tempTextObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(300, 100);
        
        // Animate
        float duration = 1.5f;
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1 - (elapsed / duration);
            tempText.color = new Color(tempText.color.r, tempText.color.g, tempText.color.b, alpha);
            rectTransform.anchoredPosition = new Vector2(0, 100 + elapsed * 50);
            yield return null;
        }
        
        // Destroy temporary text
        Destroy(tempTextObj);
    }
    
    private void UpdateScoreUI()
    {
        scoreText.text = "Score: " + score;
    }
    
    private void UpdateComboUI()
    {
        comboText.text = "Combo: " + comboCount + "/" + comboThreshold;
    }
}