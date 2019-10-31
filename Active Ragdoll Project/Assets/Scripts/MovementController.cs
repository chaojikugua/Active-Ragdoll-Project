﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    [Header("Forces")]
    [SerializeField] private float forwardsForce = 10;
    [SerializeField] private float upwardForce = 2;
    [SerializeField] private float rotationTorque = 5;
    [SerializeField] private float jumpForce = 5;
    [SerializeField] private float jumpCharge;

    [Header("Bodyparts")]
    public Rigidbody[] thighs;
    public Rigidbody[] knees;
    public Rigidbody[] shins;
    public Rigidbody[] feet;

    [Header("Upright Raycast Mods")]
    [SerializeField] private float risingModifier = 20f;
    [SerializeField] private float sinkingModifier = 20f;
    [SerializeField] private float rayDistance = 1f;
    private bool isGrounded;

    [Header ("Misc")]
    public Upright[] uprightComponents;
    [SerializeField] private float pacelength = 2f;
    private bool walkingForward;
    private bool walkingBackward;
    private bool sprinting;
    private float paceTick = 0f;

    private float steppyTick = 0f;
    private int steppycounter = 0;

    private GameObject cameraObject;
    [SerializeField] private Rigidbody rb;


    private void Start()
    {
        //rb = GetComponent<Rigidbody>();
        cameraObject = Camera.main.gameObject;
    }
    private void Update()
    {
        InputCheck();
    }

    private void FixedUpdate()
    {
        steppyTick += Time.fixedDeltaTime;
        PaceSequence();
        GroundCheck();
    }

    public void InputCheck()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (!sprinting)
            {
                forwardsForce *= 2;
                sprinting = true;
            }

        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            if (sprinting)
            {
                forwardsForce /= 2;
                sprinting = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            walkingForward = true;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            walkingBackward = true;
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            //rb.constraints = RigidbodyConstraints.None;
            walkingForward = false;

        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            //rb.constraints = RigidbodyConstraints.None;
            walkingBackward = false;

        }
        if (Input.GetKey(KeyCode.A))
        {
            rb.constraints = RigidbodyConstraints.None;
            rb.AddRelativeTorque(Vector3.back * rotationTorque);
            SteppySteps('L');
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
        if (Input.GetKey(KeyCode.D))
        {
            rb.constraints = RigidbodyConstraints.None;
            rb.AddRelativeTorque(Vector3.forward * rotationTorque);
            SteppySteps('R');
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            //rb.constraints = RigidbodyConstraints.None;
            rb.AddTorque(Vector3.right * rotationTorque / 2);             //rotate leg parts and pelvis in opposite directions to make character crouch a bit
            thighs[0].AddTorque(Vector3.left * rotationTorque / 2);
            thighs[1].AddTorque(Vector3.left * rotationTorque / 2);
            shins[0].AddTorque(Vector3.right * rotationTorque / 2);
            shins[1].AddTorque(Vector3.right * rotationTorque / 2);
            feet[0].AddTorque(Vector3.left * rotationTorque / 2);
            feet[1].AddTorque(Vector3.left * rotationTorque / 2);
            jumpCharge += Time.deltaTime;
            jumpCharge = Mathf.Clamp(jumpCharge, 0f, 1.5f);
            ToggleUpright(false);
        }
        if (Input.GetKeyUp(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce * jumpCharge, ForceMode.Impulse);
            rb.AddForce(-rb.transform.up * jumpForce * jumpCharge, ForceMode.Impulse);
            feet[1].AddForce(-transform.up * jumpForce * jumpCharge/10, ForceMode.Impulse);
            feet[0].AddForce(-transform.up * jumpForce * jumpCharge/10, ForceMode.Impulse);
            jumpCharge = 0f;
            rb.constraints = RigidbodyConstraints.None;
            //ToggleUpright(true);
        }
        else if (Input.GetKeyUp(KeyCode.Space) && !isGrounded)
        {
            ToggleUpright(true);
            jumpCharge = 0f;
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleUpright();
        }
    }

    private void PaceSequence()
    {
        float magnitude = 1f;
        if (paceTick > pacelength * magnitude)
        {
            paceTick = 0;
        }

        #region Walk Forwards
        if (walkingForward)
        {
            paceTick += Time.fixedDeltaTime;
            if (paceTick > 0.9 * magnitude)
            {
                //pause between steps
            }
            else if (paceTick > 0.5 * magnitude)
            {
                //Adding forces from the pelvises "forward" and "upward"
                knees[1].AddForce(-transform.up * forwardsForce + transform.forward * upwardForce * 2, ForceMode.Impulse);
                feet[1].AddForce(-transform.up * forwardsForce + transform.forward * upwardForce, ForceMode.Impulse);
                feet[0].AddForce(-feet[0].transform.up * upwardForce, ForceMode.Impulse);

                //Debug.DrawRay(feet[1].transform.position, transform.forward, Color.red, 1f);
            }
            else if (paceTick > 0.4 * magnitude)
            {
                //pause between steps
            }
            else
            {
                knees[0].AddForce(-transform.up * forwardsForce + transform.forward * upwardForce * 2, ForceMode.Impulse);
                feet[0].AddForce(-transform.up * forwardsForce + transform.forward * upwardForce, ForceMode.Impulse);
                feet[1].AddForce(-feet[0].transform.up * upwardForce, ForceMode.Impulse);

                //Debug.DrawRay(feet[0].transform.position, transform.forward, Color.red, 1f);
            }
            rb.AddForce(rb.transform.forward * upwardForce, ForceMode.Impulse);
        }
        #endregion

        #region Backwards Walk
        if (walkingBackward)
        {
            paceTick += Time.fixedDeltaTime;
            if (paceTick > 0.8 * magnitude)
            {
                //pause between steps
            }
            else if (paceTick > 0.5 * magnitude)
            {
                //Adding forces from the pelvises "forward" and "upward"
                knees[1].AddForce(transform.up * forwardsForce + transform.forward * forwardsForce, ForceMode.Impulse);
                shins[1].AddForce(transform.forward * upwardForce, ForceMode.Impulse);
                //feet[0].AddForce(feet[0].transform.up * upwardForce, ForceMode.Impulse);

                //Debug.DrawRay(feet[1].transform.position, transform.forward, Color.red, 1f);
            }
            else if (paceTick > 0.3 * magnitude)
            {
                //pause between steps
            }
            else
            {
                knees[0].AddForce(transform.up * forwardsForce + transform.forward * forwardsForce, ForceMode.Impulse);
                shins[0].AddForce(transform.forward * upwardForce, ForceMode.Impulse);
                //feet[1].AddForce(feet[0].transform.up * upwardForce, ForceMode.Impulse);

                //Debug.DrawRay(feet[0].transform.position, transform.forward, Color.red, 1f);
            }
            //rb.AddForce(-rb.transform.forward * upwardForce, ForceMode.Impulse);
        }
        #endregion


    }

    private void OldMoveForward(int index)
    {
        knees[index].AddForce(cameraObject.transform.forward * forwardsForce + cameraObject.transform.up * forwardsForce, ForceMode.Impulse); 
        feet[index].AddForce(cameraObject.transform.forward * forwardsForce + cameraObject.transform.up * upwardForce, ForceMode.Impulse);
        Debug.DrawRay(feet[index].transform.position, cameraObject.transform.forward * forwardsForce, Color.red, 1f);
    }

    // This method needs improvement to make rotational steps clearer
    private void SteppySteps(char direction)
    {
        if (steppyTick > 0.20)
        {
            knees[steppycounter % 2].AddForce(cameraObject.transform.up * forwardsForce * 2 + cameraObject.transform.forward, ForceMode.Impulse);
            feet[steppycounter % 2].AddForce(-cameraObject.transform.up * upwardForce * 2, ForceMode.Impulse);
            switch (direction)
            {
                case 'R':
                    feet[steppycounter % 2].AddRelativeTorque(Vector3.forward * rotationTorque / 2, ForceMode.Impulse);
                    break;

                case 'L':
                    feet[steppycounter % 2].AddRelativeTorque(Vector3.back * rotationTorque / 2, ForceMode.Impulse);;
                    break;

                default:
                    Debug.LogError("INVALID ROTATION DIRECTION");
                    break;
            }
            steppycounter++;
            steppyTick = 0;
        }
        switch (direction)
        {
            case 'R':
                feet[1].AddForce(cameraObject.transform.forward * upwardForce + cameraObject.transform.up * forwardsForce);
                break;

            case 'L':
                feet[0].AddForce(cameraObject.transform.forward * upwardForce + cameraObject.transform.up * forwardsForce);
                break;

            default:
                Debug.LogError("INVALID ROTATION DIRECTION");
                break;
        }
    }

    private void ToggleUpright()
    {
        foreach (Upright bodypart in uprightComponents)
        {
            bodypart.holdUpright = !bodypart.holdUpright;
        }
    }

    private void ToggleUpright(bool mode)
    {
        //rb.constraints = RigidbodyConstraints.None;
        foreach (Upright bodypart in uprightComponents)
        {
            bodypart.holdUpright = mode;
        }
    }

    private void GroundCheck()
    {
        if (Physics.Raycast(rb.transform.position, -transform.forward, rayDistance, 1 << 9))
        {
            Debug.DrawRay(rb.transform.position, -transform.forward * rayDistance, Color.red);
            if (!isGrounded)
            {
                isGrounded = true;
                ToggleUpright(true);
            }
            rb.AddForce(Vector3.up * risingModifier, ForceMode.Force); //TODO use raycast normal to add forces instead of "up"
        }
        else
        {
            Debug.DrawRay(rb.transform.position, -transform.forward * rayDistance, Color.green);
            if (isGrounded)
            {
                isGrounded = false;
                //ToggleUpright(false);
            }
            rb.AddForce(Vector3.down * sinkingModifier, ForceMode.Force);
        }
    }
}
