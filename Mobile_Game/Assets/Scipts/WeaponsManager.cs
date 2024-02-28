using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponsManager : MonoBehaviour
{
    [SerializeField] public List<GameObject> weaponPrefabs;

    void Start()
    {
        List<GameObject> checkedWeaponsList = new List<GameObject>();
        foreach (GameObject weapon in weaponPrefabs)
        {
            if (weapon.GetComponent<Weapon>() == null)
                Debug.LogError($"<color=red>WeaponsManager: {weapon.name} has no Weapon script!</color>");
            if (checkedWeaponsList.Contains(weapon))
                Debug.LogWarning($"<color=orange>WeaponManager: has duplicate: {weapon.name}</color>");
            checkedWeaponsList.Add(weapon);
        }
    }
}
