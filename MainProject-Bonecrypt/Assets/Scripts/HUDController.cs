using UnityEngine;
using UnityEngine.UI;

// On-screen UI (singleton). Subscribes to the player's health events to update the health bar,
// displays the current room number, and exposes show/hide toggles for the victory and defeat
// panels (both hidden on startup).
public class HUDController : MonoBehaviour
{
    public static HUDController Instance { get; private set; }

    [Header("Health")]
    [SerializeField] private Image healthFill;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Room")]
    [SerializeField] private Text roomLabel;

    [Header("End Screens")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
    }

    void Start()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHealth;
            UpdateHealth(playerHealth.CurrentHP, playerHealth.maxHP);
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealth;
    }

    public void UpdateHealth(int current, int max)
    {
        if (healthFill == null) return;
        healthFill.fillAmount = max > 0 ? (float)current / max : 0f;
    }

    public void SetRoom(int roomNumber)
    {
        if (roomLabel == null) return;
        roomLabel.text = $"Room {roomNumber}";
    }

    public void ShowVictory()
    {
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }

    public void ShowDefeat()
    {
        if (defeatPanel != null) defeatPanel.SetActive(true);
    }
}
