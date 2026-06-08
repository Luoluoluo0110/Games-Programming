using UnityEngine;

// First-person player movement and camera control.
// Mouse-look yaws the body and pitches the camera (clamped so you can't flip over),
// WASD drives a CharacterController with simple gravity, and the position is hard-clamped
// to the level bounds so the player can't walk out of the corridor. Input is ignored once
// the round has ended (win/lose) so the camera doesn't keep spinning on the end screen.
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float gravity = -20f;

    [Header("Look")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;
    public float maxPitch = 85f;

    [Header("Bounds")]
    public float minX = -4f;
    public float maxX = 4f;
    public float minZ = -10f;
    public float maxZ = 55f;

    private CharacterController controller;
    private float verticalVelocity;
    private float pitch;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // stop responding once the round ends; mouse-look ignores timeScale, so without
        // this the camera keeps spinning on the win/lose screen
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.State.Playing)
            return;

        HandleLook();
        HandleMove();
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(0f, mouseX, 0f);   // yaw turns the whole body

        // pitch only tilts the camera, kept within range so you can't flip over
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void HandleMove()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        move *= moveSpeed;

        // small downward push when grounded keeps the controller settled on the floor
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -1f;
        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);

        // hard-clamp to the level bounds so the player can't walk out of the corridor
        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x, minX, maxX);
        p.z = Mathf.Clamp(p.z, minZ, maxZ);
        transform.position = p;
    }
}
