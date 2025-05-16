using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    // References to GameManager
    private GameManager gameManager;
    
    private void Start()
    {
        gameManager = GameManager.Instance;
        
        // Set up button listeners
        SetupButtonListeners();
    }
    
    private void SetupButtonListeners()
    {
        // Main Menu Buttons
        GameObject.Find("StartButton").GetComponent<Button>().onClick.AddListener(gameManager.StartGame);
        GameObject.Find("SettingsButton").GetComponent<Button>().onClick.AddListener(gameManager.ShowSettings);
        
        // Gameplay Buttons
        GameObject.Find("PlaceButton").GetComponent<Button>().onClick.AddListener(gameManager.PlaceBlock);
        
        // Game Over Buttons
        GameObject.Find("RestartButton").GetComponent<Button>().onClick.AddListener(gameManager.RestartGame);
        GameObject.Find("MenuButton").GetComponent<Button>().onClick.AddListener(gameManager.BackToMainMenu);
        
        // Settings Buttons
        GameObject.Find("BackButton").GetComponent<Button>().onClick.AddListener(gameManager.BackToMainMenu);
        GameObject.Find("SoundToggle").GetComponent<Toggle>().onValueChanged.AddListener(gameManager.ToggleSound);
    }
}