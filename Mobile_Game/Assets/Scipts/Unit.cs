using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit
{
    float CurrentMaxHealth;
    float CurrentHealth;
    float CurrentSpeed;
    int CurrentDamage = 1;
    int CurrentArmor = 0;
   protected bool Alive = true;


    public float Health
    {
        get
        {
            return CurrentHealth;
        }
        set
        {
            CurrentHealth = value;
        }
    }

    public float MaxHealth
    {
        get
        {
            return CurrentMaxHealth;
        }
        set
        {
            CurrentMaxHealth = value;
        }
    }

    public int Armor
    {
        get
        {
            return CurrentArmor;
        }
        set
        {
            CurrentArmor = value;
        }
    }

    public int Damage
    {
        get
        {
            return CurrentDamage;
        }
        set
        {
            CurrentDamage = value;
        }
    }
    public float Speed
    {
        get
        {
            return CurrentSpeed;
        }
        set
        {
            CurrentSpeed = value;
        }
    }
    public bool IsAlive
    {
        get
        {
            return Alive;
        }
        set
        {
            IsAlive = value;
        }
    }
    public void DmgUnit(int DmgTaken)
    {
        if (Alive)
        {
            CurrentHealth -= (DmgTaken - (DmgTaken * (Armor / 100)));
            if (CurrentHealth < 0)
            {
                Alive = false;
            }
        }
    }
    public void Heal(int heal)
    {
        if (CurrentHealth < CurrentMaxHealth && Alive == true)
        {
            CurrentHealth = CurrentHealth + heal;
            if (CurrentHealth > CurrentMaxHealth)
            {
                CurrentHealth = CurrentMaxHealth;
            }
        }
    }
    

}


public class Player : Unit
{
    float CurrentForceField;
    float CurrentMaxForceField;
    public float ForceField
    {
        get
        {
            return CurrentForceField;
        }
        set
        {
            CurrentForceField = value;
        }
    }
    public float MaxForceField
    {
        get
        {
            return CurrentMaxForceField;
        }
        set
        {
            CurrentMaxForceField = value;
        }
    }
  public Player(float maxhealth,float health,float speed, int damage,int armor)
    {
        base.MaxHealth = maxhealth;
        base.Health = health;
        base.Speed = speed;
        base.Damage = damage;
        base.Armor = armor;
        base.IsAlive = true;
        CurrentForceField = 0;
        CurrentMaxForceField = maxhealth;
        Alive = true;
    }
    public void AddToForceField(float force)
    {
        if (ForceField < MaxForceField)
        {
            ForceField += force;
        }
    }
}