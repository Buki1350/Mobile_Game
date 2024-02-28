using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerActions : MonoBehaviour
{
    [Header("Other")]
    [SerializeField] GameObject gameManager;
    WeaponsManager weaponsManager;

    [SerializeField] GameObject pickUpButton;
    bool isPickingUp = false; // for button

    [SerializeField] GameObject swapButton;

    [SerializeField] GameObject itemHighlight;
    GameObject highlightObject;
    bool highlightCreated = false;

    [SerializeField] TextMeshProUGUI     Eq;

    [Header("Player")]
    [SerializeField] float pickUpRadius;
    [SerializeField] GameObject weaponHolder;

    PlayerWeapons pWeapons;


    bool noErrors = true;
    void Start()
    {
        #region init, listeners and errors
        if (pickUpButton.GetComponent<Button>() != null)
            pickUpButton.GetComponent<Button>().onClick.AddListener(OnPickUpButtonClick);
        else
        {
            Debug.LogError($"<color=red>PlayerAction: button component not found on pickUpButton!</color>");
            noErrors = false;
        }

        if (swapButton.GetComponent<Button>() != null)
            swapButton.GetComponent<Button>().onClick.AddListener(SwapWeapons);
        else
        {
            Debug.LogError($"<color=red>PlayerAction: button component not found on swapButton!</color>");
            noErrors = false;
        }

        if (GetComponent<PlayerWeapons>() != null)
            pWeapons = GetComponent<PlayerWeapons>();
        else
        {
            Debug.LogError($"<color=red>PlayerAction: no PlayerWeapon script!</color>");
            noErrors = false;
        }

        if (gameManager.GetComponent<WeaponsManager>() != null)
            weaponsManager = gameManager.GetComponent<WeaponsManager>();
        else
        {
            Debug.LogError($"<color=red>PlayerAnimations: PlayerWeapons script is missing!</color>");
            noErrors = false;
        }
        #endregion

        foreach (Transform weapon in weaponHolder.transform)
        {
            weapon.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (noErrors)
        { 
            #region Picking up items
            //Picking up items is made by checking all items around and picking closest pickable object or weapon
            //
            //Making collider
            Vector3 point1Capsule = transform.position + new Vector3(0.0f, 10.0f, 0.0f);
            Vector3 point2Capsule = transform.position - new Vector3(0.0f, 10.0f, 0.0f);
            Collider[] colliders = Physics.OverlapCapsule(point1Capsule, point2Capsule, pickUpRadius);
            float closestDistance = 0;
            GameObject closestPickableObject = null;

            foreach (Collider collider in colliders)
            {
                //filtering specific items and choosing the closest one
                if (collider.gameObject.GetComponent<Item>() != null || collider.gameObject.GetComponent<Weapon>() != null)
                {
                    if (closestPickableObject == null)
                    {
                        closestDistance = Vector3.Distance(transform.position, collider.gameObject.transform.position);
                        closestPickableObject = collider.gameObject;
                    }
                    else
                    {
                        if (Vector3.Distance(transform.position, collider.gameObject.transform.position) < closestDistance)
                        {
                            closestDistance = Vector3.Distance(transform.position, collider.gameObject.transform.position);
                            closestPickableObject = collider.gameObject;
                        }
                    }
                }
            }

            //if there is object to pick up - highlight it and handle actions
            if (closestPickableObject != null)
            {
                pickUpButton.SetActive(true);
                
                //HIGHLIGHT
                //Take center of collision box, so highlight wont be below origin (doesn't have to be in center)
                Vector3 rayOrigin = closestPickableObject.transform.position;
                if (closestPickableObject.GetComponent<BoxCollider>() != null)
                    rayOrigin = closestPickableObject.GetComponent<BoxCollider>().bounds.center;
                else if (closestPickableObject.GetComponent<CapsuleCollider>() != null)
                    rayOrigin = closestPickableObject.GetComponent<CapsuleCollider>().bounds.center;
                else if (closestPickableObject.GetComponent<SphereCollider>() != null)
                    rayOrigin = closestPickableObject.GetComponent<SphereCollider>().bounds.center;
                else if (closestPickableObject.GetComponent<MeshCollider>() != null)
                    rayOrigin = closestPickableObject.GetComponent<MeshCollider>().bounds.center;
                else
                    Debug.LogError($"<color=red>{closestPickableObject.name} has no proper collider!</color>");

                Ray rayToGround = new Ray(rayOrigin, -transform.up);
                float maxRaycastDistance = 100.0f;
                RaycastHit hitInfo;
                if (Physics.Raycast(rayToGround, out hitInfo, maxRaycastDistance))
                {
                    Vector3 groundPoint = hitInfo.point;
                    if (!highlightCreated)
                    {
                        highlightObject = Instantiate(itemHighlight, groundPoint + new Vector3(0.0f, 0.01f, 0.0f), Quaternion.identity);
                        highlightCreated = true;
                    }
                    else
                        highlightObject.transform.position = groundPoint + new Vector3(0.0f, 0.01f, 0.0f);
                }


                //PICKING UP
                if (isPickingUp)
                {
                    if (closestPickableObject.gameObject.GetComponent<Weapon>() != null)
                    {
                        GameObject newWeapon = null;
                        for (int i = 0; i < weaponsManager.weaponPrefabs.Count && newWeapon == null; i++)
                        {
                            if (closestPickableObject.name == weaponsManager.weaponPrefabs[i].gameObject.name)
                            {
                                newWeapon = weaponsManager.weaponPrefabs[i].gameObject;
                                newWeapon.name = closestPickableObject.name;
                            }
                        }
                        if (newWeapon == null)
                        {
                            Debug.LogError($"<color=red>PlayerActions: Picked weapon ({closestPickableObject.name}) is undefinded by WeaponManager prefab list!</color>");
                            noErrors = false;
                        }

                        if (pWeapons.currentWeapon == null)
                        {
                            pWeapons.currentWeapon = newWeapon;
                            weaponHolder.transform.Find(pWeapons.currentWeapon.name).gameObject.SetActive(true);
                        }
                        else if (pWeapons.secondaryWeapon == null)
                            pWeapons.secondaryWeapon = newWeapon;
                        else
                        {
                            weaponHolder.transform.Find(pWeapons.currentWeapon.name).gameObject.SetActive(false);
                            GameObject weaponToThrowAway = Instantiate(pWeapons.currentWeapon, closestPickableObject.transform.position, Quaternion.identity);
                            weaponToThrowAway.name = pWeapons.currentWeapon.name;
                            pWeapons.currentWeapon = newWeapon;
                            weaponHolder.transform.Find(pWeapons.currentWeapon.name).gameObject.SetActive(true);
                        }
                        Destroy(closestPickableObject);
                    }
                    isPickingUp = false;
                }
            }
            else
            {
                pickUpButton.SetActive(false);
                Destroy(highlightObject);
                highlightCreated = false;
            }

            if (pWeapons.secondaryWeapon == null)
            {
                if (pWeapons.currentWeapon == null)
                    Eq.text = "Equipment is empty";
                else
                    Eq.text = "1: " + pWeapons.currentWeapon.name;
            }
            else
                Eq.text = "1: " + pWeapons.currentWeapon.name + "\n2: " + pWeapons.secondaryWeapon.name;
            #endregion

            #region swapping weapons
            if (pWeapons.currentWeapon != null && pWeapons.secondaryWeapon != null)
                swapButton.SetActive(true);
            else
                swapButton.SetActive(false);

            #endregion
        }
    }
    public void OnPickUpButtonClick()
    {
        isPickingUp=true;
    }

    public void SwapWeapons()
    {
        weaponHolder.transform.Find(pWeapons.currentWeapon.name).gameObject.SetActive(false);
        GameObject tempSwapWeapon = pWeapons.currentWeapon;
        pWeapons.currentWeapon = pWeapons.secondaryWeapon;
        pWeapons.secondaryWeapon = tempSwapWeapon;
        weaponHolder.transform.Find(pWeapons.currentWeapon.name).gameObject.SetActive(true);
    }
}
