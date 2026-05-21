using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TowerGame.Player;
using TowerGame.UI;

namespace TowerGame.Core
{
    /// <summary>
    /// Runs automatically on play. Rather than require the user to wire up GameObjects in
    /// every scene, the bootstrap builds the persistent game world (camera, managers,
    /// player, UI) once and then hands control to <see cref="GameManager"/>.
    /// </summary>
    public static class Bootstrap
    {
        private static bool _built;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() { _built = false; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Build()
        {
            if (_built) return;
            _built = true;

            // Camera.
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.05f, 0.1f);
            cam.transform.position = new Vector3(0f, 0f, -10f);
            camGo.AddComponent<AudioListener>();
            Object.DontDestroyOnLoad(camGo);

            // Managers.
            var managers = new GameObject("~Managers");
            Object.DontDestroyOnLoad(managers);
            managers.AddComponent<GameManager>();
            managers.AddComponent<TowerManager>();

            // Player.
            var player = PlayerFactory.CreatePlayer();
            Object.DontDestroyOnLoad(player);

            // Camera follow.
            var follow = camGo.AddComponent<CameraFollow>();
            follow.target = player.transform;

            // UI root.
            var ui = UIFactory.CreateUIRoot();
            Object.DontDestroyOnLoad(ui);

            // EventSystem.
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
                Object.DontDestroyOnLoad(es);
            }

            // Start in main menu.
            GameManager.Instance.SetState(GameState.MainMenu);
        }
    }

    /// <summary>Simple 2D camera follow with smooth lerp.</summary>
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public float smoothing = 6f;

        private void LateUpdate()
        {
            if (target == null) return;
            var desired = new Vector3(target.position.x, target.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * smoothing);
        }
    }
}
