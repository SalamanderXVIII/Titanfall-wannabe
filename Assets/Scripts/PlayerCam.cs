using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;
    public Transform camHolder;

    float xRotation;
    float yRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void SetFov(float endValue)
    {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }

    public void SetTilt(float tilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, tilt), 0.25f);
    }

	public void SetIncline(float tilt)
	{
		transform.DOLocalRotate(new Vector3(tilt, 0, 0), 0.25f);
	}

	public void CameraLandingShake()
    {
        GetComponent<Camera>().DOShakeRotation(0.1f, new Vector3(1, 0, 0), 1, 0, true, ShakeRandomnessMode.Harmonic);
    }
}
