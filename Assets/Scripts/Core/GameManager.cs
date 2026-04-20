using UnityEngine;

namespace TowerGame.Core
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        Victory,
        GameOver
    }

    /// <summary>
    /// Singleton coordinator for the whole game. Holds the current <see cref="GameState"/>
    /// and exposes global events that UI layers subscribe to.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; } = GameState.MainMenu;

        public event System.Action<GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetState(GameState next)
        {
            if (State == next) return;
            State = next;
            Time.timeScale = next == GameState.Paused ? 0f : 1f;
            OnStateChanged?.Invoke(next);
        }

        public void StartNewRun()
        {
            SetState(GameState.Playing);
            TowerManager.Instance?.StartNewRun();
        }

        public void TogglePause()
        {
            if (State == GameState.Playing) SetState(GameState.Paused);
            else if (State == GameState.Paused) SetState(GameState.Playing);
        }
    }
}
