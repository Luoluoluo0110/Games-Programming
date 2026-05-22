using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This class controls player movement and aiming.
///
/// Architecture role:
/// Demonstrates safe New Input System lifecycle management - input maps are
/// enabled in OnEnable() and disabled in OnDisable() so no input callbacks
/// linger after the object is deactivated (prevents input-event memory leaks).
/// </summary>
public class Controller : MonoBehaviour
{
    [Header("GameObject/Component References")]
    [Tooltip("The Rigidbody2D component to use in \"Astroids Mode\".")]
    public Rigidbody2D myRigidbody = null;

    [Header("Movement Variables")]
    [Tooltip("The speed at which the player will move.")]
    [Range(0f, 50f)]
    public float moveSpeed = 10.0f;
    [Tooltip("The speed at which the player rotates in asteroids movement mode")]
    [Range(0f, 360f)]
    public float rotationSpeed = 60f;

    [Header("Input Actions & Controls")]
    [Tooltip("The input action(s) that map to player movement")]
    public InputAction moveAction;
    [Tooltip("The input action(s) that map to where the controller looks")]
    public InputAction lookAction;

    /// <summary>
    /// Enum which stores different aiming modes
    /// </summary>
    public enum AimModes { AimTowardsMouse, AimForwards };

    [Tooltip("The aim mode in use by this player:\n" +
        "Aim Towards Mouse: Player rotates to face the mouse\n" +
        "Aim Forwards: Player aims the direction they face (doesn't face towards the mouse)")]
    public AimModes aimMode = AimModes.AimTowardsMouse;

    /// <summary>
    /// Enum to handle different movement modes for the player
    /// </summary>
    public enum MovementModes { MoveHorizontally, MoveVertically, FreeRoam, Astroids };

    [Tooltip("The movement mode used by this controller:\n" +
        "Move Horizontally: Player can only move left/right\n" +
        "Move Vertically: Player can only move up/down\n" +
        "FreeRoam: Player can move in any direction and can aim\n" +
        "Astroids: Player moves forward/back in the direction they are facing and rotates with horizontal input")]
    public MovementModes movementMode = MovementModes.FreeRoam;

    // Whether the player can aim with the mouse or not
    private bool canAimWithMouse
    {
        get { return aimMode == AimModes.AimTowardsMouse; }
    }

    // Whether the player's X coordinate is locked (also assign in rigidbody constraints)
    private bool lockXCoordinate
    {
        get { return movementMode == MovementModes.MoveVertically; }
    }

