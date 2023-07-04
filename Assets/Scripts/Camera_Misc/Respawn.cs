using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    [SerializeField] private PlayerMovement pm;
    public Transform start;
    public Transform checkpoint;

    void Start()
    {
        checkpoint.position = start.position;
    }

    void Update()
    {
        if (Math.Abs(pm.PlayerObj.position.y) > 50)
            RespawnMethod();
    }

    public void RespawnMethod()
    {
		pm.transform.position = checkpoint.position;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.layer == 7)
        {
            RespawnMethod();
        }
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == 8)
		{
			checkpoint.position = other.gameObject.transform.position;
		}
	}
}
