
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.XR;
using System.Collections.Generic;

/// <summary>
/// Manages 360° video playback with headset detection for play/pause control
/// Video stops when headset is removed and restarts from beginning when put back on
/// Compatible with OpenXR and Meta XR Core SDK
/// </summary>
public class Video360Manager : MonoBehaviour
{
    [Header("Video Settings")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private VideoClip video360Clip;
    [SerializeField] private GameObject videoSphere;
    
    [Header("Headset Detection")]
    [SerializeField] private float checkInterval = 0.5f; // Check every 0.5 seconds
    [SerializeField] private bool debugMode = true;
    public GameObject HMDFound;
    public GameObject HMDNotFound;
    [Header("Return to Desert Scene")]
    [SerializeField] private bool returnAfterVideoEnds = true;
    [SerializeField] private float delayBeforeReturn = 2f;
    
    private bool isHeadsetWorn = true;
    private bool wasHeadsetWorn = true;
    private float nextCheckTime = 0f;
    
    private InputDevice hmd;
    private bool hmdInitialized = false;
    
    private void Start()
    {
        Debug.Log("360 Video Scene Started");
        
        InitializeVideoPlayer();
        InitializeHMD();
        
        // Start playing the video immediately
        PlayVideo();
    }
    
    private void Update()
    {
        // Check headset status periodically
        if (Time.time >= nextCheckTime)
        {
            CheckHeadsetStatus();
            nextCheckTime = Time.time + checkInterval;
        }
        
        // Handle video end
        // if (returnAfterVideoEnds && videoPlayer != null && !videoPlayer.isPlaying && videoPlayer.frame > 0)
        // {
        //     Invoke(nameof(ReturnToDesertScene), delayBeforeReturn);
        // }
    }
    
    /// <summary>
    /// Initialize the video player component
    /// </summary>
    private void InitializeVideoPlayer()
    {
        // If VideoPlayer is not assigned, try to get it from this GameObject
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
            
            if (videoPlayer == null)
            {
                Debug.LogError("VideoPlayer component not found! Please assign it in the inspector.");
                return;
            }
        }
        
        // Configure video player
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
        
        // Assign video clip
        if (video360Clip != null)
        {
            videoPlayer.clip = video360Clip;
        }
        else
        {
            Debug.LogWarning("360 Video Clip is not assigned!");
        }
        
        // Assign to sphere material if available
        if (videoSphere != null)
        {
            Renderer renderer = videoSphere.GetComponent<Renderer>();
            if (renderer != null)
            {
                videoPlayer.targetMaterialRenderer = renderer;
                videoPlayer.targetMaterialProperty = "_MainTex";
            }
        }
        
        // Subscribe to video events
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnVideoEnded;
    }
    
    /// <summary>
    /// Initialize the Head-Mounted Display (HMD) device for OpenXR
    /// </summary>
    private void InitializeHMD()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, devices);
        
        if (devices.Count > 0)
        {
            hmd = devices[0];
            hmdInitialized = true;
            HMDFound.SetActive(true);
            if (debugMode)
            {
                Debug.Log($"HMD Initialized: {hmd.name}");
                
                // Log available features for debugging
                List<InputFeatureUsage> features = new List<InputFeatureUsage>();
                if (hmd.TryGetFeatureUsages(features))
                {
                    Debug.Log($"Available HMD features: {features.Count}");
                    foreach (var feature in features)
                    {
                        Debug.Log($"  - {feature.name} ({feature.type})");
                    }
                }
            }
        }
        else
        {
            HMDNotFound.SetActive(true);
            Debug.LogWarning("No HMD device found! Will retry on next check.");
        }
    }
    
    /// <summary>
    /// Check if the headset is currently being worn
    /// Uses OpenXR's userPresence feature
    /// </summary>
    private void CheckHeadsetStatus()
    {
        if (!hmdInitialized || !hmd.isValid)
        {
            InitializeHMD();
            return;
        }
        
        // Try to get user presence (headset worn status)
        // This works with OpenXR and Meta Quest runtime
        if (hmd.TryGetFeatureValue(CommonUsages.userPresence, out bool userPresent))
        {
            isHeadsetWorn = userPresent;
            
            // Detect state change
            if (isHeadsetWorn != wasHeadsetWorn)
            {
                OnHeadsetStatusChanged(isHeadsetWorn);
                wasHeadsetWorn = isHeadsetWorn;
            }
        }
        else
        {
            // Fallback: If userPresence is not available, try tracking state
            if (hmd.TryGetFeatureValue(CommonUsages.trackingState, out InputTrackingState trackingState))
            {
                bool isTracking = (trackingState & InputTrackingState.Position) != 0 || 
                                  (trackingState & InputTrackingState.Rotation) != 0;
                
                if (isTracking != isHeadsetWorn)
                {
                    isHeadsetWorn = isTracking;
                    OnHeadsetStatusChanged(isHeadsetWorn);
                    wasHeadsetWorn = isHeadsetWorn;
                }
            }
            else if (debugMode)
            {
                Debug.LogWarning("Could not read userPresence or trackingState from HMD");
            }
        }
    }
    
    /// <summary>
    /// Called when headset worn status changes
    /// </summary>
    private void OnHeadsetStatusChanged(bool isWorn)
    {
        if (debugMode)
        {
            Debug.Log($"Headset status changed: {(isWorn ? "WORN" : "REMOVED")}");
        }
        
        if (isWorn)
        {
            // Headset put back on - restart video from beginning
            RestartVideo();
        }
        else
        {
            // Headset removed - stop video
            StopVideo();
        }
    }
    
    /// <summary>
    /// Start playing the video
    /// </summary>
    private void PlayVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Play();
            
            if (debugMode)
            {
                Debug.Log("Video started playing");
            }
        }
    }
    
    /// <summary>
    /// Stop the video
    /// </summary>
    private void StopVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            
            if (debugMode)
            {
                Debug.Log("Video stopped");
            }
        }
    }
    
    /// <summary>
    /// Restart the video from the beginning
    /// </summary>
    private void RestartVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.time = 0;
            videoPlayer.Play();
            
            if (debugMode)
            {
                Debug.Log("Video restarted from beginning");
            }
        }
    }
    
    /// <summary>
    /// Called when video preparation is complete
    /// </summary>
    private void OnVideoPrepared(VideoPlayer vp)
    {
        if (debugMode)
        {
            Debug.Log("Video prepared and ready to play");
        }
    }
    
    /// <summary>
    /// Called when video reaches the end
    /// </summary>
    private void OnVideoEnded(VideoPlayer vp)
    {
        if (debugMode)
        {
            Debug.Log("Video playback ended");
        }
    }
    
    // /// <summary>
    // /// Return to the desert scene
    // /// </summary>
    // private void ReturnToDesertScene()
    // {
    //     if (SceneTransitionManager.Instance != null)
    //     {
    //         SceneTransitionManager.Instance.LoadDesertScene();
    //     }
    // }
    //
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.loopPointReached -= OnVideoEnded;
        }
    }
}