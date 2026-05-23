using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
                ec.OnDeath += () => OnEnemyDied(ec);
            }
        }

        if (alive.Count == 0) Clear();
    }

    void OnEnemyDied(EnemyCube ec)
    {
        alive.Remove(ec);
        if (alive.Count == 0) Clear();
    }

    void Clear()
    {
        if (cleared) return;
        cleared = true;
        if (exitDoor != null) exitDoor.SetActive(false);
        onCleared?.Invoke();

        if (isBossRoom && GameManager.Instance != null)
            GameManager.Instance.TriggerWin();
    }
}
