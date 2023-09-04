using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    float playerSpeed = 10f;
    float rotationSpeed = 10f;

    public Transform playerBody;
    public CharacterController controller;
    public FixedJoystick FJoystick_L;
    public FixedJoystick FJoystick_R;

    bool isMoving = false;
    bool isFighting = false;

    Vector3 moveDir = Vector3.zero;
    Quaternion targetRotation = Quaternion.identity;
    float JStickInputDegres = 0f;
    float JStickInputRadians = 0f;
    float rotationDelta = 0;
    void Start()
    {
            
    }

    void Update()
    {
        if (FJoystick_L.Horizontal == 0 && FJoystick_L.Vertical == 0)
            isMoving = false;
        else
            isMoving = true;

        if (FJoystick_R.Horizontal == 0 && FJoystick_R.Vertical == 0)
            isFighting = false;
        else
            isFighting = true;

        if (isMoving)
        {
            moveDir = new Vector3(FJoystick_L.Horizontal, 0, FJoystick_L.Vertical).normalized;
            _ = controller.Move(moveDir * playerSpeed * Time.deltaTime);

            if (!isFighting)
            {
                JStickInputRadians = Mathf.Atan2(FJoystick_L.Horizontal, FJoystick_L.Vertical);
                JStickInputDegres = JStickInputRadians * Mathf.Rad2Deg;

                targetRotation = Quaternion.Euler(0, JStickInputDegres, 0);

                rotationDelta = Quaternion.Angle(targetRotation, playerBody.rotation);
                playerBody.rotation = Quaternion.Lerp(playerBody.rotation, targetRotation, 100f / rotationDelta * rotationSpeed * Time.deltaTime);
            }
        }

        if (isFighting)
        {
            JStickInputRadians = Mathf.Atan2(FJoystick_R.Horizontal, FJoystick_R.Vertical);
            JStickInputDegres = JStickInputRadians * Mathf.Rad2Deg;

            targetRotation = Quaternion.Euler(0, JStickInputDegres, 0);

            rotationDelta = Quaternion.Angle(targetRotation, playerBody.rotation);
            playerBody.rotation = Quaternion.Lerp(playerBody.rotation, targetRotation, 100f / rotationDelta * rotationSpeed * Time.deltaTime);
        }

        Debug.Log(FJoystick_L.Horizontal + " || " + FJoystick_L.Vertical);
    }
}
