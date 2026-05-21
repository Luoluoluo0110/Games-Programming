using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Makes a celestial body clickable and "alive" for kids:
///   - Hover : the body gently grows so a child knows it can be clicked.
///   - Click : plays a sound, flashes an emissive glow, spawns an optional
///             particle burst, and asks the GameManager to show its fun fact.
///
/// Requires a Collider so the mouse can detect it.
/// Compatible with Unity 2022.3.62f1.
/// </summary>
[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class InteractableBody : MonoBehaviour
{
    [Header("Kid-Friendly Fact (shown in the pop-up)")]
    [Tooltip("The friendly name shown as the pop-up title.")]
    public string displayName = "Planet";

    [TextArea(2, 5)]
    [Tooltip("One short, simple, fun fact a young child can understand.")]
    public string funFact = "I am a planet in our Solar System!";

    [Header("Camera")]
    [Tooltip("How close the camera zooms to this body. " +
             "0 = use the camera's default focus distance.")]
    public float cameraFocusDistance = 0f;

    [Header("Hover Effect")]
    [Tooltip("How much bigger the body grows while the mouse is over it.")]
    public float hoverScaleMultiplier = 1.12f;

    [Tooltip("How quickly the body grows / shrinks.")]
    public float hoverLerpSpeed = 8f;

    [Header("Click Feedback - Glow")]
    [Tooltip("Colour the body flashes with when it is clicked.")]
    public Color clickGlowColor = new Color(1f, 0.95f, 0.6f);

    [Tooltip("How long the glow flash lasts, in seconds.")]
    public float glowPulseDuration = 0.6f;

    [Header("Click Feedback - Sound (optional)")]
    [Tooltip("Sound played when this body is clicked. Optional.")]
    public AudioClip clickSound;

    [Range(0f, 1f)]
    public float clickSoundVolume = 0.8f;

    [Tooltip("Pitch of the click sound. Higher = a brighter, shorter feel.")]
    public float clickSoundPitch = 1f;

    [Tooltip("Trim the click sound to this many seconds (0 = play the whole clip).")]
    public float clickSoundMaxLength = 0f;

    [Header("Click Feedback - Particle Burst (optional Prefab)")]
    [Tooltip("A particle-effect Prefab spawned at the body when clicked. Optional.")]
    public GameObject clickBurstPrefab;

    [Tooltip("Seconds before the spawned burst is cleaned up.")]
    public float clickBurstLifetime = 2f;

    // --- internal state ---
    Vector3 baseScale;          // the body's normal size
    Vector3 targetScale;        // the size the body is gliding toward
    Renderer bodyRenderer;
    Material bodyMaterial;      // a unique material instance for this body
    Color baseEmission;         // the body's resting glow colour
    bool hasEmission;
    float glowTimer;
    AudioSource audioSource;

    void Awake()
    {
        baseScale = transform.localScale;
        targetScale = baseScale;

        // Cache a material INSTANCE so glowing affects only THIS body.
        bodyRenderer = GetComponentInChildren<Renderer>();
        if (bodyRenderer != null)
        {
            bodyMaterial = bodyRenderer.material; // .material returns a unique instance
            hasEmission = bodyMaterial.HasProperty("_EmissionColor");
            if (hasEmission)
            {
                bodyMaterial.EnableKeyword("_EMISSION");
                baseEmission = bodyMaterial.GetColor("_EmissionColor");
            }
        }

        // Use a DEDICATED AudioSource so click sounds never disturb an ambient
        // loop on the same object (for example the Sun's looping fire sound).
        if (clickSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D so the child always hears it clearly
        }
    }

    void Update()
    {
        // Smoothly grow / shrink toward the current hover target size.
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale,
                                            hoverLerpSpeed * Time.deltaTime);

        // Fade the click glow back down to normal over time.
        if (glowTimer > 0f && hasEmission)
        {
            glowTimer -= Time.deltaTime;
            float t = Mathf.Clamp01(glowTimer / glowPulseDuration);
            bodyMaterial.SetColor("_EmissionColor",
                                  Color.Lerp(baseEmission, clickGlowColor, t));
        }
    }

    // ----- Mouse events (Unity calls these automatically on objects with a Collider) -----

    void OnMouseEnter()
    {
        if (IsPointerOverUI()) return;
        targetScale = baseScale * hoverScaleMultiplier; // grow a little
    }

    void OnMouseExit()
    {
        targetScale = baseScale; // shrink back to normal
    }

    void OnMouseDown()
    {
        if (IsPointerOverUI()) return; // ignore clicks that land on the UI panel

        PlayClickFeedback();

        if (GameManager.Instance != null)
            GameManager.Instance.FocusOnBody(this);
    }

    /// <summary>Plays the sound, glow flash and particle burst for one click.</summary>
    void PlayClickFeedback()
    {
        // 1. Glow flash (Materials concept).
        glowTimer = glowPulseDuration;

        // 2. Sound (Audio concept).
        if (clickSound != null && audioSource != null)
        {
            audioSource.pitch = clickSoundPitch;
            audioSource.volume = clickSoundVolume;
            audioSource.clip = clickSound;
            audioSource.Play();
            if (clickSoundMaxLength > 0f)
            {
                CancelInvoke(nameof(StopClickSound));
                Invoke(nameof(StopClickSound), clickSoundMaxLength);
            }
        }

        // 3. Particle burst built from a Prefab (Prefabs concept).
        if (clickBurstPrefab != null)
        {
            GameObject burst = Instantiate(clickBurstPrefab,
                                           transform.position,
                                           Quaternion.identity);
            Destroy(burst, clickBurstLifetime);
        }
    }

    void StopClickSound()
    {
        if (audioSource != null) audioSource.Stop();
    }

    /// <summary>True when the mouse is over a UI element (so we should ignore the click).</summary>
    static bool IsPointerOverUI()
    {
        return EventSystem.current != null &&
               EventSystem.current.IsPointerOverGameObject();
    }
}
