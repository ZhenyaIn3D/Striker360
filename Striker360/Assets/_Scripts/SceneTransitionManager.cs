using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.InputSystem;

/// <summary>
/// Singleton manager for handling scene transitions.
/// This persists across scenes to maintain the event system.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{

    [Header("Input")]
    public InputActionReference buttonBAction;
    
    [Header("Scene Settings")]
    public string targetSceneName = "360VideoScene";
    public bool useSceneIndex = false;
    public int targetSceneIndex = 1;
    
    [Header("Feedback")]
    public AudioClip buttonPressSound;
    private AudioSource audioSource;
    
    void Start()
    {
        // Setup audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0.5f;
        
        // Enable input action
        if (buttonBAction != null)
        {
            buttonBAction.action.Enable();
            buttonBAction.action.performed += OnButtonBPressed;
        }
        else
        {
            Debug.LogError("Button B Action not assigned!");
        }
    }
    
    void OnButtonBPressed(InputAction.CallbackContext context)
    {
        Debug.Log("Button B pressed - Changing scene...");
        
        // Play sound feedback
        if (buttonPressSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonPressSound);
        }
        
        // Change scene
        ChangeScene();
    }
    
    void ChangeScene()
    {
        if (useSceneIndex)
        {
            if (targetSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(targetSceneIndex);
            }
            else
            {
                Debug.LogError($"Scene index {targetSceneIndex} out of range!");
            }
        }
        else
        {
            if (Application.CanStreamedLevelBeLoaded(targetSceneName))
            {
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.LogError($"Scene '{targetSceneName}' not found in Build Settings!");
            }
        }
    }
    
    void OnDestroy()
    {
        if (buttonBAction != null && buttonBAction.action != null)
        {
            buttonBAction.action.performed -= OnButtonBPressed;
            buttonBAction.action.Disable();
        }
    }
}
