using UnityEngine;

// Walk into this trigger to finish the level. Placed past the boss door, so it only becomes
// reachable once the boss room is cleared and that door opens.
[RequireComponent(typeof(Collider))]
public class LevelExitPortal : MonoBehaviour
{
    void Reset()
    {
        Collider c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance != null)
            GameManager.Instance.TriggerWin();
    }
}
