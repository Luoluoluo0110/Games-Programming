using UnityEngine;

/// <summary>
/// BONUS feature: a comet that flies across the scene on a straight path,
/// waits off-screen, then loops through again. It is clickable too - add an
/// InteractableBody alongside it. While the player is focused on the comet it
/// pauses its flight so a child has time to read its fun fact.
/// Compatible with Unity 2022.3.62f1.
/// </summary>
[RequireComponent(typeof(InteractableBody))]
[DisallowMultipleComponent]
public class CometController : MonoBehaviour
{
    [Header("Flight Path (world-space points)")]
    [Tooltip("Where the comet enters the scene.")]
    public Vector3 startPoint = new Vector3(-45f, 8f, 25f);

    [Tooltip("Where the comet leaves the scene.")]
    public Vector3 endPoint = new Vector3(45f, -6f, -25f);

    [Tooltip("Flight speed in metres per second.")]
    public float speed = 14f;

    [Tooltip("Seconds to wait off-screen before flying through again.")]
    public float pauseBetweenRuns = 5f;

    [Header("Look")]
    [Tooltip("How fast the comet tumbles, in degrees per second.")]
    public float spinSpeed = 50f;

    InteractableBody body;
    float journey;     // progress along the path, 0..1
    float waitTimer;
    bool waiting;

    void Awake()
    {
        body = GetComponent<InteractableBody>();
    }

    void OnEnable()
    {
        ResetToStart();
    }

    void Update()
    {
        // Always tumble gently so the comet looks lively.
        transform.Rotate(Vector3.one.normalized, spinSpeed * Time.deltaTime, Space.Self);

        // Freeze the flight while the child is reading the comet's fact.
        if (GameManager.Instance != null && GameManager.Instance.IsFocused(body))
            return;

        // Waiting off-screen between runs.
        if (waiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f) ResetToStart();
            return;
        }

        // Advance along the straight path from start to end.
        float pathLength = Vector3.Distance(startPoint, endPoint);
        if (pathLength < 0.01f) return;

        journey += (speed / pathLength) * Time.deltaTime;
        transform.position = Vector3.Lerp(startPoint, endPoint, journey);

        // Reached the end - wait, then fly through again.
        if (journey >= 1f)
        {
            waiting = true;
            waitTimer = pauseBetweenRuns;
        }
    }

    void ResetToStart()
    {
        transform.position = startPoint;
        journey = 0f;
        waiting = false;
    }
}
