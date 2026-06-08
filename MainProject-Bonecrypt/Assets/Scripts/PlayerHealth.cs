using System;
using UnityEngine;

// Tracks the player's hit points. TakeDamage() subtracts damage (clamped at zero) and raises
// OnHealthChanged so the HUD can redraw the health bar; when HP reaches zero it raises OnDeath
// and tells the GameManager to trigger the lose state.
public class PlayerHealth : MonoBehaviour
{
    public int maxHP = 100;
    public int CurrentHP { get; private set; }

    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    private bool dead;

    void Awake()
    {
        CurrentHP = maxHP;
    }

    void Start()
    {
        // push the starting value so the HUD draws a full bar on spawn
        OnHealthChanged?.Invoke(CurrentHP, maxHP);
    }

    public void TakeDamage(int amount)
    {
        if (dead || amount <= 0) return;
        CurrentHP = Mathf.Max(0, CurrentHP - amount);   // never go below zero
        OnHealthChanged?.Invoke(CurrentHP, maxHP);
        if (CurrentHP == 0)
        {
            dead = true;
            OnDeath?.Invoke();
            if (GameManager.Instance != null)
                GameManager.Instance.TriggerLose();
        }
    }
}
