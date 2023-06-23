using Microsoft.Win32.SafeHandles;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private PlayerMovement pm;
    public PlayerCam cam;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
	public float slideCooldownMax;
	public float slideYScale;
	private float slideCooldown;
	public float slideTimer;
	private bool slideReady = true;
	private float startYScale;

	[Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();

        startYScale = playerObj.localScale.y;
    }

    private void StartSlide()
    {
        pm.sliding = true;
        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 3f, ForceMode.Impulse);
        slideCooldown = slideCooldownMax;
		slideTimer = maxSlideTime;
        slideReady = false;
        cam.SetIncline(-5f);
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (!pm.OnSlope() || rb.velocity.y > -0.1f)
        {
			rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
			slideTimer -= Time.deltaTime;
		}

        else
        {
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }
		if (slideTimer < 0)
			StopSlide();
	}

    private void StopSlide()
    {
        pm.sliding = false;
		cam.SetIncline(0f);
		if (!Input.GetKey(slideKey) && pm.cealing == false)
        {
			playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
		}
        else
        {
            pm.state = PlayerMovement.MovementState.crouching;
			pm.desiredMoveSpeed = pm.crouchSpeed;
			cam.SetFov(80f);
        }
	}

    // Update is called once per frame
    void Update()
    {
		horizontalInput = Input.GetAxisRaw("Horizontal");
		verticalInput = Input.GetAxisRaw("Vertical");
		SlideInput();
	}
    private void FixedUpdate()
    {

    }

    void SlideInput()
    {
		if (Input.GetKey(slideKey) && (Input.GetKey(pm.sprintKey)) && (horizontalInput != 0 || verticalInput != 0) && (slideReady == true) && pm.grounded && (pm.state != PlayerMovement.MovementState.crouching))
			StartSlide();
		if (Input.GetKeyUp(slideKey) && pm.sliding)
			StopSlide();
		if (pm.sliding)
			SlidingMovement();
		else
			SlideReset();
	}

    void SlideReset()
    {
        if (slideReady == false)
		    slideCooldown -= Time.deltaTime;
		if (slideCooldown < 0)
        {
			slideReady = true;
            slideCooldown = slideCooldownMax;
		}
	}
}
