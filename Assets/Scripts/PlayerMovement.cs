using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float wallrunSpeed;
    public float groundDrag;
    public float horizontalInput;
    public float verticalInput;
    public MovementState state;
    public Transform orientation;
    public Transform PlayerObj;
    public Vector3 moveDirection;
    public bool wallrunning;
    public float currentSpeed;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public bool doubleJumpReady;
    public bool readyToJump;
    public float jumpCooldownMax;
    public RaycastHit floorShakeCheck;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;
    public RaycastHit cealingCheck;
    public bool cealing;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    Rigidbody rb;
    Sliding sliding;
    public PlayerCam cam;

    public enum MovementState
    {
        walking,
        sprinting,
        wallrunning,
        crouching,
        air
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        startYScale = transform.localScale.y;
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            Jump();
            readyToJump = false;
            doubleJumpReady = true;
            jumpCooldown = jumpCooldownMax;
        }
        if (Input.GetKeyDown(jumpKey) && !grounded && doubleJumpReady && wallrunning == false && (jumpCooldown == jumpCooldownMax)) 
        {
            Jump();
            doubleJumpReady = false;
        }

        CrouchCheck();

        if (Input.GetKeyDown(sprintKey) && !(state == MovementState.crouching))
        {
            cam.SetFov(90f);
        }

		if (Input.GetKeyUp(sprintKey) && !wallrunning)
		{
			cam.SetFov(80f);
		}
	}

    void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        MyInput();
        SpeedControl();
        StateHandler();
        DragControl();
        JumpReset();
        CealingCollisionHandler();
		currentSpeed = Math.Abs(rb.velocity.magnitude);
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        if (!wallrunning) 
            rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }

        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

	void JumpReset()
	{
        if (readyToJump == false)
		    jumpCooldown -= Time.deltaTime;
		if (jumpCooldown < 0)
		{
			readyToJump = true;
			jumpCooldown = jumpCooldownMax;
		}
	}
	private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    public void CealingCollisionHandler()
    {
		cealing = Physics.Raycast(PlayerObj.position, Vector3.up, out cealingCheck, startYScale, whatIsGround);
        if (cealing == false && !Input.GetKey(crouchKey))
        {
			PlayerObj.localScale = new Vector3(PlayerObj.localScale.x, startYScale, PlayerObj.localScale.z);
		}
	}

    private void StateHandler()
    {
        if (wallrunning)
        {
            state = MovementState.wallrunning;
            moveSpeed = wallrunSpeed;
        }

        else if (Input.GetKey(crouchKey) && !Input.GetKey(sprintKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }

        else if(grounded && Input.GetKey(sprintKey) && !(state == MovementState.crouching))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }

        else if (grounded && (!Input.GetKey(crouchKey)) && !cealing)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }

        else if (!Input.GetKey(crouchKey) && !cealing)
        {
            state = MovementState.air;
        }
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    void DragControl()
    {
		if (grounded)
			rb.drag = groundDrag;
		else
			rb.drag = 0;
	}

    //Код для динамического изменения FOV в зависимости от скорости передвижения.
	//void CheckSpeed()
	//{
	//	switch (currentSpeed)
	//	{
	//		case > highspeed:
	//			desiredFov = 85f;
	//			break;
	//		case <= highspeed when currentSpeed > slowspeed:
	//			desiredFov = 75f;
	//			break;
 //           default:
 //               desiredFov = 60f;
 //               break;
	//	}
	//}

    private void YPositionCheck()
    {

    }

	private void OnCollisionEnter(Collision collision)
	{
        if (collision.gameObject.layer == 6)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out floorShakeCheck, playerHeight))
				cam.CameraLandingShake();
        }
	}

    public void CrouchCheck()
    {
        if (Input.GetKeyDown(crouchKey) && !Input.GetKey(sprintKey) && !(state == MovementState.crouching))
        {
            PlayerObj.localScale = new Vector3(PlayerObj.localScale.x, crouchYScale, PlayerObj.localScale.z);
            rb.AddForce(Vector3.down * 3f, ForceMode.Impulse);
        }

        if ((Input.GetKeyUp(crouchKey)) && (cealing == false))
        {
            PlayerObj.localScale = new Vector3(PlayerObj.localScale.x, startYScale, PlayerObj.localScale.z);
        }
    }
}
