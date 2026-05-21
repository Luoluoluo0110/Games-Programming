using UnityEngine;

/// <summary>
/// Smoothly moves the Main Camera between two states:
///   - Overview : the wide "home" shot of the whole Solar System.
///   - Focus    : zoomed in on the one body the player just clicked.
///
/// Every movement uses Lerp / Slerp so the camera GLIDES instead of snapping,
/// and it tracks an orbiting body without jitter (work is done in LateUpdate).
/// Compatible with Unity 2022.3.62f1.
/// </summary>
[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
public class CameraController : MonoBehaviour
{
    [Header("Overview (home) View")]
    [Tooltip("Optional empty Transform marking the home camera pose. " +
             "If left empty, the camera's pose at start-up is used.")]
    public Transform overviewAnchor;

    [Tooltip("Field of View used for the wide overview shot.")]
    public float overviewFOV = 60f;

    [Header("Focus View")]
    [Tooltip("Default distance the camera sits from a focused body.")]
    public float focusDistance = 6f;

    [Tooltip("How far above the body the camera sits, for a pleasant angle.")]
    public float focusHeight = 2f;

    [Tooltip("Field of View used when zoomed in on a body (smaller = closer).")]
    public float focusFOV = 35f;

    [Header("Smoothing Speeds")]
    public float moveLerpSpeed = 3f;
    public float rotateSlerpSpeed = 3f;
    public float fovLerpSpeed = 3f;

    Camera cam;
    Vector3 homePosition;
    Quaternion homeRotation;
    Transform focusTarget;
    float activeFocusDistance;
    bool isFocusing;

    void Awake()
    {
        cam = GetComponent<Camera>();
        activeFocusDistance = focusDistance;

        // Remember the home pose so we can always glide back to it.
        if (overviewAnchor != null)
        {
            homePosition = overviewAnchor.position;
            homeRotation = overviewAnchor.rotation;
        }
        else
        {
            homePosition = transform.position;
            homeRotation = transform.rotation;
        }
    }

    /// <summary>Begin gliding toward and zooming in on the given body.</summary>
    public void FocusOn(Transform target, float distanceOverride = 0f)
    {
        focusTarget = target;
        activeFocusDistance = (distanceOverride > 0f) ? distanceOverride : focusDistance;
        isFocusing = true;
    }

    /// <summary>Glide back to the wide overview shot.</summary>
    public void ReturnToOverview()
    {
        focusTarget = null;
        isFocusing = false;
    }

    // LateUpdate runs AFTER bodies have moved this frame, so the camera
    // tracks an orbiting body perfectly with no jitter.
    void LateUpdate()
    {
        Vector3 desiredPosition;
        Quaternion desiredRotation;
        float desiredFOV;

        if (isFocusing && focusTarget != null)
        {
            // Sit back-and-up from the body, along the line from home to the body.
            Vector3 directionFromHome = focusTarget.position - homePosition;
            if (directionFromHome.sqrMagnitude < 0.001f)
                directionFromHome = -transform.forward; // safety fallback
            directionFromHome.Normalize();

            desiredPosition = focusTarget.position
                              - directionFromHome * activeFocusDistance
                              + Vector3.up * focusHeight;
            desiredRotation = Quaternion.LookRotation(focusTarget.position - desiredPosition);
            desiredFOV = focusFOV;
        }
        else
        {
            desiredPosition = homePosition;
            desiredRotation = homeRotation;
            desiredFOV = overviewFOV;
        }

        // Smooth glide - the key polish requirement (no instant snapping).
        transform.position = Vector3.Lerp(transform.position, desiredPosition,
                                          moveLerpSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation,
                                              rotateSlerpSpeed * Time.deltaTime);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, desiredFOV,
                                     fovLerpSpeed * Time.deltaTime);
    }
}
