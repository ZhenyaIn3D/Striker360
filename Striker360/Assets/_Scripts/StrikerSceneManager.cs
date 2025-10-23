using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

/// <summary>
/// Manages the Desert Tank scene, including user interaction and scene state
/// Compatible with OpenXR and Meta XR Core SDK
/// </summary>
public class StrikerSceneManager : MonoBehaviour
{
    [Header("Tank Reference")]
    [SerializeField] private GameObject tankModel;
    
    [Header("Test Settings")]
    [SerializeField] private bool triggerVideoOnStart = false;
    [SerializeField] private float delayBeforeTrigger = 3f;
    
    [Header("Controller Button Trigger")]
    [SerializeField] private bool enableButtonTrigger = true;
    [SerializeField] private bool useRightController = true; // Use right controller for A button
    [SerializeField] private bool debugInput = true;
    
    private InputDevice rightController;
    private InputDevice leftController;
    private bool videoTriggered = false;
    private bool controllersInitialized = false;
    
    private void Start()
    {
        Debug.Log("Desert Tank Scene Started");
        
        // Validate tank model reference
        if (tankModel == null)
        {
            Debug.LogWarning("Tank model is not assigned in TankSceneManager!");
        }
        
        // Test trigger if enabled
        if (triggerVideoOnStart)
        {
            Invoke(nameof(TriggerVideo), delayBeforeTrigger);
        }
        
        // Initialize controllers
        if (enableButtonTrigger)
        {
            InitializeControllers();
        }
    }
    
    private void Update()
    {
        // Check for controller button press to trigger video
        if (enableButtonTrigger && !videoTriggered)
        {
            CheckForButtonPress();
        }
    }
    
    /// <summary>
    /// Initialize the VR controllers for OpenXR
    /// </summary>
    private void InitializeControllers()
    {
        List<InputDevice> devices = new List<InputDevice>();
        
        // Get right controller
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, 
            devices);
        
        if (devices.Count > 0)
        {
            rightController = devices[0];
            if (debugInput)
            {
                Debug.Log($"Right Controller found: {rightController.name}");
            }
        }
        
        // Get left controller
        devices.Clear();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller, 
            devices);
        
        if (devices.Count > 0)
        {
            leftController = devices[0];
            if (debugInput)
            {
                Debug.Log($"Left Controller found: {leftController.name}");
            }
        }
        
        controllersInitialized = (rightController.isValid || leftController.isValid);
        
        if (!controllersInitialized)
        {
            Debug.LogWarning("No controllers found! Will retry initialization.");
        }
    }
    
    /// <summary>
    /// Check for button press on the controller
    /// OpenXR uses CommonUsages for button inputs
    /// </summary>
    private void CheckForButtonPress()
    {
        if (!controllersInitialized)
        {
            InitializeControllers();
            return;
        }
        
        InputDevice targetController = useRightController ? rightController : leftController;
        
        if (!targetController.isValid)
        {
            InitializeControllers();
            return;
        }
        
        // Check for primary button (A button on right controller, X on left)
        if (targetController.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButtonValue) && primaryButtonValue)
        {
            if (debugInput)
            {
                Debug.Log($"Primary button pressed on {(useRightController ? "right" : "left")} controller");
            }
            TriggerVideo();
            return;
        }
        
        // Alternative: Check trigger button as backup
        if (targetController.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerButtonValue) && triggerButtonValue)
        {
            if (debugInput)
            {
                Debug.Log($"Trigger button pressed on {(useRightController ? "right" : "left")} controller");
            }
            TriggerVideo();
            return;
        }
        
        // Alternative: Check grip button as backup
        if (targetController.TryGetFeatureValue(CommonUsages.gripButton, out bool gripButtonValue) && gripButtonValue)
        {
            if (debugInput)
            {
                Debug.Log($"Grip button pressed on {(useRightController ? "right" : "left")} controller");
            }
            TriggerVideo();
            return;
        }
    }
    
    /// <summary>
    /// Public method to trigger the video transition
    /// Can be called from UI buttons or other scripts
    /// </summary>
    public void TriggerVideo()
    {
        if (!videoTriggered)
        {
            videoTriggered = true;
            Debug.Log("Triggering PlayVideoCalled event");
            //SceneTransitionManager.TriggerPlayVideo();
        }
    }
    
    /// <summary>
    /// Example: Rotate tank for demonstration
    /// Call this from Update() if you want the tank to rotate
    /// </summary>
    public void RotateTank(float speed = 30f)
    {
        if (tankModel != null)
        {
            tankModel.transform.Rotate(Vector3.up, speed * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// Reset the video trigger flag (useful for testing)
    /// </summary>
    public void ResetVideoTrigger()
    {
        videoTriggered = false;
        Debug.Log("Video trigger reset");
    }
}