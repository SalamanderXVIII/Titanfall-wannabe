using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("Wallrunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float wallClimbSpeed;
    public float maxWallRunTime;
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    private float wallRunTime;
    [SerializeField] float maxWallCooldown;
    [SerializeField] float wallTimer;

    [Header("Exiting")]
    private bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;

    [Header("Input")]
    private float horizontalInput;
    private float verticalInput;
    public KeyCode upwardsRunKey = KeyCode.LeftShift;
    public KeyCode downwardsRunKey = KeyCode.LeftControl;
    private bool upwardsRunning;
    private bool downwardsRunning;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    private bool wallLeft;
    private bool wallRight;
    public bool IsCurrentWallViable;

    [Header("Gravity")]
    public bool useGravity;
    public float gravityCounterForce;

    [Header("References")]
    public Transform orientation;
    private PlayerMovement pm;
    private Rigidbody rb;
    public PlayerCam cam;
    public Transform playerObj;
    public GameObject recentWall;
    public GameObject currentWall;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        wallTimer = maxWallCooldown;
    }

	private void Update()
	{
		CheckForWall();
        StateMachine();
        WallReset();
	}

	private void FixedUpdate()
	{
        if(pm.wallrunning)
        {
            WallRunningMovement();
        }
	}

	private void CheckForWall()
    {
        wallRight = Physics.Raycast(playerObj.position, orientation.right, out rightWallhit, wallCheckDistance, whatIsWall);
        if (wallRight)
        {
			currentWall = rightWallhit.collider.gameObject;
		}
		wallLeft = Physics.Raycast(playerObj.position, -orientation.right, out leftWallhit, wallCheckDistance, whatIsWall);
		if (wallLeft)
        {
			currentWall = leftWallhit.collider.gameObject;
		}
	}

    private bool AboveGround()
    {
        return !Physics.Raycast(playerObj.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        upwardsRunning = Input.GetKey(upwardsRunKey);
        downwardsRunning = Input.GetKey(downwardsRunKey);

        if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround() && !exitingWall)
        {
            if(!pm.wallrunning && Input.GetKey(pm.sprintKey) && IsCurrentWallViable && (recentWall != currentWall))
            {
                StartWallRun();
			}

            if (wallRunTime > 0)
            {
                wallRunTime -= Time.deltaTime;
            }

            if ((wallRunTime <= 0) && (pm.wallrunning == true))
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }

            if (Input.GetKey(pm.jumpKey) && (recentWall != currentWall))
            {
                WallJump();
            }
        }

        else if (exitingWall)
        {
            if (pm.wallrunning)
            {
                StopWallRun();
            }
            if(exitWallTimer > 0)
            {
                exitWallTimer -= Time.deltaTime;
            }
            if(exitWallTimer <= 0)
            {
                exitingWall = false;
            }
        }

		else if (pm.wallrunning)
		{
			StopWallRun();
		}
	}

    private void StartWallRun()
    {
        pm.wallrunning = true;
        pm.doubleJumpReady = true;
        wallRunTime = maxWallRunTime;
		rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        cam.SetFov(95f);
        if (wallLeft)
            cam.SetTilt(-5f);
		if (wallRight)
			cam.SetTilt(5f);
	}

    private void WallRunningMovement()
    {
        rb.useGravity = useGravity;
        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);
        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
        {
            wallForward = -wallForward;
        }
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        if (upwardsRunning)
            rb.velocity = new Vector3(rb.velocity.x, wallClimbSpeed, rb.velocity.z);

		if (downwardsRunning)
			rb.velocity = new Vector3(rb.velocity.x, -wallClimbSpeed, rb.velocity.z);

		if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
        rb.AddForce(-wallNormal * 100, ForceMode.Force);

        if (useGravity)
        {
            rb.AddForce(playerObj.up * gravityCounterForce, ForceMode.Force);
        }
    }

    private void StopWallRun()
    {
        pm.wallrunning = false;
        if(Input.GetKey(pm.sprintKey))
        {
            cam.SetFov(90f);
        }
        else 
        {
			cam.SetFov(80f);
		}
        if (wallLeft)
        {
            rb.AddForce(playerObj.right * 5f, ForceMode.Impulse);
        }
		if (wallRight)
		{
			rb.AddForce(-playerObj.right * 5f, ForceMode.Impulse);
		}
		cam.SetTilt(0);
		recentWall = currentWall;
	}

	void WallReset()
	{
		if (recentWall == currentWall)
			wallTimer -= Time.deltaTime;
		if (wallTimer < 0)
		{
			recentWall = null;
			wallTimer = maxWallCooldown;
		}
	}

	private void WallJump()
    {
        exitingWall= true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;
        Vector3 forceToApply = playerObj.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }
}
