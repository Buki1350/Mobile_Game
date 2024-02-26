using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;



public class PlayerMovement : MonoBehaviour
{
    [SerializeField] public float playerSpeed = 3f;
    [SerializeField] public float rotationSpeed = 10f;
        
    public Transform playerModel;
    public FixedJoystick FJoystick_L;
    public FixedJoystick FJoystick_R;

    [NonSerialized] public float walkBlendX;
    [NonSerialized] public float walkBlendY;
    //States for animation and movement logic
    [NonSerialized] public bool isMoving;
    [NonSerialized] public bool isFighting;

    CharacterController controller;
    Vector3 spine1Rotation;// For lateupdate

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
            gameObject.AddComponent<CharacterController>();
    }

    //NOTE ================================================================================================================
    //Nie krêc krêgos³upem. Dodaj animacje chodzenia po skosie i wyblenduj je
    //=====================================================================================================================

    void Update()
    {
        Vector3 moveDir = new Vector3(FJoystick_L.Direction.x, 0.0f, FJoystick_L.Direction.y);
        Vector3 lookDir = new Vector3(FJoystick_R.Direction.x, 0.0f, FJoystick_R.Direction.y);

        isMoving = true;
        isFighting = true;

        if (moveDir.magnitude == 0)
            isMoving = false;
        if (lookDir.magnitude == 0)
            isFighting = false;

        float targetPlayerAngle;
        float currentPlayerSpeed = playerSpeed;
        if (!isFighting)
            targetPlayerAngle = Mathf.Atan2(-moveDir.z, moveDir.x) * Mathf.Rad2Deg;
        else
        {
            targetPlayerAngle = Mathf.Atan2(-lookDir.z, lookDir.x) * Mathf.Rad2Deg;
            currentPlayerSpeed /= 2;
        }
        
        if (isMoving || isFighting)
            playerModel.rotation = Quaternion.Euler(0f, targetPlayerAngle - 90, 0f);


        controller.Move(moveDir * currentPlayerSpeed * Time.deltaTime);


        //FOR LOWERBODY ANIMATION BLEND TREE

        //normalized: 1 forward, -1 backward
        //BUG: going to -1.2 when backwards
        walkBlendY = moveDir.magnitude - Vector3.Angle(moveDir, lookDir) / 90;
        if (!isFighting)
            walkBlendY *= 2;
        //normalized: 1 left, -1 right
        walkBlendX = Mathf.Sin(Vector3.SignedAngle(moveDir, lookDir, transform.up) * Mathf.PI / 180);



        //Debug.Log((float)(int)(walkBlendX*100)/100 + "   ||   " + (float)(int)(walkBlendY * 100) / 100);

    }
}
