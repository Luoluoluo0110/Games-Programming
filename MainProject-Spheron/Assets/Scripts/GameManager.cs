using UnityEngine;
using UnityEngine.SceneManagement;

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
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
