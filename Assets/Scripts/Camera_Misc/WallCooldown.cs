using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCooldown : MonoBehaviour
{
	[SerializeField] private bool IsWallViable;
	private WallRunning wallRunning;
	private CapsuleCollider capsuleCollider;
	public float maxWallCooldown;
    [SerializeField] private float WallTimer;


    // Start is called before the first frame update
    void Start()
    {
        WallTimer = maxWallCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        WallReset();
    }

	void WallReset()
	{
		if (IsWallViable == false)
			WallTimer -= Time.deltaTime;
		if (WallTimer < 0)
		{
			IsWallViable = true;
            WallTimer = maxWallCooldown;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "Player")
        {
            wallRunning.IsCurrentWallViable = IsWallViable;
        }
	}
	private void OnCollisionExit(Collision collision)
	{
		if (collision.gameObject.tag == "Player")
		{
			IsWallViable = false;
		}
	}
}
