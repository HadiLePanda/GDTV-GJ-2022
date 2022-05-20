// Created by the GameDev.tv team. Let us know what cool things you create using this! https://GameDev.tv

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    [SerializeField] float forwardSpeed;
    [SerializeField] float reverseSpeed;
    [SerializeField] float turnSpeed;

    [Tooltip("How far off the ground the collider is before it registers as being in the air")]
    [SerializeField] float distanceCheck = .2f;
    [SerializeField] LayerMask groundLayer;

    [Tooltip("Modify the effect of gravity without changing the cars mass")]
    [SerializeField] float gravityMultiplier = 5f;

    Rigidbody sphereRigidbody;
    Vector2 rawInput;
    float moveInput;
    float turnInput;
    bool isGrounded;

    void Awake()
    {
        sphereRigidbody = GetComponentInChildren<Rigidbody>();
    }

    void Start()
    {
        //This makes sure we don't have issues with the car body following the sphere
        sphereRigidbody.transform.parent = null;
    }

    void FixedUpdate()
    {
        //Make sure any objects you want to drive on are tagged with the "Ground" layer
        CheckIfGrounded();
        
        if (isGrounded)
        {
            //Make the car go
            sphereRigidbody.AddForce(transform.forward * moveInput, ForceMode.Acceleration);
        }
        else
        {
            //Make the car respond to gravity when it is not grounded
            sphereRigidbody.AddForce(Physics.gravity * gravityMultiplier);
        }
    }

    void Update()
    {
        TurnVehicle();
        MoveCarBodyWithSphere();
    }

    //Used by the input system
    void OnMove(InputValue value)
    {
        rawInput = value.Get<Vector2>();
        moveInput = rawInput.y;
        turnInput = rawInput.x;
        SetMoveSpeed();
    }

    void SetMoveSpeed()
    {
        if (moveInput > 0)
        {
            moveInput *= forwardSpeed;
        }
        else
        {
            moveInput *= reverseSpeed;
        }
    }

    void TurnVehicle()
    {
        float newRotation = turnInput * turnSpeed * Time.deltaTime;
        transform.Rotate(0, newRotation, 0, Space.World);
    }

    void MoveCarBodyWithSphere()
    {
        //With your car game object, be sure that the car body and sphere start in exactly the same position
        //or else things go wrong pretty quickly. The next line is making the car body follow the spehere.
        transform.position = sphereRigidbody.transform.position;
    }

    void CheckIfGrounded()
    {
        isGrounded = Physics.CheckSphere(transform.position, distanceCheck, groundLayer, QueryTriggerInteraction.Ignore);
    }
}
