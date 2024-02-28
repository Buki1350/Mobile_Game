using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    [SerializeField] Animator animator;

    PlayerMovement pMovement;
    PlayerWeapons pWeapons;
    
    void Start()
    {
        #region init and errors
        if (gameObject.GetComponent<PlayerMovement>() == null)
            Debug.LogError($"<color=red>PlayerAnimations: PlayerMovement script is missing!</color>");
        else
            pMovement = gameObject.GetComponent<PlayerMovement>();

        if (gameObject.GetComponent<PlayerWeapons>() == null)
            Debug.LogError($"<color=red>PlayerAnimations: PlayerWeapons script is missing!</color>");
        else
            pWeapons = gameObject.GetComponent<PlayerWeapons>();
        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("WalkBlendX", pMovement.walkBlendX);
        animator.SetFloat("WalkBlendY", pMovement.walkBlendY);
        animator.SetBool("isFighting", pMovement.isFighting);
        animator.SetBool("isMoving", pMovement.isMoving);

        int weaponIndex = 0;
        if (pMovement.isFighting)
        {
            foreach (GameObject sword in pWeapons.swords)
                sword.SetActive(false);

            foreach (GameObject greatsword in pWeapons.greatswords)
                greatsword.SetActive(false);
            
            foreach (GameObject axe in pWeapons.axes)
                axe.SetActive(false);

            foreach (GameObject greataxe in pWeapons.greataxes)
                greataxe.SetActive(false);

            animator.SetLayerWeight(animator.GetLayerIndex("UpperBody"), 1);
            if (pWeapons.currentWeapon == null)
                weaponIndex = 0;
            else
            {
                switch (pWeapons.currentWeapon.GetComponent<Weapon>().weaponType)
                {
                    case Weapons.nonWeapon: weaponIndex = 1; break;
                    case Weapons.sword: weaponIndex = 2; break;
                    case Weapons.greatsword: weaponIndex = 3; break;
                    case Weapons.axe: weaponIndex = 4; break;
                    case Weapons.greataxe: weaponIndex = 5; break;
                }
            }
        }
        else
            animator.SetLayerWeight(animator.GetLayerIndex("UpperBody"), 0);
        animator.SetInteger("weaponIndex", weaponIndex);


    }
}
