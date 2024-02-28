using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] public Weapons weaponType;
    [SerializeField] public int weaponTier;

    [SerializeField] public int damage;
}
