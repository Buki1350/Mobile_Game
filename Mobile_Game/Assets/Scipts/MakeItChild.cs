using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeItChild : MonoBehaviour
{
    [SerializeField] GameObject parent;

    private void Start()
    {
        transform.position = parent.transform.position;
        transform.parent = parent.transform;
    }
}
