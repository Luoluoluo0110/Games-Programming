using UnityEngine;
using UnityEngine.SceneManagement;

// Global game state machine (singleton). Holds the Playing/Win/Lose state; TriggerWin and
// TriggerLose pause the game (timeScale = 0), show the matching end-screen panel and unlock the
// cursor. After the round ends, pressing R reloads the current scene to restart.
public class GameManager : MonoBehaviour
{
    public enum State { Playing, Win, Lose }

    public static GameManager Instance { get; private set; }
    public State CurrentState { get; private set; } = State.Playing;

    [SerializeField] private HUDController hud;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        // R reloads the level, but only after you've won or lost
        if (CurrentState != State.Playing && Input.GetKeyDown(KeyCode.R))
            Restart();
    }

    public void TriggerWin()
    {
        if (CurrentState != State.Playing) return;
        CurrentState = State.Win;
        Time.timeScale = 0f;
        if (hud != null) hud.ShowVictory();
        UnlockCursor();
    }

    public void TriggerLose()
    {
        if (CurrentState != State.Playing) return;
        CurrentState = State.Lose;
        Time.timeScale = 0f;
        if (hud != null) hud.ShowDefeat();
        UnlockCursor();
    }

    public void Restart()
    {
        Time.timeScale = 1f;   // undo the pause before reloading
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
