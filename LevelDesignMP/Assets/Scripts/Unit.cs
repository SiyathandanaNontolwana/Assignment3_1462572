using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public string unitName;

    public int damage;
    public int healAmount;
    public int critModifier;
    public int critAmount = 0;

    public int maxHP;
    public int currentHP;

    private void Start()
    {
        critModifier = damage * critAmount;
    }

    public bool TakeDamage(int damage)
    {
        currentHP -= damage;

        if (currentHP <= 0)
        {
            return true;
        }
        else
            return false;
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP)
            currentHP = maxHP;
    }

}
