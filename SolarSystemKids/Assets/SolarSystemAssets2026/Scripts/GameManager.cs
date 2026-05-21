using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// The "brain" of the scene. It connects clicks, the fact pop-up, the camera
/// and the selection spotlight together. Exactly ONE GameManager lives in the
/// scene and other scripts reach it through GameManager.Instance.
/// Compatible with Unity 2022.3.62f1.
/// </summary>
[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    /// <summary>Global access point so any InteractableBody can report a click.</summary>
    public static GameManager Instance { get; private set; }

    [Header("Scene References")]
    public CameraController cameraController;

    [Header("Fact Pop-up UI")]
    [Tooltip("Root panel of the fact pop-up. It starts hidden.")]
    public GameObject factPanel;
    public Text titleText;
    public Text factText;
    [Tooltip("UI button that returns the player to the overview.")]
    public Button backButton;

    [Header("Audio (optional)")]
    public AudioSource uiAudioSource;
    public AudioClip backButtonSound;

    [Header("Selection Spotlight (optional)")]
    [Tooltip("A spotlight that shines on whichever body is selected.")]
    public Light selectionSpotlight;
    [Tooltip("How high above the body the spotlight sits.")]
    public float spotlightHeight = 10f;

    InteractableBody currentBody;

    void Awake()
    {
        // Simple singleton - keep only one GameManager alive.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // The pop-up and spotlight begin switched off (clean overview).
        if (factPanel != null) factPanel.SetActive(false);
        if (selectionSpotlight != null) selectionSpotlight.enabled = false;

        // Wire the Back button in code so the designer cannot forget to.
        if (backButton != null)
            backButton.onClick.AddListener(ReturnToOverview);
    }

    void Update()
    {
        // Click on empty space to return to the overview - intuitive for kids.
        if (currentBody != null && Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
                bool hitBody = Physics.Raycast(ray, out RaycastHit hit) &&
                               hit.collider.GetComponentInParent<InteractableBody>() != null;
                if (!hitBody)
                    ReturnToOverview();
            }
        }

        // Keep the spotlight aimed at the focused body as it orbits.
        if (selectionSpotlight != null && selectionSpotlight.enabled && currentBody != null)
        {
            Vector3 abovePos = currentBody.transform.position + Vector3.up * spotlightHeight;
            selectionSpotlight.transform.position = abovePos;
            selectionSpotlight.transform.rotation =
                Quaternion.LookRotation(currentBody.transform.position - abovePos);
        }
    }

    /// <summary>Called by an InteractableBody when the player clicks it.</summary>
    public void FocusOnBody(InteractableBody body)
    {
        if (body == null) return;
        currentBody = body;

        // Show the kid-friendly fact pop-up.
        if (titleText != null) titleText.text = body.displayName;
        if (factText != null) factText.text = body.funFact;
        if (factPanel != null) factPanel.SetActive(true);

        // Glide the camera in toward the body.
        if (cameraController != null)
            cameraController.FocusOn(body.transform, body.cameraFocusDistance);

        // Switch on the selection spotlight.
        if (selectionSpotlight != null) selectionSpotlight.enabled = true;
    }

    /// <summary>Hides the pop-up and glides the camera back to the overview.</summary>
    public void ReturnToOverview()
    {
        if (currentBody == null) return;
        currentBody = null;

        if (factPanel != null) factPanel.SetActive(false);
        if (cameraController != null) cameraController.ReturnToOverview();
        if (selectionSpotlight != null) selectionSpotlight.enabled = false;

        if (uiAudioSource != null && backButtonSound != null)
            uiAudioSource.PlayOneShot(backButtonSound);
    }

    /// <summary>True while the given body is the one currently focused.</summary>
    public bool IsFocused(InteractableBody body)
    {
        return currentBody != null && currentBody == body;
    }

    /// <summary>True when the mouse is over a UI element.</summary>
    static bool IsPointerOverUI()
    {
        return EventSystem.current != null &&
               EventSystem.current.IsPointerOverGameObject();
    }
}
