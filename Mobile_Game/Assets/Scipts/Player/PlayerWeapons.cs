using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Weapons
{
    none,
    nonWeapon,
    sword,
    greatsword,
    axe,
    greataxe
}

public class PlayerWeapons : MonoBehaviour
{
    [Header("Weapons on player model (in hand) ordered by its index")]
    [SerializeField] public GameObject[] swords;
    [SerializeField] public GameObject[] greatswords;
    [SerializeField] public GameObject[] axes;
    [SerializeField] public GameObject[] greataxes;

    [NonSerialized] public GameObject currentWeapon;
    [NonSerialized] public GameObject secondaryWeapon;

    //[NonSerialized] public Weapons currentWeapon = Weapons.none;
    //[NonSerialized] public int currentWeaponIndex = 0;
    //[NonSerialized] public Weapons secondaryWeapon = Weapons.none;
    //[NonSerialized] public int secondaryWeaponIndex = 0;
}
