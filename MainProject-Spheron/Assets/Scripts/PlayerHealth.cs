using System;
using UnityEngine;

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
        OnHealthChanged?.Invoke(CurrentHP, maxHP);
    }

    public void TakeDamage(int amount)
    {
        if (dead || amount <= 0) return;
        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        OnHealthChanged?.Invoke(CurrentHP, maxHP);
        if (CurrentHP == 0)
        {
            dead = true;
            OnDeath?.Invoke();
        }
    }
}
