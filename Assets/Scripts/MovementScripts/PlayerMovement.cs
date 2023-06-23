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
    public float slideSpeed;
    public float desiredMoveSpeed;
    [SerializeField] private float lastDesiredMoveSpeed;
    public bool sliding;
    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

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
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    Rigidbody rb;
    public PlayerCam cam;

    public enum MovementState
    {
        walking,
        sprinting,
        wallrunning,
        crouching,
        sliding,
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
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y >= 0)
                rb.AddForce(Vector3.down * 50f, ForceMode.Force);
        }

        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        if(!wallrunning) 
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

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Math.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while(time < difference)
        {
            if (sliding || state == MovementState.air)
                moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            else
				moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference*2);

			if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);
                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
            {
				time += Time.deltaTime * speedIncreaseMultiplier;
			}
            yield return null;
        }
        moveSpeed = desiredMoveSpeed;
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
            exitingSlope = false;
		}
	}

    public void CealingCollisionHandler()
    {
		cealing = Physics.Raycast(PlayerObj.position, Vector3.up, out cealingCheck, startYScale +0.3f, whatIsGround);
        if (cealing == false && !Input.GetKey(crouchKey))
        {
			PlayerObj.localScale = new Vector3(PlayerObj.localScale.x, startYScale, PlayerObj.localScale.z);
		}
	}

    private void StateHandler()
    {
        if (sliding)
        {
            state = MovementState.sliding;
            if (OnSlope() && rb.velocity.y < 0.1f)
                desiredMoveSpeed = slideSpeed;
            else
                desiredMoveSpeed = sprintSpeed;
        }

        else if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed;
        }

        else if (Input.GetKey(crouchKey) && !Input.GetKey(sprintKey))
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        else if(grounded && Input.GetKey(sprintKey) && !(state == MovementState.crouching))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }

        else if (grounded && (!Input.GetKey(crouchKey)) && !cealing)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        else if (!Input.GetKey(crouchKey) && !cealing)
        {
            state = MovementState.air;
        }

        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
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

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    void DragControl()
    {
		if (grounded)
			rb.drag = groundDrag;
		else
			rb.drag = 0;
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
