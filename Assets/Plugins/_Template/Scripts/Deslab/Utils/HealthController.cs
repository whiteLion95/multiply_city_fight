using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HealthController : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int curHealth;
    [SerializeField] private SliderBar healthBar;

    public Action OnDie;
    public Action<int> OnHealthChanged;

    private void Awake()
    {
        curHealth = maxHealth;
    }

    private void Start()
    {
        if (healthBar != null)
        {
            healthBar.SetMaxValue(maxHealth);
            healthBar.SetValue(maxHealth);
        }
    }

    public void TakeDamage(int damage)
    {
        if (curHealth > 0)
        {
            curHealth -= Mathf.Abs(damage);

            if (curHealth <= 0)
            {
                curHealth = 0;
                OnDie?.Invoke();
            }

            OnHealthChanged?.Invoke(curHealth);
            if (healthBar != null) healthBar.ChangeValue(curHealth);
        }
    }

    public void RecoverHealth(int value)
    {
        if (curHealth < maxHealth)
        {
            curHealth += Mathf.Abs(value);

            if (curHealth > maxHealth)
            {
                curHealth = maxHealth;
            }

            OnHealthChanged?.Invoke(curHealth);
            if (healthBar != null) healthBar.ChangeValue(curHealth);
        }
    }
}