    // Whether the player's Y coordinate is locked (also assign in rigidbody constraints)
    public bool lockYCoordinate
    {
        get { return movementMode == MovementModes.MoveHorizontally; }
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called when the script instance is loaded.
    /// Lifecycle split: Awake caches component references on THIS object so the
    /// rest of the script can assume they are valid.
    /// </summary>
    private void Awake()
    {
        Debug.Log("[Controller] Awake - caching component references.");
        if (myRigidbody == null)
        {
            myRigidbody = GetComponent<Rigidbody2D>();
        }
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called whenever the attached gameobject is enabled.
    /// INPUT LIFECYCLE: activate the input maps here so input is only ever read
    /// while this controller is actually active in the scene.
    /// </summary>
    private void OnEnable()
    {
        Debug.Log("[Controller] OnEnable - enabling input actions.");
        moveAction.Enable();
        lookAction.Enable();
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called whenever the attached gameobject is disabled.
    /// INPUT LIFECYCLE: cleanly deactivate the input maps here. Without this the
    /// actions would keep polling devices after the controller is gone (leak).
    /// </summary>
    private void OnDisable()
    {
        Debug.Log("[Controller] OnDisable - disabling input actions.");
        moveAction.Disable();
        lookAction.Disable();
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called once before the first Update.
    /// Lifecycle split: Start validates configuration that depends on the
    /// inspector being fully set up (input bindings).
    /// </summary>
    private void Start()
    {
        if (moveAction.bindings.Count == 0 || lookAction.bindings.Count == 0)
        {
            Debug.LogWarning("An Input Action does not have a binding set! Make sure that each " +
                "Input Action has a binding set or the controller will not work!");
        }
    }

    /// <summary>
    /// Description:
    /// Standard Unity function called once per frame. Kept lightweight - it only
    /// delegates to the HandleInput() helper.
    /// </summary>
    private void Update()
    {
        HandleInput();
    }

    /// <summary>
    /// Description:
    /// Helper that reads input each frame and drives movement / aiming.
    /// </summary>
    private void HandleInput()
    {
        // Find the position that the player should look at
        Vector2 lookPosition = GetLookPosition();

        // Get movement input from the input action
        if (moveAction.bindings.Count == 0)
        {
            Debug.LogError("The Move Input Action does not have a binding set! It must have a " +
                "binding set in order for movement to happen!");
        }
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 movementVector = new Vector3(moveInput.x, moveInput.y, 0);

        // Move and aim the player
        MovePlayer(movementVector);
        LookAtPoint(lookPosition);
    }

    /// <summary>
    /// Description:
    /// Returns the position the player should look at.
    /// </summary>
    /// <returns>Vector2: The position the player should look at</returns>
    public Vector2 GetLookPosition()
    {
        Vector2 result = transform.up;
        if (aimMode != AimModes.AimForwards)
        {
            if (lookAction.bindings.Count == 0)
            {
                Debug.LogError("The Look Input Action does not have a binding set! It must have " +
                    "a binding set in order for the player to look around!");
            }
            result = lookAction.ReadValue<Vector2>();
        }
        else
        {
            result = transform.up;
        }
        return result;
    }

    /// <summary>
    /// Description:
    /// Helper that moves the player according to the active movement mode.
    /// All translations are multiplied by Time.deltaTime for framerate independence.
    /// </summary>
    /// <param name="movement">The direction to move the player</param>
    private void MovePlayer(Vector3 movement)
    {
        // Move according to the Astroids setting (physics based)
        if (movementMode == MovementModes.Astroids)
        {
            // Defensive: grab a rigidbody if one was not cached in Awake.
            if (myRigidbody == null)
            {
                myRigidbody = GetComponent<Rigidbody2D>();
            }

            // Move the player using physics, scaled by deltaTime.
            Vector2 force = transform.up * movement.y * Time.deltaTime * moveSpeed;
            myRigidbody.AddForce(force);

            // Rotate the player around the z axis with horizontal input.
            Vector3 newRotationEulars = transform.rotation.eulerAngles;
            float zAxisRotation = transform.rotation.eulerAngles.z;
            float newZAxisRotation = zAxisRotation - rotationSpeed * movement.x * Time.deltaTime;
            newRotationEulars = new Vector3(newRotationEulars.x, newRotationEulars.y, newZAxisRotation);
            transform.rotation = Quaternion.Euler(newRotationEulars);
        }
        // Move according to the other (transform based) settings
        else
        {
            // Respect locked coordinates for the constrained movement modes.
            if (lockXCoordinate)
            {
                movement.x = 0;
            }
            if (lockYCoordinate)
            {
                movement.y = 0;
            }
            transform.position = transform.position + (movement * Time.deltaTime * moveSpeed);
        }
    }

    /// <summary>
    /// Description:
    /// Helper that rotates the player to look at a screen-space point.
    /// </summary>
    /// <param name="point">The screen space position to look at</param>
    private void LookAtPoint(Vector3 point)
    {
        if (Time.timeScale > 0)
        {
            Vector2 lookDirection = Camera.main.ScreenToWorldPoint(point) - transform.position;

            if (canAimWithMouse)
            {
                transform.up = lookDirection;
            }
            else if (myRigidbody != null)
            {
                myRigidbody.freezeRotation = true;
            }
        }
    }
}
