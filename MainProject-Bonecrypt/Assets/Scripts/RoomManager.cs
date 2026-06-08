using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Controls a single room via a trigger volume. The first time the player enters, it updates the
// HUD room number and spawns the room's enemy wave at the spawn points. It tracks the living
// enemies and, once they are all dead, clears the room: opens the exit door (disabling the door
// = open) and, if this is the boss room, tells the GameManager to trigger the win.
[RequireComponent(typeof(Collider))]
public class RoomManager : MonoBehaviour
{
    [Header("Identity")]
    public int roomNumber = 1;
    public bool isBossRoom = false;

    [Header("Spawning")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    [Header("Door (active = closed, inactive = open)")]
    [SerializeField] private GameObject exitDoor;

    public UnityEvent onCleared;

    private readonly List<EnemyCube> alive = new List<EnemyCube>();
    private bool activated;
    private bool cleared;

    void Reset()
    {
        Collider c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) Activate();
    }

    // fired the first time the player enters: lock the room in and spawn its wave
    public void Activate()
    {
        if (activated) return;
        activated = true;

        if (HUDController.Instance != null)
            HUDController.Instance.SetRoom(roomNumber);

        if (enemyPrefab == null || spawnPoints.Count == 0)
        {
            Clear();
            return;
        }

        foreach (var sp in spawnPoints)
        {
            if (sp == null) continue;
            GameObject e = Instantiate(enemyPrefab, sp.position, sp.rotation);
            EnemyCube ec = e.GetComponent<EnemyCube>();
            if (ec != null)
            {
                alive.Add(ec);
                ec.OnDeath += () => OnEnemyDied(ec);   // tick this enemy off when it dies
            }
        }

        if (alive.Count == 0) Clear();   // nothing actually spawned, so open straight away
    }

    void OnEnemyDied(EnemyCube ec)
    {
        alive.Remove(ec);
        if (alive.Count == 0) Clear();
    }

    // room beaten: open the exit and, if this is the final room, win the game
    void Clear()
    {
        if (cleared) return;
        cleared = true;
        if (exitDoor != null) exitDoor.SetActive(false);   // disabling the door = open
        onCleared?.Invoke();

        if (isBossRoom && GameManager.Instance != null)
            GameManager.Instance.TriggerWin();
    }
}
