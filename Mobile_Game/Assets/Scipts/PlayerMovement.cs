using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    float maxPlayerSpeed = 3f;
    float rotationSpeed = 10f;

    public Transform tempObj;
    public Transform playerBody;
    public CharacterController controller;
    public FixedJoystick FJoystick_L;
    public FixedJoystick FJoystick_R;

    public Animator animator;
    public Transform rotationBone;

    bool isMoving = false;
    bool isFighting = false;

    Vector3 moveDir = Vector3.zero;
    Quaternion targetPlayerRotation = Quaternion.identity;
    Quaternion targetTorsoRotation = Quaternion.identity;
    Quaternion targetTorsoRotationBone = Quaternion.identity;
    float playerSpeed = 0f;
    float JStickLInputDegres = 0f;
    float JStickLInputRadians = 0f;
    float JStickRInputDegres = 0f;
    float JStickRInputRadians = 0f;
    float rotationTargetDelta = 0;
    float BodyAngleDeltaNormalized = 0;
    float JStickLDistance = 0f;
    float JSticksDeltaTo90 = 0f;

    void Update()
    {

        #region vars

        //Pitagoras
        JStickLDistance = Mathf.Sqrt(FJoystick_L.Horizontal * FJoystick_L.Horizontal + FJoystick_L.Vertical * FJoystick_L.Vertical);

        moveDir = new Vector3(FJoystick_L.Horizontal, 0, FJoystick_L.Vertical).normalized;

        JStickLInputRadians = Mathf.Atan2(FJoystick_L.Horizontal, FJoystick_L.Vertical);
        JStickLInputDegres = JStickLInputRadians * Mathf.Rad2Deg;
        targetPlayerRotation = Quaternion.Euler(0, JStickLInputDegres, 0);

        JStickRInputRadians = Mathf.Atan2(FJoystick_R.Horizontal, FJoystick_R.Vertical);
        JStickRInputDegres = JStickRInputRadians * Mathf.Rad2Deg;
        targetTorsoRotation = Quaternion.Euler(0, JStickRInputDegres, 0);
        targetTorsoRotationBone = Quaternion.Euler(0, JStickRInputDegres + 90, 0);

        BodyAngleDeltaNormalized = (Quaternion.Angle(targetPlayerRotation, targetTorsoRotation)/-90)+1;

        JSticksDeltaTo90 = Mathf.DeltaAngle(JStickLInputDegres, JStickRInputDegres);
        if (JSticksDeltaTo90 > 90f)
            JSticksDeltaTo90 = -JSticksDeltaTo90 + 180;
        if (JSticksDeltaTo90 < -90f)
            JSticksDeltaTo90 = -JSticksDeltaTo90 - 180;
        JSticksDeltaTo90 = JSticksDeltaTo90 / 90;

        playerSpeed = maxPlayerSpeed * JStickLDistance;
        if (isFighting)
            playerSpeed = playerSpeed / 3;

        if (FJoystick_L.Horizontal == 0 && FJoystick_L.Vertical == 0)
            isMoving = false;
        else
            isMoving = true;

        if (FJoystick_R.Horizontal == 0 && FJoystick_R.Vertical == 0)
            isFighting = false;
        else
            isFighting = true;

        #endregion

        #region movement
        if (isMoving && !isFighting)
        {
            _ = controller.Move(moveDir * playerSpeed * Time.deltaTime);

            rotationTargetDelta = Quaternion.Angle(targetPlayerRotation, playerBody.rotation);
            playerBody.rotation = Quaternion.Lerp(playerBody.rotation, targetPlayerRotation, 100f / rotationTargetDelta * rotationSpeed * Time.deltaTime);
        }

        if (isMoving && isFighting)
        {
            moveDir = new Vector3(FJoystick_L.Horizontal, 0, FJoystick_L.Vertical).normalized;
            _ = controller.Move(moveDir * playerSpeed * Time.deltaTime);

            rotationBone.rotation = Quaternion.Lerp(rotationBone.rotation, targetTorsoRotationBone, 100f / rotationTargetDelta * rotationSpeed * Time.deltaTime);
            playerBody.rotation = Quaternion.Lerp(playerBody.rotation, targetTorsoRotation, 100f / rotationTargetDelta * rotationSpeed * Time.deltaTime);
        }
        
        if(!isFighting)
        {
            rotationBone.rotation = Quaternion.Euler(rotationBone.eulerAngles.x, playerBody.eulerAngles.y + 90, rotationBone.eulerAngles.z);
        }

        #endregion

        #region animations

        animator.SetFloat("Blend", JStickLDistance);
        animator.SetFloat("WalkBlendHoriz", -JSticksDeltaTo90);
        animator.SetFloat("WalkBlendVert", BodyAngleDeltaNormalized);

        if (isFighting)
            animator.SetFloat("Blend", Mathf.Clamp(animator.GetFloat("Blend"), 0f, 0.5f));


        if (JStickLDistance == 0 && animator.GetFloat("Blend") != 0)
        {
            animator.SetFloat("Blend", animator.GetFloat("Blend") - Time.deltaTime * maxPlayerSpeed);
        }


        if (isFighting)
            animator.SetBool("IsFighting", true);
        else
            animator.SetBool("IsFighting", false);

        #endregion
    }
}
