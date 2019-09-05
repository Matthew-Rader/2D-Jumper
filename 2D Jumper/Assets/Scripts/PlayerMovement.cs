using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	[SerializeField] private CharacterController2D controller;
	[SerializeField] private float runSpeed = 40f;

	float horizontalMove = 0.0f;
	bool jumping = false;
	bool grabWall = false;

	// Update is called once per frame
	void Update()
    {
		horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

		if (Input.GetButtonDown("Jump")) {
			jumping = true;
		}

		grabWall = Input.GetAxis("LT") == 0 ? false : true;
    }

	void FixedUpdate()
	{
		//controller.Move(horizontalMove * Time.fixedDeltaTime, jumping, grabWall);
		jumping = false;
	}
}
