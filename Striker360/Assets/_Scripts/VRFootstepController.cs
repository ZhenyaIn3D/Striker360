// VRFootstepController.cs
// Attach to XR Origin
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRFootstepController : MonoBehaviour
{
    [Header("Movement Detection")]
    public Transform headTransform; // Main Camera
    private Vector3 lastPosition;
    private float distanceTraveled = 0f;
    
    [Header("Footstep Settings")]
    public float stepDistance = 1.5f; // Distance in meters before playing footstep
    public AudioClip[] sandFootsteps; // Array of sand footstep sounds
    
    [Header("Audio")]
    private AudioSource audioSource;
    public float volume = 0.4f;
    public float pitchVariation = 0.1f;
    
    void Start()
    {
        if (headTransform == null)
        {
            headTransform = Camera.main.transform;
        }
        
        lastPosition = headTransform.position;
        
        // Setup audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f; // 2D sound (attached to player)
        audioSource.volume = volume;
        
        // If no footstep sounds assigned, generate procedural sounds
        if (sandFootsteps == null || sandFootsteps.Length == 0)
        {
            Debug.LogWarning("No footstep sounds assigned. Consider adding sand footstep audio clips.");
        }
    }
    
    void Update()
    {
        // Calculate horizontal movement only (ignore vertical head bob)
        Vector3 currentPos = new Vector3(headTransform.position.x, 0, headTransform.position.z);
        Vector3 lastPos = new Vector3(lastPosition.x, 0, lastPosition.z);
        
        float distance = Vector3.Distance(currentPos, lastPos);
        distanceTraveled += distance;
        
        // Play footstep when threshold reached
        if (distanceTraveled >= stepDistance)
        {
            PlayFootstep();
            distanceTraveled = 0f;
        }
        
        lastPosition = headTransform.position;
    }
    
    void PlayFootstep()
    {
        if (sandFootsteps != null && sandFootsteps.Length > 0)
        {
            // Play random footstep sound
            AudioClip clip = sandFootsteps[Random.Range(0, sandFootsteps.Length)];
            audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            audioSource.PlayOneShot(clip);
        }
        else
        {
            // Generate procedural footstep sound if no clips assigned
            GenerateProceduralFootstep();
        }
    }
    
    void GenerateProceduralFootstep()
    {
        // Create a simple procedural footstep sound
        // Note: For better results, use actual audio clips
        AudioClip footstep = AudioClip.Create("ProceduralFootstep", 4410, 1, 44100, false);
        float[] samples = new float[4410];
        
        for (int i = 0; i < samples.Length; i++)
        {
            // Simple noise with envelope
            float envelope = Mathf.Exp(-i / 2000f);
            samples[i] = Random.Range(-0.3f, 0.3f) * envelope;
        }
        
        footstep.SetData(samples, 0);
        audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        audioSource.PlayOneShot(footstep);
    }
}