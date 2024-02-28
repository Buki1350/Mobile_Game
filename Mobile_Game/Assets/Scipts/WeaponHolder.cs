using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    void Start()
    {
        foreach (Transform weapons in transform)
        {
            weapons.gameObject.GetComponent<BoxCollider>().enabled = false;
        }
    }
}
